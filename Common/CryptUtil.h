#pragma once

#include <wincrypt.h>

#define MAX_PASSWORD_LENGTH  64 

extern const int base64_encode[66];
extern const int base64_decode[256];

BOOL HMAC(ALG_ID id, const BYTE *pKey, const DWORD cbKey, const BYTE *pData, const DWORD cbData, BYTE **ppOut, DWORD *pcbOut);
BOOL GetRandom(BYTE *pOut, DWORD cbOut);

BOOL Encrypt(const string& input_string, string& output_string);
BOOL Encrypt(const wstring& input_string, wstring& output_string);
BOOL Decrypt(const string& input_string, string& output_string);
BOOL Decrypt(const wstring& input_string, wstring& output_string);

template<class T>
BOOL Base64Encode(BYTE *pBinary, DWORD cbBinary, T& base64_string)
{
	base64_string.clear();
	for (DWORD i = 0; i < cbBinary; i += 3)
	{
		bool b1 = i + 1 < cbBinary;
		bool b2 = i + 2 < cbBinary;

		int i0 = pBinary[i] >> 2;
		int i1 = (pBinary[i] & 0x03) << 4 | (b1 ? (pBinary[i + 1] & 0xF0) >> 4 : 0);
		int i2 = b1 ? (pBinary[i + 1] & 0x0F) << 2 | (b2 ? (pBinary[i + 2] & 0xC0) >> 6 : 0) : 64;
		int i3 = b2 ? pBinary[i + 2] & 0x3F : 64;
		base64_string.push_back(base64_encode[i0]);
		base64_string.push_back(base64_encode[i1]);
		base64_string.push_back(base64_encode[i2]);
		base64_string.push_back(base64_encode[i3]);
	}
	return TRUE;
}

template<class T>
BOOL Base64Decode(const T& base64_string, BYTE **ppOut, DWORD *pcbOut)
{
	if (base64_string.size() % 4 != 0) {
		return FALSE;
	}

	int length = 0;
	for (auto itr = base64_string.begin(); itr != base64_string.end(); itr++) {
		if (base64_decode[*itr & 0xFF] >= 0)
			length++;
	}
	if (length != base64_string.size()) {
		return FALSE;
	}

	length = (length >> 2) * 3;
	BYTE *buf = new BYTE[length];
	*ppOut = buf;

	for (auto itr = base64_string.begin(); itr != base64_string.end();) {
		int i0 = *itr++ & 0xFF;
		int i1 = *itr++ & 0xFF;
		int i2 = *itr++ & 0xFF;
		int i3 = *itr++ & 0xFF;
		*buf++ = base64_decode[i0] << 2 | base64_decode[i1] >> 4;
		if (i2 == '=')
			break;
		*buf++ = base64_decode[i1] << 4 | base64_decode[i2] >> 2;
		if (i3 == '=')
			break;
		*buf++ = base64_decode[i2] << 6 | base64_decode[i3];
	}
	*pcbOut = (DWORD)(buf - *ppOut);
	return TRUE;
}
