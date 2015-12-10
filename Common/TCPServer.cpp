#include "StdAfx.h"
#include "TCPServer.h"
#include <process.h>
#include "ErrDef.h"
#include "CtrlCmdUtil.h"
#include "../../Common/PathUtil.h"
#include <wincrypt.h>
#pragma comment(lib, "advapi32.lib")

CTCPServer::CTCPServer(void)
{
	m_pCmdProc = NULL;
	m_pParam = NULL;
	m_dwPort = 8081;

	m_hStopEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
	m_hThread = NULL;

	m_sock = INVALID_SOCKET;

	WSAData wsaData;
	WSAStartup(MAKEWORD(2,0), &wsaData);
}

CTCPServer::~CTCPServer(void)
{
	if( m_hThread != NULL ){
		::SetEvent(m_hStopEvent);
		// スレッド終了待ち
		if ( ::WaitForSingleObject(m_hThread, 2000) == WAIT_TIMEOUT ){
			::TerminateThread(m_hThread, 0xffffffff);
		}
		CloseHandle(m_hThread);
		m_hThread = NULL;
	}
	::CloseHandle(m_hStopEvent);
	m_hStopEvent = NULL;
	
	if( m_sock != INVALID_SOCKET ){
		shutdown(m_sock,SD_RECEIVE);
		closesocket(m_sock);
		m_sock = INVALID_SOCKET;
	}
	WSACleanup();
}

BOOL CTCPServer::StartServer(DWORD dwPort, LPCWSTR acl, LPCWSTR password, CMD_CALLBACK_PROC pfnCmdProc, void* pParam)
{
	if( pfnCmdProc == NULL || pParam == NULL ){
		return FALSE;
	}
	if( m_hThread != NULL ){
		return FALSE;
	}
	m_pCmdProc = pfnCmdProc;
	m_pParam = pParam;
	m_dwPort = dwPort;
	m_acl = acl;
	WtoUTF8(password, m_password);

	m_sock = socket(AF_INET, SOCK_STREAM, 0);
	if( m_sock == INVALID_SOCKET ){
		return FALSE;
	}
	m_addr.sin_family = AF_INET;
	m_addr.sin_port = htons((WORD)dwPort);
	m_addr.sin_addr.S_un.S_addr = INADDR_ANY;
	BOOL b=1;

	setsockopt(m_sock, SOL_SOCKET, SO_REUSEADDR, (const char *)&b, sizeof(b));
	DWORD socketBuffSize = 1024*1024;
	setsockopt(m_sock, SOL_SOCKET, SO_SNDBUF, (const char*)&socketBuffSize, sizeof(socketBuffSize));
	setsockopt(m_sock, SOL_SOCKET, SO_SNDBUF, (const char*)&socketBuffSize, sizeof(socketBuffSize));

	bind(m_sock, (struct sockaddr *)&m_addr, sizeof(m_addr));

	listen(m_sock, 1);

	ResetEvent(m_hStopEvent);
	m_hThread = (HANDLE)_beginthreadex(NULL, 0, ServerThread, (LPVOID)this, CREATE_SUSPENDED, NULL);
	ResumeThread(m_hThread);

	return TRUE;
}

void CTCPServer::StopServer()
{
	if( m_hThread != NULL ){
		::SetEvent(m_hStopEvent);
		// スレッド終了待ち
		if ( ::WaitForSingleObject(m_hThread, 15000) == WAIT_TIMEOUT ){
			::TerminateThread(m_hThread, 0xffffffff);
		}
		CloseHandle(m_hThread);
		m_hThread = NULL;
	}
	
	if( m_sock != INVALID_SOCKET ){
		shutdown(m_sock,SD_RECEIVE);
		closesocket(m_sock);
		m_sock = INVALID_SOCKET;
	}
}

static BOOL TestAcl(struct in_addr addr, wstring acl)
{
	//書式例: +192.168.0.0/16,-192.168.0.1
	BOOL ret = FALSE;
	for(;;){
		wstring val;
		BOOL sep = Separate(acl, L",", val, acl);
		if( val.empty() || val[0] != L'+' && val[0] != L'-' ){
			//書式エラー
			return FALSE;
		}
		wstring a, b, c, d, m;
		Separate(val.substr(1), L".", a, b);
		Separate(b, L".", b, c);
		Separate(c, L".", c, d);
		ULONG mask = Separate(d, L"/", d, m) ? _wtoi(m.c_str()) : 32;
		if( a.empty() || b.empty() || c.empty() || d.empty() || mask > 32 ){
			//書式エラー
			return FALSE;
		}
		mask = mask == 0 ? 0 : 0xFFFFFFFFUL << (32 - mask);
		ULONG host = (ULONG)_wtoi(a.c_str()) << 24 | _wtoi(b.c_str()) << 16 | _wtoi(c.c_str()) << 8 | _wtoi(d.c_str());
		if( (ntohl(addr.s_addr) & mask) == (host & mask) ){
			ret = val[0] == L'+';
		}
		if( sep == FALSE ){
			return ret;
		}
	}
}

static BOOL ReceiveData(SOCKET sock, CMD_STREAM& stCmd)
{
	DWORD head[2];
	int iRet = recv(sock, (char*)head, sizeof(DWORD) * 2, 0);
	if (iRet != sizeof(DWORD) * 2){
		return FALSE;
	}
	stCmd.param = head[0];
	stCmd.dataSize = head[1];
	_OutputDebugString(L"Cmd = %d, size = %d\n", stCmd.param, stCmd.dataSize);

	if (stCmd.dataSize > 0) {
		SAFE_DELETE_ARRAY(stCmd.data);
		stCmd.data = new BYTE[stCmd.dataSize];

		DWORD dwRead = 0;
		while (dwRead < stCmd.dataSize) {
			iRet = recv(sock, (char*)(stCmd.data + dwRead), stCmd.dataSize - dwRead, 0);
			if (iRet == SOCKET_ERROR) {
				break;
			}
			else if (iRet == 0) {
				break;
			}
			dwRead += iRet;
		}
		if (dwRead < stCmd.dataSize) {
			return FALSE;
		}
	}
	return TRUE;
}

static BOOL SendData(SOCKET sock, CMD_STREAM& stRes)
{
	DWORD head[2] = { stRes.param, stRes.dataSize };
	int iRet = send(sock, (char*)head, sizeof(DWORD) * 2, 0);
	if (iRet == SOCKET_ERROR) {
		return FALSE;
	}
	if (stRes.dataSize > 0) {
		if (stRes.data == NULL) {
			return FALSE;
		}
		iRet = send(sock, (char*)(stRes.data), stRes.dataSize, 0);
		if (iRet == SOCKET_ERROR) {
			return FALSE;
		}
	}
	return TRUE;
}

// HMAC を求める (CryptCreateHash が derived  key からの計算しかできないようなので自家実装)
BOOL HMAC(ALG_ID id, const BYTE *key, const DWORD sizeKey, const BYTE *data, const DWORD sizeData, BYTE **ppOut, DWORD *pSizeOut)
{
	BYTE tmp[64] = { 0 };
	BYTE opad[64] = { 0 };
	BYTE ipad[64] = { 0 };
	DWORD size;

	HCRYPTPROV  hProv = NULL;
	DWORD ret = CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT);

	if (ret) {
		if (sizeKey > 64) {
			// key が64バイトを超える場合は key の hash 値を使う
			HCRYPTHASH  hHash = NULL;
			size = sizeof(tmp);
			ret = (CryptCreateHash(hProv, id, 0, 0, &hHash) &&
				CryptHashData(hHash, key, sizeKey, 0) &&
				CryptGetHashParam(hHash, HP_HASHVAL, tmp, &size, 0));
			if (hHash) {
				CryptDestroyHash(hHash);
			}
		}
		else {
			// key が64バイト未満の場合は64バイトまで 0 fill した値を使う
			memcpy(tmp, key, sizeKey);
		}
	}

	if (ret) {
		// ipad & opad を計算する
		for (int i = 0; i < sizeof(tmp); i++) {
			ipad[i] = tmp[i] ^ 0x36;
			opad[i] = tmp[i] ^ 0x5c;
		}

		// ipad + data の hash を求める
		HCRYPTHASH  hHash = NULL;
		ret = (CryptCreateHash(hProv, id, 0, 0, &hHash) &&
			CryptHashData(hHash, ipad, sizeof(ipad), 0) &&
			CryptHashData(hHash, data, sizeData, 0) &&
			CryptGetHashParam(hHash, HP_HASHVAL, tmp, &size, 0));
		if (hHash) {
			CryptDestroyHash(hHash);
		}
	}

	if (ret) {
		// opad + (ipad + data の hash) の hash(HMAC) を求める 
		HCRYPTHASH  hHash = NULL;
		ret = (CryptCreateHash(hProv, id, 0, 0, &hHash) &&
			CryptHashData(hHash, opad, sizeof(opad), 0) &&
			CryptHashData(hHash, tmp, size, 0) &&
			CryptGetHashParam(hHash, HP_HASHVAL, tmp, &size, 0));
		if (hHash) {
			CryptDestroyHash(hHash);
		}
	}

	if (ret) {
		*ppOut = new BYTE[size];
		*pSizeOut = size;
		memcpy(*ppOut, tmp, size);
	}
	if (hProv) {
		CryptReleaseContext(hProv, 0);
	}
	return ret;
}

static BOOL Authenticate(SOCKET sock, const char *password)
{
	// パスワードが設定されていない場合は認証処理を行わない
	if (password == nullptr || *password == '\0') {
		return TRUE;
	}

#if 1
	// nonce を生成する (軽量版)
	DWORD size = sizeof(LARGE_INTEGER);
	BYTE *msg = new BYTE[size];
	QueryPerformanceCounter((LARGE_INTEGER*)msg);
#else
	// nonce を生成する (CryptGenRandom版)
	DWORD size = 20;
	BYTE *msg = new BYTE[size];
	HCRYPTPROV  hProv = NULL;
	DWORD ret = (CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT) && CryptGenRandom(hProv, size, msg));
	if (hProv) {
		CryptReleaseContext(hProv, 0);
	}
#endif

	// 認証要求として、nonce をクライアントへ送る
	CMD_STREAM stAuth;
	stAuth.param = CMD_AUTH_REQUEST;
	stAuth.data = msg;
	stAuth.dataSize = size;
	if (SendData(sock, stAuth) == FALSE) {
		return FALSE;
	}

	// 受信待機
	fd_set ready;
	struct timeval to;
	to.tv_sec = 1;
	to.tv_usec = 0;
	FD_ZERO(&ready);
	FD_SET(sock, &ready);
	if (select(0, &ready, NULL, NULL, &to) == SOCKET_ERROR) {
		return FALSE;
	}
	if (!FD_ISSET(sock, &ready)) {
		return FALSE;
	}

	// レスポンスを受け取る
	CMD_STREAM stRes;
	ReceiveData(sock, stRes);
	if (stRes.param != CMD2_EPG_SRV_AUTH_REPLY) {
		return FALSE;
	}

	BYTE *hmacOut = NULL;
	DWORD szOut = 0;
	BOOL ret = FALSE;
	switch (stRes.dataSize)
	{
	case 128 / 8:  // HMAC-MD5
		ret = HMAC(CALG_MD5, (BYTE*)password, (DWORD)strlen(password), msg, size, &hmacOut, &szOut);
		break;
	case 160 / 8: // HMAC-SHA1
		ret = HMAC(CALG_SHA1, (BYTE*)password, (DWORD)strlen(password), msg, size, &hmacOut, &szOut);
		break;
	case 256 / 8: // HMAC-SHA256
		ret = HMAC(CALG_SHA_256, (BYTE*)password, (DWORD)strlen(password), msg, size, &hmacOut, &szOut);
		break;
	}
	ret = ret && (szOut == stRes.dataSize && memcmp(hmacOut, stRes.data, szOut) == 0);
	delete[] hmacOut;
	return ret;
}

UINT WINAPI CTCPServer::ServerThread(LPVOID pParam)
{
	CTCPServer* pSys = (CTCPServer*)pParam;

	SOCKET sock = INVALID_SOCKET;
	struct sockaddr_in client;
	
	fd_set ready;
	struct timeval to;

	while(1){
		if( WaitForSingleObject( pSys->m_hStopEvent, 0 ) != WAIT_TIMEOUT ){
			//中止
			break;
		}

		to.tv_sec = 1;
		to.tv_usec = 0;
		FD_ZERO(&ready);
		FD_SET(pSys->m_sock, &ready);

		if( select(0, &ready, NULL, NULL, &to ) == SOCKET_ERROR ){
			break;
		}
		if( sock == INVALID_SOCKET ){
			if ( FD_ISSET(pSys->m_sock, &ready) ){
				int len = sizeof(client);
				sock = accept(pSys->m_sock, (struct sockaddr *)&client, &len);
				if (sock == INVALID_SOCKET) {
					closesocket(pSys->m_sock);
					pSys->m_sock = INVALID_SOCKET;
					break;
				} else if (TestAcl(client.sin_addr, pSys->m_acl) == FALSE) {
					_OutputDebugString(L"Deny from IP:0x%08x\r\n", ntohl(client.sin_addr.s_addr));
					closesocket(sock);
					sock = INVALID_SOCKET;
				}
			}
		}
		if( sock != INVALID_SOCKET ){
			DWORD socketBuffSize = 1024*1024;
			setsockopt(sock, SOL_SOCKET, SO_SNDBUF, (const char*)&socketBuffSize, sizeof(socketBuffSize));
			setsockopt(sock, SOL_SOCKET, SO_SNDBUF, (const char*)&socketBuffSize, sizeof(socketBuffSize));
			
			to.tv_sec = 1;
			to.tv_usec = 0;
			FD_ZERO(&ready);
			FD_SET(sock, &ready);
			if( select(0, &ready, NULL, NULL, &to ) == SOCKET_ERROR ){
				shutdown(sock,SD_RECEIVE);
				closesocket(sock);
				sock = INVALID_SOCKET;
				continue;
			}
			if ( FD_ISSET(sock, &ready) ){
				CMD_STREAM stCmd;
				CMD_STREAM stRes;
				do{
					if (ReceiveData(sock, stCmd) == FALSE) {
						break;
					}

					if (Authenticate(sock, pSys->m_password.c_str()) == FALSE) {
						_OutputDebugString(L"Authentication error from IP:0x%08x\r\n", ntohl(client.sin_addr.s_addr));
						break;
					}

					if( stCmd.param == CMD2_EPG_SRV_REGIST_GUI_TCP || stCmd.param == CMD2_EPG_SRV_UNREGIST_GUI_TCP || stCmd.param == CMD2_EPG_SRV_ISREGIST_GUI_TCP ){
						string ip = inet_ntoa(client.sin_addr);

						REGIST_TCP_INFO setParam;
						AtoW(ip, setParam.ip);
						ReadVALUE(&setParam.port, stCmd.data, stCmd.dataSize, NULL);

						SAFE_DELETE_ARRAY(stCmd.data);
						stCmd.data = NewWriteVALUE(&setParam, stCmd.dataSize);
					}

					pSys->m_pCmdProc(pSys->m_pParam, &stCmd, &stRes);
					if( stRes.param == CMD_NO_RES ){
						break;
					}

					if (SendData(sock, stRes) == FALSE) {
						break;
					}

					SAFE_DELETE_ARRAY(stCmd.data);
					SAFE_DELETE_ARRAY(stRes.data);
					stCmd.dataSize = 0;
					stRes.dataSize = 0;
				}while(stRes.param == CMD_NEXT || stRes.param == OLD_CMD_NEXT); //Emun用の繰り返し
			}
			shutdown(sock,SD_RECEIVE);
			closesocket(sock);
			sock = INVALID_SOCKET;
		}
	}

	if( sock != INVALID_SOCKET ){
		shutdown(sock,SD_RECEIVE);
		closesocket(sock);
	}
	
	return 0;
}
