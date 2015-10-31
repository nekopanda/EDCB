#ifndef __UTIL_H__
#define __UTIL_H__

#include <string>
#include <map>
#include <set>
#include <vector>
#include <algorithm>
using std::string;
using std::wstring;
using std::pair;
using std::map;
using std::multimap;
using std::vector;

#include <TCHAR.h>
#include <windows.h>


template<class T> inline void SAFE_DELETE(T*& p) { delete p; p = NULL; }
template<class T> inline void SAFE_DELETE_ARRAY(T*& p) { delete[] p; p = NULL; }

HANDLE _CreateDirectoryAndFile( LPCTSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpsa, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile );
BOOL _CreateDirectory( LPCTSTR lpPathName );
//ボリュームのマウントを考慮して実ドライブの空きを取得する
BOOL _GetDiskFreeSpaceEx(
  LPCTSTR lpDirectoryName,                 // ディレクトリ名
  PULARGE_INTEGER lpFreeBytesAvailable,    // 呼び出し側が利用できるバイト数
  PULARGE_INTEGER lpTotalNumberOfBytes,    // ディスク全体のバイト数
  PULARGE_INTEGER lpTotalNumberOfFreeBytes // ディスク全体の空きバイト数
);
__int64 GetFileLastModified(const wstring& path);
bool GetFileExist(const wstring& path);
bool GetDirectoryExist(const wstring& path);

void _OutputDebugString(const TCHAR *pOutputString, ...);
void GetLastErrMsg(DWORD err, wstring& msg);

LONG FilterException(struct _EXCEPTION_POINTERS * ExceptionInfo);

HANDLE FileOpenRead(const wstring& filepath);

// ディレクトリのファイルリストをキャッシュして存在確認やファイルオープン操作を高速化
// 使い方
//  1. 開く予定のファイルをCachePath()で登録（これはあくまでキャッシュするためのヒントなので実際に開くのと違ってもよい）
//  2. UpdateDirectoryInfo()でディレクトリ情報を更新
//  3. Open() or Exist() でファイルにアクセス
class CDirectoryCache {
public:
	void CachePath(const wstring& filepath);
	void UpdateDirectoryInfo();
	HANDLE Open(const wstring& filepath, bool withCache = true);
	bool Exist(const wstring& filepath, bool withCache = true);
private:
	struct DirectoryInfo {
		int64_t lastModified;
		bool exist;
		std::set<wstring> files;

		DirectoryInfo()
			: lastModified(0)
			, exist(false)
		{ }
	};
	std::map<wstring, DirectoryInfo> directoryMap;
};


#endif
