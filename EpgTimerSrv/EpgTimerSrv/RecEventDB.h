//  (C) Copyright Nekopanda 2015.
#pragma once

#include <vector>
#include <map>

#include "../../Common/EpgTimerUtil.h"

#include "LightTsUtils.h"
#include "EpgDBManager.h"

struct REC_EVENT_INFO : public EPGDB_EVENT_INFO {
	DWORD recFileId;
	DWORD loadErrorCount;
	DWORD rawDataLen;
	BYTE* rawDataPtr;

	int64_t startTime64;

	REC_EVENT_INFO()
		: EPGDB_EVENT_INFO()
		, recFileId(0)
		, loadErrorCount(0)
		, rawDataLen(0)
		, rawDataPtr(NULL)
		, startTime64(0)
	{ }
};

struct REC_EVENT_SERVICE_DATA {
	std::vector<REC_EVENT_INFO*> eventList;

	const REC_EVENT_SERVICE_DATA* operator->() const {
		return this;
	}
};

// 録画ファイルの番組情報データベース
class CRecEventDB {
	//
public:

	typedef std::map<DWORD, REC_FILE_INFO> REC_INFO_MAP;
	typedef std::map<DWORD, REC_EVENT_INFO*> REC_EVENT_MAP;
	typedef std::map<LONGLONG, REC_EVENT_SERVICE_DATA> SERVICE_EVENT_MAP;

	bool Load(const std::wstring& filePath, const REC_INFO_MAP& recFiles) {
		OutputDebugString(L"Start Load RecEventDB\r\n");
		DWORD time = GetTickCount();

		Clear();

		this->filePath = filePath;
		if (LoadFile() == false) {
			OutputDebugString(L"RecEventDBの読み込みに失敗しました\r\n");
			return false;
		}
		LoadRecFiles(recFiles);
		RecreateServiceMap();

		_OutputDebugString(L"End Load RecEventDB %dmsec\r\n", GetTickCount() - time);
		return true;
	}

	void AddRecInfo(const REC_FILE_INFO& item) {
		REC_EVENT_INFO* eventInfo = CreateNew(item.id);
		eventInfo->recFileId = item.id;
		CMediaData* buffer = LoadTs(eventInfo, item.recFilePath, item.serviceID);
		if (buffer != NULL) {
			buffers.push_back(buffer);
		}
	}

	bool Save() {
		if (filePath.empty()) {
			return true;
		}
		if (buffers.size() == 0) {
			// 新しいデータがないので書き込む必要がない
			return true;
		}

		OutputDebugString(L"Start Save RecEventDB\r\n");
		DWORD time = GetTickCount();

		// ファイルを開く
		HANDLE hFile = _CreateDirectoryAndFile(this->filePath.c_str(), GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == INVALID_HANDLE_VALUE) {
			_OutputDebugString(L"CRecEventDB::Save(): Error: Cannot open file: %s\r\n", this->filePath.c_str());
			return false;
		}

		// バッファに書き込む
		{
			CMediaData buffer;

			DWORD dwVersion = 1;
			buffer.AddData((BYTE*)&dwVersion, 4);

			for (std::pair<DWORD, REC_EVENT_INFO*> entry : eventMap) {
				buffer.AddData((BYTE*)&entry.first, 4);
				buffer.AddData((BYTE*)&entry.second->loadErrorCount, 4);
				buffer.AddData((BYTE*)&entry.second->rawDataLen, 4);
				buffer.AddData(entry.second->rawDataPtr, entry.second->rawDataLen);
			}

			// ファイルに書き込む
			DWORD dwWrite = 0;
			WriteFile(hFile, buffer.GetData(), buffer.GetSize(), &dwWrite, NULL);
			CloseHandle(hFile);

			if (dwWrite != buffer.GetSize()) {
				OutputDebugString(L"Error? CRecEventDB::Save() \r\n");
				return false;
			}

			// 読み込み直す
			Clear();
			rawDataBuffer = std::move(buffer);
		}

		LoadRawData();
		RecreateServiceMap();

		_OutputDebugString(L"End Save RecEventDB %dmsec\r\n", GetTickCount() - time);

		return false;
	}

	void Clear() {
		for (std::pair<DWORD, REC_EVENT_INFO*> entry : eventMap) {
			SAFE_DELETE(entry.second);
		}
		eventMap.clear();
		serviceMap.clear();

		for (CMediaData* buffer : buffers) {
			delete buffer;
		}
		buffers.clear();
	}
	
	// 録画ファイルを検索　マッチした録画ファイルのIDを返す
	vector<DWORD> SearchRecFile(const EPGDB_SEARCH_KEY_INFO& item) {
		vector<REC_EVENT_INFO*> list;
		IRegExpPtr regExp;
		
		// 検索
		EPGDB_SEARCH_KEY_INFO key = item;
		if (key.andKey.compare(0, 7, L"^!{999}") == 0) {
			//無効を示すキーワードを削除
			key.andKey.erase(0, 7);
		}
		CEpgDBManager::SearchEvent(&key, serviceMap, [&](CEpgDBManager::SEARCH_RESULT_EVENT result) {
			REC_EVENT_INFO* eventInfo = (REC_EVENT_INFO*)result.info;
			list.push_back(eventInfo);
		}, regExp);

		// 日時順にソート
		std::sort(list.begin(), list.end(), [](REC_EVENT_INFO* a, REC_EVENT_INFO* b) {
			return a->startTime64 < b->startTime64;
		});

		// IDリストに変換
		vector<DWORD> ret;
		ret.reserve(list.size());
		for (REC_EVENT_INFO* info : list) {
			ret.push_back(info->recFileId);
		}

		return ret;
	}

	bool HasEpgInfo(DWORD id) {
		auto it = eventMap.find(id);
		if (it != eventMap.end()) {
			return (it->second->rawDataLen > 0);
		}
		return false;
	}

	//const SERVICE_EVENT_MAP& GetServiceMap() const { return serviceMap; }
	//const REC_EVENT_MAP& GetEventMap() const { return eventMap; }

private:
	wstring filePath;
	REC_EVENT_MAP eventMap;
	SERVICE_EVENT_MAP serviceMap;

	CMediaData rawDataBuffer;

	std::vector<CMediaData*> buffers;

	CTsPacketParser packetParser;
	CEitDetector eitDetector;

	class PacketWriter : public CTsFilter{
	public:
		virtual void StorePacket(CTsPacket* pPacket) {
			buffer->AddData(pPacket->GetData(), pPacket->GetSize());
		}
		CMediaData* buffer;
	};

	bool LoadFile() {
		if (filePath.empty()) {
			return false;
		}
		HANDLE hFile = CreateFile(filePath.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == INVALID_HANDLE_VALUE) {
			if (GetLastError() == ERROR_FILE_NOT_FOUND) {
				return true;
			}
			OutputDebugString(L"CRecEventDB::Load(): Error: Cannot open file\r\n");
			return false;
		}
		DWORD dwFileSize = GetFileSize(hFile, NULL);
		if (dwFileSize != INVALID_FILE_SIZE && dwFileSize != 0) {
			rawDataBuffer.SetSize(dwFileSize);
			DWORD dwRead;
			if (ReadFile(hFile, rawDataBuffer.GetData(), dwFileSize, &dwRead, NULL) && dwRead != 0) {
				LoadRawData();
				CloseHandle(hFile);
				return true;
			}
			rawDataBuffer.ClearBuffer();
		}
		CloseHandle(hFile);
		return false;
	}

	bool LoadEventInfo(REC_EVENT_INFO* eventInfo, BYTE* cur, BYTE* end) {
		if (cur + 4 > end) return false;
		DWORD patLen = *(DWORD*)cur; cur += 4;
		BYTE* patData = cur; cur += patLen;
		if (cur + 4 > end) return false;
		DWORD eitLen = *(DWORD*)cur; cur += 4;
		BYTE* eitData = cur; cur += patLen;
		if (cur > end) return false;

		CMediaData eitSectionData(eitData, eitLen);
		CSiSectionEIT eit(&eitSectionData);
		if (eit.Check() && eit.Parse()) {
			eitDetector.GetEITConverter()->Feed(eit, eventInfo);
			return true;
		}
		return false;
	}

	void LoadRawData() {
		if (eitDetector.Initialize() == false) return;

		BYTE* cur = rawDataBuffer.GetData();
		BYTE* end = cur + rawDataBuffer.GetSize();

		if (cur + 4 > end) return;
		DWORD dwVersion = *(DWORD*)(cur + 0);
		cur += 4;

		if (dwVersion != 1) {
			// 未対応バージョン
			return;
		}

		while (cur + 12 <= end) {
			DWORD fileId = *(DWORD*)(cur + 0);
			DWORD loadErrorCount = *(DWORD*)(cur + 4);
			DWORD dataLen = *(DWORD*)(cur + 8);
			cur += 12;

			REC_EVENT_INFO* eventInfo = CreateNew(fileId);
			eventInfo->recFileId = fileId;
			eventInfo->loadErrorCount = loadErrorCount;
			eventInfo->rawDataLen = dataLen;
			eventInfo->rawDataPtr = cur;
			if (dataLen > 0) {
				if (cur + dataLen > end) {
					_OutputDebugString(L"RecEventDB読み込み中サイズエラー");
				}
				else {
					if (LoadEventInfo(eventInfo, cur, end) == false) {
						_OutputDebugString(L"RecEventDB読み込み中サイズエラー");
					}
					cur += dataLen;
				}
			}
		}
	}

	CMediaData* LoadTs(REC_EVENT_INFO* eventInfo, const std::wstring& recFilePath, int serviceId) {
		if (eitDetector.Initialize() == false) return NULL;

		HANDLE hFile = CreateFile(recFilePath.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
		if (hFile == INVALID_HANDLE_VALUE) {
			return NULL;
		}

		packetParser.SetOutputFilter(&eitDetector);
		packetParser.Reset();
		eitDetector.Reset();
		eitDetector.SetTarget(eventInfo, serviceId);

		CMediaData buffer;
		buffer.SetSize(256 * 1024);

		// 中間位置までシーク
		DWORD sizeHigh = 0;
		DWORD sizeLow = GetFileSize(hFile, &sizeHigh);
		int64_t mid = ((int64_t(sizeHigh) << 32) + sizeLow) / 2;
		LONG distLow = (LONG)mid;
		LONG distHidh = (LONG)(mid >> 32);
		SetFilePointer(hFile, distLow, &distHidh, FILE_BEGIN);

		DWORD dwRead = 0;
		DWORD dwTotalRead = 0;
		while (ReadFile(hFile, buffer.GetData(), buffer.GetSize(), &dwRead, NULL) && dwRead > 0) {
			dwTotalRead += dwRead;
			packetParser.InputRawData(buffer.GetData(), dwRead);
			if (eitDetector.IsDetected()) {
				CloseHandle(hFile);

				CMediaData* buffer = new CMediaData();

				CMediaData* patData = eitDetector.GetPATData();
				CMediaData* eitData = eitDetector.GetEITData();
				DWORD patSize = patData->GetSize();
				DWORD eitSize = eitData->GetSize();
				buffer->AddData((BYTE*)&patSize, 4);
				buffer->AddData(*patData);
				buffer->AddData((BYTE*)&eitSize, 4);
				buffer->AddData(*eitData);

				eventInfo->rawDataLen = buffer->GetSize();
				eventInfo->rawDataPtr = buffer->GetData();
				eventInfo->startTime64 = _SYSTEMTIMEtoINT64(&eventInfo->start_time);

				_OutputDebugString(L"RecEventEB LoadTs %s : %d KB\r\n", recFilePath.c_str(), dwTotalRead / 1024);
				return buffer;
			}
			if (dwTotalRead > 128 * 1024 * 1024) { // 128MB以上読んだ
				break;
			}
		}

		CloseHandle(hFile);
		return NULL;
	}

	void LoadRecFiles(const REC_INFO_MAP& recFiles) {
		for (const auto& recInfo : recFiles) {
			DWORD fileId = recInfo.first;
			WORD serviceId = recInfo.second.serviceID;
			REC_EVENT_INFO* eventInfo = GetOrNew(fileId);
			eventInfo->recFileId = fileId;

			if (eventInfo->rawDataLen == 0 && eventInfo->loadErrorCount <= 4) {
				CMediaData* buffer = LoadTs(eventInfo, recInfo.second.recFilePath, serviceId);
				if (buffer != NULL) {
					buffers.push_back(buffer);
				}
				else {
					eventInfo->loadErrorCount++;
				}
			}
		}
	}

	void RecreateServiceMap() {
		serviceMap.clear();
		for (std::pair<DWORD, REC_EVENT_INFO*> entry : eventMap) {
			REC_EVENT_INFO* eventInfo = entry.second;
			if (eventInfo->rawDataLen > 0) {
				// 有効なデータがあるときだけ
				LONGLONG key = _Create64Key(eventInfo->original_network_id, eventInfo->transport_stream_id, eventInfo->service_id);
				serviceMap[key].eventList.push_back(eventInfo);
			}
		}
	}

	REC_EVENT_INFO* GetOrNew(DWORD fileId) {
		auto it = eventMap.find(fileId);
		if (it == eventMap.end()) {
			REC_EVENT_INFO* data = new REC_EVENT_INFO();
			eventMap[fileId] = data;
			return data;
		}
		return it->second;
	}

	REC_EVENT_INFO* CreateNew(DWORD fileId) {
		auto it = eventMap.find(fileId);
		if (it != eventMap.end()) {
			delete it->second;
		}
		return eventMap[fileId] = new REC_EVENT_INFO();
	}
};
