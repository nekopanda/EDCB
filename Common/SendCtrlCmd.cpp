#include "StdAfx.h"
#include "SendCtrlCmd.h"
/*
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "wsock32.lib")
#pragma comment(lib, "Ws2_32.lib")
*/
#include "StringUtil.h"

CSendCtrlCmd::CSendCtrlCmd(void)
{
	this->tcpFlag = FALSE;
	this->connectTimeOut = CONNECT_TIMEOUT;

	this->pipeName = CMD2_EPG_SRV_PIPE;
	this->eventName = CMD2_EPG_SRV_EVENT_WAIT_CONNECT;

	this->ip = L"127.0.0.1";
	this->port = 5678;

}


CSendCtrlCmd::~CSendCtrlCmd(void)
{
	SetSendMode(FALSE);
}

//コマンド送信方法の設定
//引数：
// tcpFlag		[IN] TRUE：TCP/IPモード、FALSE：名前付きパイプモード
void CSendCtrlCmd::SetSendMode(
	BOOL tcpFlag
	)
{
	if( this->tcpFlag == FALSE && tcpFlag ){
		WSAData wsaData;
		WSAStartup(MAKEWORD(2, 0), &wsaData);
		this->tcpFlag = TRUE;
	}else if( this->tcpFlag && tcpFlag == FALSE ){
		WSACleanup();
		this->tcpFlag = FALSE;
	}
}

//名前付きパイプモード時の接続先を設定
//EpgTimerSrv.exeに対するコマンドは設定しなくても可（デフォルト値になっている）
//引数：
// eventName	[IN]排他制御用Eventの名前
// pipeName		[IN]接続パイプの名前
void CSendCtrlCmd::SetPipeSetting(
	LPCWSTR eventName,
	LPCWSTR pipeName
	)
{
	this->eventName = eventName;
	this->pipeName = pipeName;
}

//名前付きパイプモード時の接続先を設定（接尾にプロセスIDを伴うタイプ）
//引数：
// pid			[IN]プロセスID
void CSendCtrlCmd::SetPipeSetting(
	LPCWSTR eventName,
	LPCWSTR pipeName,
	DWORD pid
	)
{
	Format(this->eventName, L"%s%d", eventName, pid);
	Format(this->pipeName, L"%s%d", pipeName, pid);
}

//TCP/IPモード時の接続先を設定
//引数：
// ip			[IN]接続先IP
// port			[IN]接続先ポート
void CSendCtrlCmd::SetNWSetting(
	wstring ip,
	DWORD port
	)
{
	this->ip = ip;
	this->port = port;
}

//接続処理時のタイムアウト設定
// timeOut		[IN]タイムアウト値（単位：ms）
void CSendCtrlCmd::SetConnectTimeOut(
	DWORD timeOut
	)
{
	this->connectTimeOut = timeOut;
}

static DWORD ReadFileAll(HANDLE hFile, BYTE* lpBuffer, DWORD dwToRead)
{
	DWORD dwRet = 0;
	for( DWORD dwRead; dwRet < dwToRead && ReadFile(hFile, lpBuffer + dwRet, dwToRead - dwRet, &dwRead, NULL); dwRet += dwRead );
	return dwRet;
}

DWORD CSendCtrlCmd::SendPipe(LPCWSTR pipeName, LPCWSTR eventName, DWORD timeOut, CMD_STREAM* send, CMD_STREAM* res)
{
	if( pipeName == NULL || eventName == NULL || send == NULL || res == NULL ){
		return CMD_ERR_INVALID_ARG;
	}

	//接続待ち
	//CreateEvent()してはいけない。イベントを作成するのはサーバの仕事のはず
	//CreateEvent()してしまうとサーバが終了した後は常にタイムアウトまで待たされることになる
	HANDLE waitEvent = OpenEvent(SYNCHRONIZE, FALSE, eventName);
	if( waitEvent == NULL ){
		return CMD_ERR_CONNECT;
	}
	DWORD dwRet = WaitForSingleObject(waitEvent, timeOut);
	CloseHandle(waitEvent);
	if( dwRet == WAIT_TIMEOUT ){
		return CMD_ERR_TIMEOUT;
	}else if( dwRet != WAIT_OBJECT_0 ){
		return CMD_ERR_CONNECT;
	}

	//接続
	HANDLE pipe = CreateFile( pipeName, GENERIC_READ|GENERIC_WRITE, 0, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL);
	if( pipe == INVALID_HANDLE_VALUE ){
		_OutputDebugString(L"*+* ConnectPipe Err:%d\r\n", GetLastError());
		return CMD_ERR_CONNECT;
	}

	DWORD write = 0;

	//送信
	DWORD head[2];
	head[0] = send->param;
	head[1] = send->dataSize;
	if( WriteFile(pipe, head, sizeof(DWORD)*2, &write, NULL ) == FALSE ){
		CloseHandle(pipe);
		return CMD_ERR;
	}
	if( send->dataSize > 0 ){
		if( WriteFile(pipe, send->data, send->dataSize, &write, NULL ) == FALSE ){
			CloseHandle(pipe);
			return CMD_ERR;
		}
	}

	//受信
	if( ReadFileAll(pipe, (BYTE*)head, sizeof(head)) != sizeof(head) ){
		CloseHandle(pipe);
		return CMD_ERR;
	}
	res->param = head[0];
	res->dataSize = head[1];
	if( res->dataSize > 0 ){
		res->data = new BYTE[res->dataSize];
		if( ReadFileAll(pipe, res->data, res->dataSize) != res->dataSize ){
			CloseHandle(pipe);
			return CMD_ERR;
		}
	}
	CloseHandle(pipe);

	return res->param;
}

static int RecvAll(SOCKET sock, char* buf, int len, int flags)
{
	int n = 0;
	while( n < len ){
		int ret = recv(sock, buf + n, len - n, flags);
		if( ret < 0 ){
			return ret;
		}else if( ret <= 0 ){
			break;
		}
		n += ret;
	}
	return n;
}

DWORD CSendCtrlCmd::SendTCP(wstring ip, DWORD port, DWORD timeOut, CMD_STREAM* sendCmd, CMD_STREAM* resCmd)
{
	if( sendCmd == NULL || resCmd == NULL ){
		return CMD_ERR_INVALID_ARG;
	}

	struct sockaddr_in server;
	SOCKET sock;

	sock = socket(AF_INET, SOCK_STREAM, 0);
	server.sin_family = AF_INET;
	server.sin_port = htons((WORD)port);
	string strA = "";
	WtoA(ip, strA);
	server.sin_addr.S_un.S_addr = inet_addr(strA.c_str());

	int ret = connect(sock, (struct sockaddr *)&server, sizeof(server));
	if( ret == SOCKET_ERROR ){
		int a= GetLastError();
		wstring aa;
		Format(aa,L"%d",a);
		OutputDebugString(aa.c_str());
		closesocket(sock);
		return CMD_ERR_CONNECT;
	}

	//送信
	DWORD head[256];
	head[0] = sendCmd->param;
	head[1] = sendCmd->dataSize;
	DWORD extSize = 0;
	if( sendCmd->dataSize > 0 ){
		extSize = min(sendCmd->dataSize, sizeof(head) - sizeof(DWORD)*2);
		memcpy(head + 2, sendCmd->data, extSize);
	}
	if( send(sock, (char*)head, sizeof(DWORD)*2 + extSize, 0) == SOCKET_ERROR ||
	    sendCmd->dataSize > extSize && send(sock, (char*)sendCmd->data + extSize, sendCmd->dataSize - extSize, 0) == SOCKET_ERROR ){
		closesocket(sock);
		return CMD_ERR;
	}
	//受信
	if( RecvAll(sock, (char*)head, sizeof(DWORD)*2, 0) != sizeof(DWORD)*2 ){
		closesocket(sock);
		return CMD_ERR;
	}
	resCmd->param = head[0];
	resCmd->dataSize = head[1];
	if( resCmd->dataSize > 0 ){
		resCmd->data = new BYTE[resCmd->dataSize];
		if( RecvAll(sock, (char*)resCmd->data, resCmd->dataSize, 0) != resCmd->dataSize ){
			closesocket(sock);
			return CMD_ERR;
		}
	}
	closesocket(sock);

	return resCmd->param;
}

DWORD CSendCtrlCmd::SendFileCopy(
	wstring val,
	BYTE** resVal,
	DWORD* resValSize
	)
{
	CMD_STREAM res;
	DWORD ret = SendCmdData(CMD2_EPG_SRV_FILE_COPY, val, &res);

	if( ret == CMD_SUCCESS ){
		if( res.dataSize == 0 ){
			return CMD_ERR;
		}
		*resValSize = res.dataSize;
		*resVal = new BYTE[res.dataSize];
		memcpy(*resVal, res.data, res.dataSize);
	}
	return ret;
}

DWORD CSendCtrlCmd::SendGetEpgFile2(
	wstring val,
	BYTE** resVal,
	DWORD* resValSize
	)
{
	CMD_STREAM res;
	DWORD ret = SendCmdData2(CMD2_EPG_SRV_GET_EPG_FILE2, val, &res);

	if( ret == CMD_SUCCESS ){
		WORD ver = 0;
		DWORD readSize = 0;
		if( ReadVALUE(&ver, res.data, res.dataSize, &readSize) == FALSE || res.dataSize <= readSize ){
			return CMD_ERR;
		}
		*resValSize = res.dataSize - readSize;
		*resVal = new BYTE[*resValSize];
		memcpy(*resVal, res.data + readSize, *resValSize);
	}
	return ret;
}

DWORD CSendCtrlCmd::SendCmdStream(CMD_STREAM* send, CMD_STREAM* res)
{
	DWORD ret = CMD_ERR;
	CMD_STREAM tmpRes;

	if( res == NULL ){
		res = &tmpRes;
	}
	if( this->tcpFlag == FALSE ){
		ret = SendPipe(this->pipeName.c_str(), this->eventName.c_str(), this->connectTimeOut, send, res);
	}else{
		ret = SendTCP(this->ip, this->port, this->connectTimeOut, send, res);
	}

	return ret;
}

