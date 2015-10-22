//  (C) Copyright Nekopanda 2015.
#include "stdafx.h"

#include "RecEventDB.h"

/*
void CRecEventDB::UpdateAutoAddLink(const EPG_AUTO_ADD_MAP& autoAddMap) {

	if (reqLinkUpdate == false) {
		return;
	}

	for (std::pair<DWORD, REC_EVENT_INFO*> entry : eventMap) {
		entry.second->autoAddIds.clear();
	}

	struct {
		const EPG_AUTO_ADD_DATA* autoAddData;
		void operator()(CEpgDBManager::SEARCH_RESULT_EVENT result) {
			REC_EVENT_INFO* eventInfo = (REC_EVENT_INFO*)result.info;
			eventInfo->autoAddIds.push_back(autoAddData->dataID);
		}
	} cb;

	//CoInitialize(NULL);
	{
		IRegExpPtr regExp;
		for (const std::pair<DWORD, EPG_AUTO_ADD_DATA>& entry : autoAddMap) {
			cb.autoAddData = &entry.second;
			CEpgDBManager::SearchEvent(const_cast<EPGDB_SEARCH_KEY_INFO*>(&entry.second.searchInfo), serviceMap, cb, regExp);
		}
	}
	//CoUninitialize();

	autoAddEventMap.clear();
	for (std::pair<DWORD, REC_EVENT_INFO*> entry : eventMap) {

		for (int dataId : entry.second->autoAddIds) {
			autoAddEventMap[dataId].eventList.push_back(entry.second);
		}
	}

	// autoAddEventMapを番組の日時順にソート
	for (auto entry : autoAddEventMap) {
		auto& eventList = entry.second.eventList;
		std::sort(eventList.begin(), eventList.end(), [](REC_EVENT_INFO* a, REC_EVENT_INFO* b) {
			return a->startTime64 < b->startTime64;
		});
	}

	reqLinkUpdate = false;
}

void CRecEventDB::AutoAddDeleted(int dataID)
{
	if (reqLinkUpdate) {
		// 差分更新の意味がないので
		return;
	}
	auto it = autoAddEventMap.find(dataID);
	if (it != autoAddEventMap.end()) {
		for (auto recInfo : it->second.eventList) {
			for (auto it = recInfo->autoAddIds.begin(); it != recInfo->autoAddIds.end(); ++it) {
				if (dataID == *it) {
					recInfo->autoAddIds.erase(it);
					break;
				}
			}
		}
	}
}

void CRecEventDB::AutoAddAdded(const EPG_AUTO_ADD_DATA& item)
{
	if (reqLinkUpdate) {
		// 差分更新の意味がないので
		return;
	}
	auto& ev = autoAddEventMap[item.dataID];
	ev.eventList.clear();

}

void CRecEventDB::AutoAddChanged(const EPG_AUTO_ADD_DATA& item)
{
	AutoAddDeleted(item.dataID);
	AutoAddAdded(item);
}
*/
