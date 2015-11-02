tkntrec氏版EDCBを改変したバージョンです。

◆このプログラムを使用して発生した全ての問題に対して責任は持ちません。

サイト
https://github.com/nekopanda/EDCB


■Readmeファイルについて■
Readmeがいくつかありますが、以下のような構成です。
※説明は私の独自解釈を含むため一部誤っている可能性がありますがご了承ください。

◇Readme.txt
　このファイルです。この改変modについての説明や基本的な使い方などが書いてあります。

◇Readme_EDCB.txt
　オリジナルEDCB（人柱版10.69）に付属のReadme.txtです。
　更新履歴だけHistory.txtに移動しています。

◇Readme_EpgDataCap_Bon.txt
　オリジナルEDCBに付属のEpgDataCap_BonのReadmeです。
　EpgDataCap_BonはBonDriverからTSデータを受信して、録画やEPGデータの取得を行うプログラムです。

◇Readme_EpgTimer.txt
　EpgTimerのReadmeです。若干アップデートはありますが、ほぼオリジナルのままです。
　EpgTimerはメインのユーザインターフェースです。録画予約や設定などはほぼこいつから操作します。

◇Readme_Mod.txt
　xtne6f氏により執筆された人柱版10.69からの改変説明です。

◇Readme_Mod_S.txt
　xtne6f氏によるEpgTimerSrv整理版の改変説明です。
　EpgTimerSrvは裏で予約録画などを実行するいわばEDCBの司令塔のようなプログラムです。
　xtne6f氏により「わりと徹底的に整理」されました。

◇ReadMe_Material_WebUI.txt
　2chの有志によるEpgTimerのWebUI「マテリアルWebUI」のReadmeです。
　非常に美しく洗練されたWebUIを作っていただいたので同梱させていただきました。
　なお、同梱バージョンはtkntrec氏版設定がデフォルトでオンになっていて、iniファイルで
　HTTPサーバを有効化すればすぐに使える状態になっています。
　（下記「ブラウザから操作できるようにする」を参照）

◇History.txt
　更新履歴ですが最近の更新情報はありません。
　最近の更新情報が見たい方はGitHubで見てください。


■このmodの改変説明■
EPG自動予約で登録された予約とその自動予約との紐づけ、EPG自動予約で録画された録画ファイルと
その自動予約との紐づけ。これまでのEDCBではこれらがきちんと実装されていませんでした。
このmodでは、この２つの紐づけを実装し、それによって可能となった機能をいくつか追加しています。

●EPG自動予約の条件を変更したり自動予約自体を削除したりすると、連動して登録された予約が削除されます。
　削除の対象はEPG自動予約によって追加された予約（予約状況が「EPG自動予約」で始まる予約）で、かつ、
　他の自動予約の対象となっていない番組の予約です。自動予約を無効にした場合は、削除した場合と同じように
　予約を削除します。これまでのEDCBの動作であった「EPG自動予約だけを削除」することはできません。
　削除して欲しくない予約は、「予約変更」ウィンドウで「EPG個別予約」に変更してください。

　なお、「予約全削除」や「予約ごと削除」では、個別予約された予約であろうと他の自動予約の対象であろうと
　問答無用で削除するので、これらと同じ動作ではありません。

●「予約変更」ウィンドウでEPG自動予約から個別予約に変更できるようにしました。
　これまではEPG自動予約から個別予約への変更は、予約を一度削除して番組表から再度予約を追加するという操作が
　必要でしたが、「予約変更」ウィンドウで変更できるようにしました。

● 「自動予約登録」→「EPG予約」タブの各自動予約項目のツールチップの表示内容を変更
　これまでは自動予約の設定を表示していましたが、このmodではその自動予約の対象となっている
　録画予約と録画済みをそれぞれ最大10件まで表示します。

● 「EPG予約条件」ウィンドウに「録画済み」タブを追加
　録画済みを全件表示したいときはここを見てください。目的外の番組が録画されてしまった場合などは
　「自動予約登録を変更」で条件を変更して登録するとすぐに反映されます。

● 「予約一覧」タブ、「録画済み一覧」タブに「該当自動予約」列を追加
  その番組が対象となっているEPG自動予約がすべて表示されます。()内の数値は内部で使われているEPG自動予約のIDです。

● 「録画済み一覧」タブに「存在」列を追加
　録画ファイルが存在するかを表示します。自動では更新されないので更新したい場合は追加した
　「リスト更新」ボタンを押してください。

● 録画予約、録画済みのコンテキストメニューに「自動予約変更」を追加
　EPG自動予約の変更ダイアログを表示します。

● 「自動予約登録」→「EPG予約」タブに「最後の録画」列を追加
　その自動予約の対象となっている録画済みのうち最も最近のものを表示します。時刻は番組の開始時刻です。

◇注意１
　紐づけは、予約された番組が＊現在の自動予約＊の対象となっているかで判断します。
　同じように録画済みも録画された番組が＊現在の自動予約＊の対象となっているかで判断します。
　自動予約の設定を変更すると紐づけも更新されます。

　どの自動予約によって予約登録されたか、あるいは、どの自動予約によって録画されたのかは、一切考慮しません。
　予約登録されたあと、番組情報が変わる、または、自動予約の設定を変更するなどによって、
　予約登録された番組や録画された番組が、自動予約の対象外になれば、紐づけは消滅します。
　（予約がどの自動予約で追加されたのかは予約状況に書いてあります。）

◇注意２
　録画済みファイルの番組情報はTSファイルから取得されます。過去の録画ファイルも録画先にファイルが
　残っていればそれを読み取りますが、ファイルが残っていない、または、何らかの理由で番組情報の取得に
　失敗すると、そのファイルは紐づけできなくなります。


■動作環境■
動作には.NetFramework4.0とVC++2015のランタイムが必要です。
.NetFramework4.0はXP以外は普通入っています。VC++2015のランタイムはインストールが必要です。

Visual Studio 2015 の Visual C++ 再頒布可能パッケージ
https://www.microsoft.com/ja-jp/download/details.aspx?id=48145


■64bitバイナリ■
　x64フォルダにあるモジュールは64bitでビルドしたモジュールになっています。
　一応ビルドできますが、テストしてませんし利用もしていません。
　64bitOSでも32bitバイナリで問題ありません。

　64bit版を使用するにはBonDriverも64bitでビルドされている必要があります。
　32bitのモジュールが１つでも必要な場合は使用できません。


■基本的な使用準備■（Readme_EDCB.txtから転載）
　１．64bitOSで64bitネイティブで動作させるには「x64」フォルダを、
　　　それ以外の場合は「x86」フォルダを使用してください。

　２．使用デバイス用のBonDriverを用意し、BonDriver フォルダに入れる。
　　　BonDriverによってはiniファイルなどで設定できる内容があるので、あ
　　　らかじめ設定をしておく

　３．EpgDataCap_Bon.exeを起動し、「設定」→「基本設定」タブで「設定関
　　　係保存フォルダ」（相対パスは不可、絶対パスで）を設定する。

　４．チューナーから使用チューナーを選んで「チャンネルスキャン」を行う。
　　　地デジで5分程はかかると思います。
　　　使用するチューナーの種類が複数の場合は同じ事を行う。
　　　同時に複数起動して、チャンネルスキャンを行うことができます。

　５．全てのチューナーのチャンネルスキャンが完了するまで待ちます。

　６．EpgDataCap_Bon.exeを終了する。
　　　（複数起動している場合は、チャンネルスキャンが終了したものから終
　　　わらせてください）

　８．EpgTimer.exeを起動する。

　９．「設定」→「基本設定」→「チューナー」タブで各BonDriverで使用する
　　　チューナー数とEPGデータの取得に使用するかを設定します。
　　　◆チューナー数が正しく設定されないと、正常に動作しません◆

１０．「EPG取得」タブでEPG取得対象サービスを設定します。

１１．EpgTimer.exeを終了する。
　　　◆チューナー数の設定は起動時にのみ反映するので、終了することが重要です◆

１２．EpgTimer.exeを起動する。

１３．基本的な使用準備は終わり。
　　　EPG取得を行い、必要に応じて各種設定や、予約登録など行う。

EpgDataCap_Bon.exeの詳細はReadme_EpgDataCap_Bon.txt
EpgTimer.exeの詳細はReadme_EpgTimer.txt
を参照してください。

◆チューナーによって地デジの受信チャンネルが異なる特殊な受信環境の場合、
◆同一サービスのチャンネルが複数、チャンネルスキャンで引っかかっている
◆可能性があります。
◆中継局の増加により、受信レベルの低いチャンネルが引っかかっている可能
◆性もあります。
◆正常に受信できる１チャンネル分のサービスのみ残して、他のチャンネルの
◆サービスは削除してください。
◆（EpgDataCap_Bon.exeで表示されるspace、chの値を参考に、設定->サービス
◆表示設定より削除）
◆同一サービスが複数あると予約録画などの動作が正常に動作しない可能性が
◆あります。


■ブラウザから操作できるようにする（マテリアルWebUI）■
　使用前に一度EpgTimerの設定を開いてOKで閉じてください。動作に必要な情
　報が保存されます。
　EpgTimerSrv.iniのSETにEnableHttpSrvとHttpPortを追加することで有効にす
　る事が可能です。
　EnableHttpSrv　0:無効、1:有効（デフォルト 0）
　HttpPort　使用ポートを指定（デフォルト 5510）

　http://127.0.0.1:5510/ のようにアクセスしてください。

　HTTPサーバ機能についての詳細はReadme_Mod_S.txtの「Civetwebの組み込みについて」
　を参照してください。


■旧バージョンから移行時の注意■
録画済み番組情報"RecInfo2Data.bin"はテキスト形式になって"RecInfo2.txt"に移動しま
した。移行する場合は「同一番組無効登録」機能のための情報がリセットされるので注意
してください。


■tkntrec氏によるUIの機能強化■
特にReadmeは用意されていないので、tkntrec氏による主な変更をここに書いておきます。
Githubのプロジェクトページ（https://github.com/tkntrec/EDCB）からの転載です。

◇検索(自動予約登録)と右クリックメニューまわりを中心に若干の機能追加・変更をしています。

◇「EPG予約条件」ウィンドウ関係
　●「自動予約登録を削除」「予約全削除」「前へ」「次へ」ボタン追加。
　●検索結果一覧の右クリックに「番組名で再検索(サブウィンドウ)」を追加。

◇右クリックメニュー関係
　●各画面の右クリックメニューを「番組表」をベースにだいたい揃えた。
　●右クリックメニュー項目の表示/非表示を選択出来るようにした。
　●[設定]-[動作設定]-[その他]の[右クリックメニューの設定]から変更出来ます。

◇その他
　●各設定ウィンドウをESCで閉じられるようにした。
　●追加設定の説明などを含むコミット
　　設定画面ではなく、設定ファイル(XML)で直接指定するオプションについての説明です。
　　・f52b17c 無効予約アイテムの文字色の変更
　　・44dd565 状態表示列の文字色の変更
　　・1c22086 各設定画面でのフォルダ選択時に「ファイル選択ダイアログ」を使用するオプション
　　・c62755d 「番組表へジャンプ」の強調表示時間の変更
　　・c8e4bcf EpgTimerNWでサーバとの接続維持を試みるオプション


■そもそもtkntrec氏版って？■
本家が更新されなくなった後、複数の方が開発を引き継いで、主にGitHubで開発しています。
現在、アクティブに更新されているのはxtne6f氏とtkntrec氏です。
各開発者の方はGitでお互いに更新を取り込んだりしていますが、おおざっぱに図にすると
以下のようになります。

【EDCB】EpgDataCap_Bonについて語るスレ 41 より
┏━━━━━━━━━━━━━━━━━━━━━━━┓
┃●相関図　　　　　　　　　　　　　　　　　　　┃
┃　　　　　　　　　　　　　　　　　　　　　　　┃
┃本家　　　　　　　　　　　　　　　　　　　　　┃
┃　↓ 　　　　　　　　　　　　　　　　　　　　 ┃
┃epgdatacapbon版　　　　　　　　　　　　　　　 ┃
┃　↓　　　　　　　　　　　　　　　　　　　　　┃
┃niisaka版──┐ 　　　　　　　　　　　　　　　┃
┃　↓　 　　　│ 　　　　　　　　　　　　　　　┃
┃Velmy版　　xtne6f版 　　　　　　　　　　　　　┃
┃　 　　＼　　↓ 　　　　　　　　　　　　　　　┃
┃　 　　　　tkntrec版 　　　　　　　　　　　　 ┃
┃　　　　　　　　　　　　　　　　　　　　　　　┃
┃※お互いに取り込んでる機能あり　　　　　　　　┃
┗━━━━━━━━━━━━━━━━━━━━━━━┛

tkntrec氏以外の方による変更は、Readme_Mod.txtおよびReadme_Mod_S.txtを参照してください。




■ライセンス表示■
◇Civetweb
HTTPサーバとして組み込んでいます。

Copyright (c) 2013-2015 The CivetWeb developers

Copyright (c) 2004-2013 Sergey Lyubka

Copyright (c) 2013 No Face Press, LLC (Thomas Davis)

Copyright (c) 2013 F-Secure Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

The CivetWeb developers CREDITS
https://github.com/civetweb/civetweb/blob/master/CREDITS.md

◇Lua

http://www.lua.org/license.html

Copyright (C) 1994-2015 Lua.org, PUC-Rio.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

◇Lua File System

http://keplerproject.github.io/luafilesystem/license.html

Copyright (c) 2003 Kepler Project.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

◇LuaBinaries
LuaBinariesによりビルドされたLuaのDLL(lua52.dll)を同梱しています。

http://luabinaries.sourceforge.net/license.html

Copyright (c) 2005-2015 Tecgraf/PUC-Rio and the Kepler Project.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

