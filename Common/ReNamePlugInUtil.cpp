#include "StdAfx.h"
#include "ReNamePlugInUtil.h"

volatile unsigned int CReNamePlugInUtil::_refCount = 0;
CReNamePlugInUtil* CReNamePlugInUtil::_instance = NULL;

CReNamePlugInUtil::CReNamePlugInUtil()
{
	InterlockedIncrement(&_refCount);
}

CReNamePlugInUtil::~CReNamePlugInUtil()
{
	ReleaseInstance();
}

CReNamePlugInUtil* CReNamePlugInUtil::GetInstance()
{
	if (_instance == NULL)
	{
		_instance = new CReNamePlugInUtil;
		_instance->pluginMap.clear();
	}
	else
	{
		InterlockedIncrement(&_refCount);
	}
	return _instance;
}

void CReNamePlugInUtil::ReleaseInstance()
{
	if (_instance && _refCount == 1)
	{
		CReNamePlugInUtil *p = _instance;
		_instance = NULL;

		for (auto itr = p->pluginMap.begin(); itr != p->pluginMap.end(); itr++)
		{
			FreeLibrary(itr->second->hModule);
			delete itr->second;
		}
		p->pluginMap.clear();
		delete p;
	}
	else
	{
		InterlockedDecrement(&_refCount);
	}
}

const CReNamePlugInUtil::PLUGIN_INFO* CReNamePlugInUtil::FindPlugin(
	const WCHAR* dllPattern,
	const WCHAR* dllFolder
	)
{
	wstring pattern = dllPattern;
	wstring pluginName = pattern.substr(0, pattern.find('?'));

	auto itr = pluginMap.find(pluginName.c_str());
	if (itr != pluginMap.end())
	{
		return &*itr->second;
	}
	else
	{
		CReNamePlugInUtil::PLUGIN_INFO *plugin = new CReNamePlugInUtil::PLUGIN_INFO;
		plugin->hModule = LoadLibrary((dllFolder + pluginName).c_str());
		if (plugin->hModule == NULL)
		{
			OutputDebugString(L"DLL‚Ìƒ[ƒh‚ÉŽ¸”s‚µ‚Ü‚µ‚½\r\n");
		}
		else
		{
			plugin->pfnConvertRecName = (ConvertRecNameRNP)GetProcAddress(plugin->hModule, "ConvertRecName");
			plugin->pfnConvertRecName2 = (ConvertRecName2RNP)GetProcAddress(plugin->hModule, "ConvertRecName2");
			plugin->pfnConvertRecName3 = (ConvertRecName3RNP)GetProcAddress(plugin->hModule, "ConvertRecName3");
		}
		pluginMap.insert(pair<const wstring, const CReNamePlugInUtil::PLUGIN_INFO*>(pluginName, plugin));
		return plugin;
	}
}

BOOL CReNamePlugInUtil::ConvertRecName3(
	PLUGIN_RESERVE_INFO* info,
	const WCHAR* dllPattern,
	const WCHAR* dllFolder,
	WCHAR* recName,
	DWORD* recNamesize
	)
{
	CReNamePlugInUtil *pluginUtil = CReNamePlugInUtil::GetInstance();
	const CReNamePlugInUtil::PLUGIN_INFO* plugin = pluginUtil->FindPlugin(dllPattern, dllFolder);

	BOOL ret = FALSE;
	if (plugin->hModule != NULL) 
	{
		wstring pattern = dllPattern;
		pattern.erase(0, pattern.find('?'));
		pattern.erase(0, 1);
		if (plugin->pfnConvertRecName3) {
			ret = plugin->pfnConvertRecName3(info, pattern.empty() ? NULL : pattern.c_str(), recName, recNamesize);
		}
		else if (plugin->pfnConvertRecName2) {
			ret = plugin->pfnConvertRecName2(info, info->epgInfo, recName, recNamesize);
		}
		else if (plugin->pfnConvertRecName) {
			ret = plugin->pfnConvertRecName(info, recName, recNamesize);
		}
		else {
			OutputDebugString(L"ConvertRecName ‚Ì GetProcAddress ‚ÉŽ¸”s\r\n");
		}
	}

	pluginUtil->ReleaseInstance();
	return ret;
}

