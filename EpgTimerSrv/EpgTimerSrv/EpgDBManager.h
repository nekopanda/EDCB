#pragma once

#include "../../Common/Util.h"
#include "../../Common/StructDef.h"
#include "../../Common/EpgDataCap3Def.h"
#include <memory>

#import "RegExp.tlb" no_namespace named_guids

class CEpgDBManager
{
public:
	typedef struct _SEARCH_RESULT_EVENT{
		EPGDB_EVENT_INFO* info;
		wstring findKey;
	}SEARCH_RESULT_EVENT;

	typedef struct _SEARCH_RESULT_EVENT_DATA{
		EPGDB_EVENT_INFO info;
		wstring findKey;
	}SEARCH_RESULT_EVENT_DATA;

public:
	CEpgDBManager(void);
	~CEpgDBManager(void);

	BOOL ReloadEpgData();

	BOOL IsLoadingData();

	BOOL IsInitialLoadingDataDone();

	BOOL CancelLoadData();

	BOOL SearchEpg(vector<EPGDB_SEARCH_KEY_INFO>* key, vector<std::unique_ptr<SEARCH_RESULT_EVENT_DATA>>* result);

	BOOL SearchEpg(vector<EPGDB_SEARCH_KEY_INFO>* key, void (*enumProc)(vector<SEARCH_RESULT_EVENT>*, void*), void* param);

	BOOL SearchEpgByKey(vector<EPGDB_SEARCH_KEY_INFO>* key, void (*enumProc)(vector<SEARCH_RESULT_EVENT>*, void*), void* param);

	BOOL GetServiceList(vector<EPGDB_SERVICE_INFO>* list);

	BOOL EnumEventInfo(LONGLONG serviceKey, vector<std::unique_ptr<EPGDB_EVENT_INFO>>* result);

	BOOL EnumEventInfo(LONGLONG serviceKey, void (*enumProc)(vector<EPGDB_EVENT_INFO*>*, void*), void* param);

	BOOL EnumEventAll(void (*enumProc)(vector<EPGDB_SERVICE_EVENT_INFO>*, void*), void* param);

	BOOL SearchEpg(
		WORD ONID,
		WORD TSID,
		WORD SID,
		WORD EventID,
		EPGDB_EVENT_INFO* result
		);

	BOOL SearchEpg(
		WORD ONID,
		WORD TSID,
		WORD SID,
		LONGLONG startTime,
		DWORD durationSec,
		EPGDB_EVENT_INFO* result
		);

	BOOL SearchServiceName(
		WORD ONID,
		WORD TSID,
		WORD SID,
		wstring& serviceName
		);

	static void ConvertSearchText(wstring& str);

	template <typename MAP, typename RESULT_CB>
	static void SearchEvent(EPGDB_SEARCH_KEY_INFO* key, const MAP& epgMap, RESULT_CB& onResultEvent, IRegExpPtr& regExp)
	{
		if (key == NULL) {
			return;
		}

		if (key->andKey.compare(0, 7, L"^!{999}") == 0) {
			//無効を示すキーワードが指定されているので検索しない
			return;
		}
		wstring andKey = key->andKey;
		BOOL caseFlag = FALSE;
		if (andKey.compare(0, 7, L"C!{999}") == 0) {
			//大小文字を区別するキーワードが指定されている
			andKey.erase(0, 7);
			caseFlag = TRUE;
		}
		// 処理は重いが、キーワード無し検索もしたい場合がある(特定局の検索や番組表など)ので条件を外す。
		//	if( andKey.size() == 0 && key->notKey.size() == 0 && key->contentList.size() == 0 && key->videoList.size() == 0 && key->audioList.size() == 0){
		//		//キーワードもジャンル指定もないので検索しない
		//		return ;
		//	}

		//キーワード分解
		vector<wstring> andKeyList;
		vector<wstring> notKeyList;

		if (key->regExpFlag == FALSE) {
			//正規表現ではないのでキーワードの分解
			wstring buff = L"";
			if (andKey.size() > 0) {
				wstring andBuff = andKey;
				Replace(andBuff, L"　", L" ");
				do {
					Separate(andBuff, L" ", buff, andBuff);
					ConvertSearchText(buff);
					if (buff.size() > 0) {
						andKeyList.push_back(buff);
					}
				} while (andBuff.size() != 0);
			}

			if (key->notKey.size() > 0) {
				wstring notBuff = key->notKey;
				Replace(notBuff, L"　", L" ");
				do {
					Separate(notBuff, L" ", buff, notBuff);
					ConvertSearchText(buff);
					if (buff.size() > 0) {
						notKeyList.push_back(buff);
					}
				} while (notBuff.size() != 0);
			}
		}
		else {
			if (andKey.size() > 0) {
				andKeyList.push_back(andKey);
				//旧い処理では対象を全角空白のまま比較していたため正規表現も全角のケースが多い。特別に置き換える
				Replace(andKeyList.back(), L"　", L" ");
			}
			if (key->notKey.size() > 0) {
				notKeyList.push_back(key->notKey);
				Replace(notKeyList.back(), L"　", L" ");
			}
		}

		//時間分解
		vector<TIME_SEARCH> timeList;
		for (size_t i = 0; i<key->dateList.size(); i++) {
			DWORD start = key->dateList[i].startHour * 60 + key->dateList[i].startMin;
			DWORD end = key->dateList[i].endHour * 60 + key->dateList[i].endMin;
			if (key->dateList[i].startDayOfWeek == key->dateList[i].endDayOfWeek) {
				if (start < end) {
					//通常
					TIME_SEARCH item;
					item.week = key->dateList[i].startDayOfWeek;
					item.start = start;
					item.end = end;
					timeList.push_back(item);
				}
				else {
					//1週間回す
					for (BYTE j = 0; j<7; j++) {
						if (j == key->dateList[i].startDayOfWeek) {
							TIME_SEARCH item1;
							item1.week = j;
							item1.start = 0;
							item1.end = end;
							timeList.push_back(item1);
							TIME_SEARCH item2;
							item2.week = j;
							item2.start = start;
							item2.end = 23 * 60 + 59;
							timeList.push_back(item2);
						}
						else {
							TIME_SEARCH item;
							item.week = j;
							item.start = 0;
							item.end = 23 * 60 + 59;
							timeList.push_back(item);
						}
					}
				}
			}
			else {
				BYTE chkWeek = key->dateList[i].startDayOfWeek;
				for (BYTE j = 0; j<7; j++) {
					if (chkWeek == key->dateList[i].startDayOfWeek) {
						TIME_SEARCH item;
						item.week = chkWeek;
						item.start = start;
						item.end = 23 * 60 + 59;
						timeList.push_back(item);
					}
					else if (chkWeek == key->dateList[i].endDayOfWeek) {
						TIME_SEARCH item;
						item.week = chkWeek;
						item.start = 0;
						item.end = end;
						timeList.push_back(item);
						break;
					}
					else {
						TIME_SEARCH item;
						item.week = chkWeek;
						item.start = 0;
						item.end = 23 * 60 + 59;
						timeList.push_back(item);
					}
					chkWeek++;
					if (chkWeek >= 7) {
						chkWeek = 0;
					}
				}
			}
		}

		//サービスごとに検索
		for (size_t i = 0; i<key->serviceList.size(); i++) {
			auto itrService = epgMap.find(key->serviceList[i]);
			if (itrService != epgMap.end()) {
				//サービス発見
				for (auto itrEvent_ = itrService->second->eventList.begin(); itrEvent_ != itrService->second->eventList.end(); itrEvent_++) {
					pair<WORD, EPGDB_EVENT_INFO*> autoEvent(std::make_pair((*itrEvent_)->event_id, *itrEvent_));
					pair<WORD, EPGDB_EVENT_INFO*>* itrEvent = &autoEvent;
					wstring matchKey = L"";
					if (key->freeCAFlag == 1) {
						//無料放送のみ
						if (itrEvent->second->freeCAFlag == 1) {
							//有料放送
							continue;
						}
					}
					else if (key->freeCAFlag == 2) {
						//有料放送のみ
						if (itrEvent->second->freeCAFlag == 0) {
							//無料放送
							continue;
						}
					}
					//ジャンル確認
					if (key->contentList.size() > 0) {
						//ジャンル指定あるのでジャンルで絞り込み
						if (itrEvent->second->contentInfo == NULL) {
							if (itrEvent->second->shortInfo == NULL) {
								//2つめのサービス？対象外とする
								continue;
							}
							//ジャンル情報ない
							BOOL findNo = FALSE;
							for (size_t j = 0; j<key->contentList.size(); j++) {
								if (key->contentList[j].content_nibble_level_1 == 0xFF &&
									key->contentList[j].content_nibble_level_2 == 0xFF
									) {
									//ジャンルなしの指定あり
									findNo = TRUE;
									break;
								}
							}
							if (key->notContetFlag == 0) {
								if (findNo == FALSE) {
									continue;
								}
							}
							else {
								//NOT条件扱い
								if (findNo == TRUE) {
									continue;
								}
							}
						}
						else {
							BOOL equal = IsEqualContent(&(key->contentList), &(itrEvent->second->contentInfo->nibbleList));
							if (key->notContetFlag == 0) {
								if (equal == FALSE) {
									//ジャンル違うので対象外
									continue;
								}
							}
							else {
								//NOT条件扱い
								if (equal == TRUE) {
									continue;
								}
							}
						}
					}

					//映像確認
					if (key->videoList.size() > 0) {
						if (itrEvent->second->componentInfo == NULL) {
							continue;
						}
						BOOL findContent = FALSE;
						WORD type = ((WORD)itrEvent->second->componentInfo->stream_content) << 8 | itrEvent->second->componentInfo->component_type;
						for (size_t j = 0; j<key->videoList.size(); j++) {
							if (type == key->videoList[j]) {
								findContent = TRUE;
								break;
							}
						}
						if (findContent == FALSE) {
							continue;
						}
					}

					//音声確認
					if (key->audioList.size() > 0) {
						if (itrEvent->second->audioInfo == NULL) {
							continue;
						}
						BOOL findContent = FALSE;
						for (size_t j = 0; j<itrEvent->second->audioInfo->componentList.size(); j++) {
							WORD type = ((WORD)itrEvent->second->audioInfo->componentList[j].stream_content) << 8 | itrEvent->second->audioInfo->componentList[j].component_type;
							for (size_t k = 0; k<key->audioList.size(); k++) {
								if (type == key->audioList[k]) {
									findContent = TRUE;
									break;
								}
							}
						}
						if (findContent == FALSE) {
							continue;
						}
					}

					//時間確認
					if (timeList.size() > 0) {
						if (itrEvent->second->StartTimeFlag == FALSE) {
							//開始時間不明なので対象外
							continue;
						}
						BOOL inTime = IsInDateTime(&timeList, itrEvent->second->start_time);
						if (key->notDateFlag == 0) {
							if (inTime == FALSE) {
								//時間範囲外なので対象外
								continue;
							}
						}
						else {
							//NOT条件扱い
							if (inTime == TRUE) {
								continue;
							}
						}
					}

					//番組長で絞り込み
					if (key->chkDurationMin != 0) {
						if ((LONGLONG)key->chkDurationMin * 60 > itrEvent->second->durationSec) {
							continue;
						}
					}
					if (key->chkDurationMax != 0) {
						if ((LONGLONG)key->chkDurationMax * 60 < itrEvent->second->durationSec) {
							continue;
						}
					}

					//キーワード確認
					if (notKeyList.size() != 0) {
						if (IsFindKeyword(key->regExpFlag, regExp, key->titleOnlyFlag, caseFlag, &notKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, FALSE) == TRUE) {
							//notキーワード見つかったので対象外
							continue;
						}

						//if( key->regExpFlag == FALSE && key->aimaiFlag == 1){
						//	//あいまい検索
						//	if( IsFindLikeKeyword(key->titleOnlyFlag, &notKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, FALSE) == TRUE ){
						//		//notキーワード見つかったので対象外
						//		continue;
						//	}
						//}else{
						//	if( IsFindKeyword(key->regExpFlag, key->titleOnlyFlag, &notKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, FALSE) == TRUE ){
						//		//notキーワード見つかったので対象外
						//		continue;
						//	}
						//}
					}
					if (andKeyList.size() != 0) {
						//if( IsFindKeyword(key->regExpFlag, key->titleOnlyFlag, &andKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, TRUE) == FALSE ){
						//	//andキーワード見つからなかったので対象外
						//	continue;
						//}
						if (key->regExpFlag == FALSE && key->aimaiFlag == 1) {
							//あいまい検索
							if (IsFindLikeKeyword(key->titleOnlyFlag, caseFlag, &andKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, TRUE, &matchKey) == FALSE) {
								//andキーワード見つからなかったので対象外
								continue;
							}
						}
						else {
							if (IsFindKeyword(key->regExpFlag, regExp, key->titleOnlyFlag, caseFlag, &andKeyList, itrEvent->second->shortInfo, itrEvent->second->extInfo, TRUE, &matchKey) == FALSE) {
								//andキーワード見つからなかったので対象外
								continue;
							}
						}
					}

					SEARCH_RESULT_EVENT addItem;
					addItem.findKey = matchKey;
					addItem.info = itrEvent->second;

					onResultEvent(addItem);
				}
			}
		}
		/*
		for( itrService = this->epgMap.begin(); itrService != this->epgMap.end(); itrService++ ){
		map<WORD, EPGDB_EVENT_INFO*>::iterator itrEvent;
		for( itrEvent = itrService->second->eventMap.begin(); itrEvent != itrService->second->eventMap.end(); itrEvent++ ){
		if( itrEvent->second->shortInfo != NULL ){
		if( itrEvent->second->shortInfo->search_event_name.find(key->andKey) != string::npos ){
		ULONGLONG mapKey = _Create64Key2(
		itrEvent->second->original_network_id,
		itrEvent->second->transport_stream_id,
		itrEvent->second->service_id,
		itrEvent->second->event_id);

		resultMap->insert(pair<ULONGLONG, EPGDB_EVENT_INFO*>(mapKey, itrEvent->second));
		}
		}
		}
		}
		*/
	}
protected:
	CRITICAL_SECTION epgMapLock;

	HANDLE loadThread;
	BOOL loadStop;
	BOOL initialLoadDone;

	typedef struct _EPGDB_SERVICE_DATA{
		EPGDB_SERVICE_INFO serviceInfo;
		vector<EPGDB_EVENT_INFO*> eventList;
		EPGDB_EVENT_INFO* eventArray;
		_EPGDB_SERVICE_DATA(void){
			eventArray = NULL;
		}
		~_EPGDB_SERVICE_DATA(void){
			delete[] eventArray;
		};
		static bool CompareEventInfo(const EPGDB_EVENT_INFO* l, const EPGDB_EVENT_INFO* r){
			return l->event_id < r->event_id;
		}
	}EPGDB_SERVICE_DATA;

	typedef struct _TIME_SEARCH{
		BYTE week;
		DWORD start;
		DWORD end;
	}TIME_SEARCH;

	map<LONGLONG, EPGDB_SERVICE_DATA*> epgMap;
protected:
	static BOOL ConvertEpgInfo(EPGDB_SERVICE_INFO* service, EPG_EVENT_INFO* src, EPGDB_EVENT_INFO* dest);
	static BOOL CALLBACK EnumEpgInfoListProc(DWORD epgInfoListSize, EPG_EVENT_INFO* epgInfoList, LPVOID param);
	void ClearEpgData();
	static UINT WINAPI LoadThread_(LPVOID param);

	UINT LoadThread();
	void SearchEvent(EPGDB_SEARCH_KEY_INFO* key, map<ULONGLONG, SEARCH_RESULT_EVENT>* resultMap, IRegExpPtr& regExp);
	static BOOL IsEqualContent(vector<EPGDB_CONTENT_DATA>* searchKey, vector<EPGDB_CONTENT_DATA>* eventData);
	static BOOL IsInDateTime(vector<TIME_SEARCH>* timeList, SYSTEMTIME startTime);
	static BOOL IsFindKeyword(BOOL regExpFlag, IRegExpPtr& regExp, BOOL titleOnlyFlag, BOOL caseFlag, vector<wstring>* keyList, EPGDB_SHORT_EVENT_INFO* shortInfo, EPGDB_EXTENDED_EVENT_INFO* extInfo, BOOL andMode, wstring* findKey = NULL);
	static BOOL IsFindLikeKeyword(BOOL titleOnlyFlag, BOOL caseFlag, vector<wstring>* keyList, EPGDB_SHORT_EVENT_INFO* shortInfo, EPGDB_EXTENDED_EVENT_INFO* extInfo, BOOL andMode, wstring* findKey = NULL);

};

