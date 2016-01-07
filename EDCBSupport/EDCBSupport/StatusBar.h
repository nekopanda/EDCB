#pragma once


#include "Widget.h"


namespace EDCBSupport
{

	// ステータスバー
	class CStatusBar : public CWidget
	{
	public:
		CStatusBar();
		~CStatusBar();
		bool Create(HWND hwndParent,int ID=0) override;
		bool SetText(LPCTSTR pszText);
	};

}	// namespace EDCBSupport
