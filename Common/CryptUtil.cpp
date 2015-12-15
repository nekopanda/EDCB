#include "stdafx.h"

#include "CryptUtil.h"
#include "StringUtil.h"
#include <wincrypt.h>

#pragma comment(lib, "advapi32.lib")
#pragma comment(lib, "crypt32.lib")

// template<class T>Base64Encode用テーブル
const int base64_encode[66] = {
	'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
	'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
	'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd',
	'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n',
	'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
	'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',
	'8', '9', '+', '/', '='
};

// template<class T>Base64Decode用テーブル
const int base64_decode[256] = {
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,62,-1,-1,-1,63,		// +, /
	52,53,54,55,56,57,58,59, 60,61,-1,-1,-1,64,-1,-1,		// 0-9, =
	-1, 0, 1, 2, 3, 4, 5, 6,  7, 8, 9,10,11,12,13,14,		// A-Z
	15,16,17,18,19,20,21,22, 23,24,25,-1,-1,-1,-1,-1,
	-1,26,27,28,29,30,31,32, 33,34,35,36,37,38,39,40,		// a-z
	41,42,43,44,45,46,47,48, 49,50,51,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1,
	-1,-1,-1,-1,-1,-1,-1,-1, -1,-1,-1,-1,-1,-1,-1,-1
};

// HMAC を求める (CryptCreateHash が derived key からの計算しかできないようなので自家実装)
BOOL HMAC(ALG_ID id, const BYTE *pKey, const DWORD cbKey, const BYTE *pData, const DWORD cbData, BYTE **ppOut, DWORD *pcbOut)
{
	BYTE tmp[64] = { 0 }; // SHA512(64バイト)まで対応
	BYTE opad[64] = { 0 };
	BYTE ipad[64] = { 0 };
	DWORD size = sizeof(tmp);

	HCRYPTPROV  hProv = NULL;
	DWORD ret = CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT);

	if (ret) {
		if (cbKey > 64) {
			// key が64バイトを超える場合は key の hash 値を使う
			HCRYPTHASH  hHash = NULL;
			ret = (CryptCreateHash(hProv, id, 0, 0, &hHash) &&
				CryptHashData(hHash, pKey, cbKey, 0) &&
				CryptGetHashParam(hHash, HP_HASHVAL, tmp, &size, 0));
			if (hHash) {
				CryptDestroyHash(hHash);
			}
		}
		else {
			// key が64バイト未満の場合は64バイトまで 0 fill した値を使う
			memcpy(tmp, pKey, cbKey);
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
			CryptHashData(hHash, pData, cbData, 0) &&
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
		*pcbOut = size;
		memcpy(*ppOut, tmp, size);
	}
	if (hProv) {
		CryptReleaseContext(hProv, 0);
	}
	return ret;
}

BOOL GetRandom(BYTE *pOut, DWORD cbOut)
{
	HCRYPTPROV  hProv = NULL;
	BOOL ret = CryptAcquireContext(&hProv, NULL, NULL, PROV_RSA_AES, CRYPT_VERIFYCONTEXT) && 
		CryptGenRandom(hProv, cbOut, pOut);
	if (hProv) {
		CryptReleaseContext(hProv, 0);
	}
	return ret;
}

BOOL Encrypt(const string& input_string, string& output_string)
{
	BOOL ret;

	DATA_BLOB input;
	DATA_BLOB output;
	DWORD dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN | CRYPTPROTECT_AUDIT;
	input.pbData = (BYTE*)input_string.data();
	input.cbData = (DWORD)input_string.size();
	ret = CryptProtectData(&input, NULL, NULL, NULL, NULL, dwFlags, &output);
	if (ret) {
		ret = Base64Encode(output.pbData, output.cbData, output_string);
		LocalFree(output.pbData);
	}
	return ret;
}

BOOL Encrypt(const wstring& input_string, wstring& output_string)
{
	BOOL ret;

	DATA_BLOB input;
	DATA_BLOB output;
	DWORD dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN | CRYPTPROTECT_AUDIT;
	string utf8;
	WtoUTF8(input_string, utf8);
	input.pbData = (BYTE*)utf8.data();
	input.cbData = (DWORD)utf8.size();
	ret = CryptProtectData(&input, NULL, NULL, NULL, NULL, dwFlags, &output);
	if (ret) {
		ret = Base64Encode(output.pbData, output.cbData, output_string);
		LocalFree(output.pbData);
	}
	return ret;
}

BOOL Decrypt(const string& input_string, string& output_string)
{
	BOOL ret;

	BYTE *pData = NULL;
	DWORD cbData;
	ret = Base64Decode(input_string, &pData, &cbData);
	if (ret) {
		DATA_BLOB input;
		DATA_BLOB output;
		DWORD dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN | CRYPTPROTECT_AUDIT;
		input.pbData = pData;
		input.cbData = cbData;
		ret = CryptUnprotectData(&input, NULL, NULL, NULL, NULL, dwFlags, &output);
		if (ret) {
			output_string.assign((char*)output.pbData, output.cbData);
			LocalFree(output.pbData);
		}
		delete[] pData;
	}
	return ret;
}

BOOL Decrypt(const wstring& input_string, wstring& output_string)
{
	BOOL ret;

	BYTE *pData = NULL;
	DWORD cbData;
	ret = Base64Decode(input_string, &pData, &cbData);
	if (ret) {
		DATA_BLOB input;
		DATA_BLOB output;
		DWORD dwFlags = CRYPTPROTECT_LOCAL_MACHINE | CRYPTPROTECT_UI_FORBIDDEN | CRYPTPROTECT_AUDIT;
		input.pbData = pData;
		input.cbData = cbData;
		ret = CryptUnprotectData(&input, NULL, NULL, NULL, NULL, dwFlags, &output);
		if (ret) {
			string utf8;
			utf8.assign((char*)output.pbData, output.cbData);
			UTF8toW(utf8, output_string);
			LocalFree(output.pbData);
		}
		delete[] pData;
	}
	return ret;
}
