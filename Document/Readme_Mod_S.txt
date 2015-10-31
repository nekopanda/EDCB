EpgTimerSrv整理版の改変説明

■概要■
EpgTimerSrv.exe以外に実質的な変更はありません。逆にEpgTimerSrv.exeは互換を維持で
きる範囲でわりと徹底的に整理しています。
サービスとして使用しない場合、EpgTimer.exeやEpgTimerTask.exeの常駐は不要になりま
した。EpgTimerSrv.exeがタスクトレイアイコンを表示し、スリープ確認ダイアログやチ
ューナ・バッチを直接起動します。
HTTPサーバ機能はフル機能のWebサーバを組み込み、ページ生成をLuaに任せました。詳細
は 後述「Civetwebの組み込みについて」を参照してください。

このファイルではReadme_Mod.txtからさらに改変した部分だけを説明します。

■Readme_EpgTimer.txt■
◇主な機能
  サーバー連携機能は廃止しました。

◇録画設定
  ・追従
    プログラム予約に切り替えるのと実質的な違いがほとんど無いため、「イベントリレ
    ー追従」に意味を変更しました。

◇設定
  ●動作設定タブ
    ●録画動作
      ・bat実行条件
        廃止しました(バッチごとに指定)。
    ●予約情報管理処理
      ・イベントリレーによる追従を行う
        廃止しました(予約ごとに指定)。
      ・EPGデータ読み込み時、予約時と番組名が変わっていれば番組名を変更する
        廃止しました(常にオン)。
      ・EPGデータ読み込み時、EventIDの変更を開始、終了時間のみで処理する
        廃止しました(常にオフ)。
      ・同一物理チャンネルで連続となるチューナーの使用を優先する
        廃止しました(常にオン)。
    ●その他
      ・EPG取得時に放送波時間でPC時計を同期する
        仕様変更ではありませんが、厳密には管理者権限ではなくSE_SYSTEMTIME_NAME特
        権(コントロールパネル→ローカルセキュリティポリシー→システム時刻の変更)
        が必要です。判断は任せますが管理者起動させるよりもユーザにこの特権を与え
        るほうがマシな気がします。
      ・EpgTimerSrvを常駐させる【追加】
        ・タスクトレイアイコンを表示する【追加】
        ・バルーンチップでの動作通知を抑制する【追加】
      ・サーバー間連携
        廃止しました。
  ●外部アプリケーション
    ●Twitter設定タブ
      廃止しました。

◇スタンバイ、休止状態への移行
  次の予約録画またはEPG取得に対して、動作設定で指定した"復帰処理開始時間"+8分、
  かつ抑制条件で指定した時間以上の開きがある場合に移行します。

◇録画後のバッチファイル実行の仕様
  バッチのプロセス優先度は"通常以下"(BELOW_NORMAL_PRIORITY_CLASS)で実行します。
  以下の拡張命令を利用できます【追加】。拡張命令はバッチファイル内のどこかに直接
  記述してください(remコメント等どんな形式でもOK)。
  _EDCBX_BATMARGIN_={bat実行条件(分)}
    このマージン以上録画予定がないときに実行開始します。デフォルトは0です。
  _EDCBX_HIDE_
    ウィンドウを非表示にします。
  _EDCBX_NORMAL_
    ウィンドウを最小化しません。
  _EDCBX_DIRECT_
    マクロを置換ではなく環境変数で渡して直接実行します。マクロを囲う$が%になるだ
    けですが、start /waitを使って別のスクリプトに処理を引き継ぐときに便利です。
    また、以下を保証します:
      ・EpgTimerSrv.exeのあるフォルダに"EpgTimer_Bon_RecEnd.bat"を作らない
      ・EpgTimer.exeを経由する間接実行はしない
      ・バッチのカレントディレクトリはそのバッチのあるフォルダになる
  取得できるマクロについては以下の3行のコマンドで確認すると手っ取り早いです。
    >rem _EDCBX_DIRECT_
    >set
    >pause

◇追従の仕様
  あいまい検索処理によるEventID変更に対する追従処理は一切行いません。

◇Twitter機能
  Twitter機能は廃止しました。代わりにEpgTimerSrv.exeのあるフォルダに置かれた以下
  のバッチファイルを実行します【追加】。取得できるマクロは従来とだいたい同じです
  (NEW系マクロは名前からNEWを取り除いています)。PostRecEnd.bat以外は$ReserveID$(
  予約ID)、$RecMode$(録画モード0=全サービス～4=視聴)、$ReserveComment$(コメント)
  も取得できます。
  ・PostAddReserve.bat : 予約を追加したとき(無効を除く)
    ・EPG自動予約のとき$ReserveComment$は"EPG自動予約"という文字列で始まります
  ・PostChgReserve.bat : 予約を変更したとき(無効を除く)
    ・$SYMDHMNEW$～$SEYMDHM28NEW$は取得できません(必要かどうか検討中…)
  ・PostRecStart.bat : 録画を開始したとき
  ・PostRecEnd.bat : 録画を終了したとき
    ・取得できるマクロは録画後バッチと完全に同じです
  また、イベント発生時に以下のバッチファイルを実行します。
  ・PostNotify.bat : 更新通知が送られたとき
    ・取得できるマクロは$NotifyID$のみです
      ・$NotifyID$(1=EPGデータ更新, 2=予約情報更新, 3=録画結果情報更新)
  バッチ仕様は録画後バッチと同じですが、_EDCBX_BATMARGIN_は無効です。また、
  _EDCBX_DIRECT_でないときの一時ファイル名は"EpgTimer_Bon_Post.bat"です。
  実行は直列に行います(互いに並列実行しない)。また、実行時点で各々の動作が完了し
  ている(予約ファイル等更新済みである)ことを保証します。
  iniフォルダに簡単な参考用バッチファイルを用意しました。

◇Q&A
・予約割り振りの仕方を詳しく
  全予約を開始時間でソートし、予約のない時間帯ごとに組分け、組ごとの予約に対して
  "予約優先度>開始時間(終ろ優先時逆順)>予約ID"の一意な優先度をつけ、優先度順に予
  約をチューナに割り当てます。ここで、優先度をもとに実際の録画時間を計算し、最長
  となるチューナを選びます(同じ長さならBonDriverの優先度順)。チューナ強制指定時
  はここで選べるチューナが限定されることになります。録画時間が0分ならチューナ不
  足となります。

・開始と終了時間重なっているときの動作を詳しく
  別チャンネルの場合、前番組の優先度が高いときはそれが終わるまで後番組の録画は始
  まりません。後番組の優先度が高いときは、その開始20秒前に前番組の録画を終了しま
  す。マージンを含めて録画時間です。マージン無視等の仕様は廃止しました。

◇追従の仕様(この項上書き)
  追従処理には大きく2種類あります。
  1.EPGデータ読み込み時に、読み込んだEPGデータから追従を行う
    ※このEPGデータはEpgTimerの番組表と同じものです
    プログラム予約、および2.で一度でも変更された予約は対象外です。追従するパラメ
    ータは開始時間、終了時間、およびイベント名です。

  2.起動中のEpgDataCap_Bon.exeの蓄積しているEPGデータから追従を行う
    起動中のチャンネルと異なるチャンネルの予約、6時間以上先の予約、プログラム予
    約、および無効予約は対象外です。追従するパラメータは開始時間、終了時間、イベ
    ント名、およびイベントリレーです。イベントリレーは現在番組(present)の情報の
    みを利用して行います。
    現在番組の終了時間が未定になった場合：
      現在番組の予約が録画終了(マージン含む)まで5分を切るタイミングで、5分ずつ予
      約を延長していきます。終了時間未定のまま番組が終わった場合、番組終了から5
      ～10分後に録画が終わることになります。終了時間が再度決定すれば、決定時間に
      変更します。

    次番組(following)の終了時間が未定になった場合：
      次番組の予約が録画終了(マージン含む)まで5分を切るタイミングで、5分ずつ予約
      を延長していきます。終了時間未定のまま次番組が切り替わった場合、後述の「現
      在でも次でもない番組」として扱います。終了時間が再度決定すれば、決定時間に
      変更します。
      また、このとき現在でも次でもない番組についても時間が定まらないので、これら
      の予約が録画終了(マージン含む)まで5分を切るタイミングで、録画総時間が
      TuijyuHourに達するまで、5分ずつ予約を延長していきます。次番組の終了時間が
      再度決定すれば、延長を停止します。この番組が現在または次番組になれば、その
      時間に変更します。
      一部の放送局で、未定解消直後の番組のイベントIDが変更されることがあります。
      これに対処するため、現在または次番組に未定追従中の予約とイベント名が完全一
      致するイベントが存在する場合に限り、その予約を番組終了時間まで延長します。

◇追従動作のカスタマイズ
  ・NoEpgTuijyuMinは廃止しました(常に0)
  ・DuraChgMarginMinは廃止しました(常に0)
  ・TuijyuHourのデフォルトは3時間になりました

◇予約割り振りのアルゴリズムの変更
  廃止しました。

◇ブラウザから表示できるようにする
  「Civetwebの組み込みについて」を参照。

◇録画後bat起動時の形式を変える
  非サービス時は常に最小化で起動します。

◇情報通知ログを自動的にファイルに保存する
  EpgTimerSrv.iniのSETでSaveNotifyLog=1とすると、EpgTimerSrvのあるフォルダの
  EpgTimerSrvNotifyLog.txtにも保存できます(EpgTimerSrvによる直接保存)【追加】。

◇デバッグ出力をファイルに保存する【追加】
  EpgTimerSrv.iniのSETでSaveDebugLog=1とすると、デバッグ出力(OutputDebugStringW)
  の内容をEpgTimerSrvのあるフォルダのEpgTimerSrvDebugLog.txtに保存できます。
  ※DLLからのデバッグ出力は対象外
  EpgDataCap_Bon.iniについても同様に設定できます。こちらはEpgDataCap_Bon.exeの起
  動数に応じてEpgDataCap_Bon_DebugLog-{番号}.txtとなります。

◇DLNAのDMSぽい機能を使う
  「Civetwebの組み込みについて」を参照。

■Civetwebの組み込みについて■
HTTPサーバ機能の簡単化とディレクトリトラバーサル等々のバグ修正を目的に、EpgTimerSrv.exeにCivetwebを組み込みました。
HTTPサーバ機能は従来通りEpgTimerSrv.iniのEnableHttpSrvキーを1にすると有効になります(2にするとEpgTimerSrv.exeと同じ場所にログファイルも出力)。
有効にする場合はEpgTimerSrv.exeと同じ場所にlua52.dllが必要です。対応するものをDLしてください。
http://sourceforge.net/projects/luabinaries/files/5.2.3/Windows%20Libraries/Dynamic/
Civetwebについては本家のドキュメント↓を参照してください(英語)
https://github.com/bel2125/civetweb/tree/master/docs
SSL/TLSを利用する場合はEpgTimerSrv.exeと同じ場所にssleay32.dllとlibeay32.dllが必要です。自ビルド(推奨)するか信頼できるどこかから入手してください。
とりあえず http://www.openssl.org/related/binaries.html にある http://indy.fulgan.com/SSL で動作を確認しています。

「DLNAのDMSぽい機能」はHTTPサーバに統合しました。
iniフォルダにあるdlna以下をdocument_rootに置いてEpgTimerSrv.iniのEnableDMSキーを1にすると有効になります。

以下の設定をCivetwebのデフォルトから変更しています:
  access_control_list: EpgTimerSrv.iniのHttpAccessControlListキーの値(デフォルトは"+127.0.0.1")
    ※従来通りすべてのアクセスを許可する場合は"+0.0.0.0/0"とする
  extra_mime_types: "ContentTypeText.txt"に追加したMIMEタイプ(従来通り)
  listening_ports: EpgTimerSrv.iniのHttpPortキーの値(従来通りデフォルトは5510)
    ※複数ポート指定方法などは本家ドキュメント参照
  document_root: EpgTimerSrv.iniのHttpPublicFolderキーの値(デフォルトはEpgTimerSrv.exeと同じ場所の"HttpPublic")
    ※document_rootには日本語(マルチバイト)文字を含まないこと
  num_threads: 3
  lua_script_pattern: "**.lua$|**.html$|*/api/*$" (つまり.htmlファイルはLuaスクリプト扱いになる)
    ※REST APIとの互換のため、document_root直下のapiフォルダにあるファイルもLuaスクリプト扱いになる
  ssl_certificate: listening_portsに文字's'を含むとき、EpgTimerSrv.exeと同じ場所の"ssl_cert.pem"
  global_auth_file: EpgTimerSrv.exeと同じ場所の"glpasswd"

document_root以下のフォルダやファイルが公開対象です(色々遊べる)。
iniフォルダに原作っぽい動作をするLuaスクリプトを追加したので参考にしてください。

外部公開は推奨しませんが、もしも行う場合は本家ドキュメントに従い"glpasswd"を作成し(フォルダごとの.htpasswdは不確実)、
SSL/TLSを利用してください。さらに重要なデータから隔離してください。
Civetwebでセキュリティが確保されているだろうと判断できたのは認証処理までの不正アクセス耐性のみです。
DoS耐性は期待できませんし、パス無しの公開サーバとしての利用もお勧めできません。

"ssl_cert.pem"(秘密鍵+自己署名証明書)の作成手順は本家ドキュメントに従わないほうが良いです。暗号強度が
「SSL/TLS暗号設定ガイドライン」(https://www.ipa.go.jp/security/vuln/ssl_crypt_config.html)を満たしていません。
(作成手順例、鵜呑みにしないこと)
> openssl genrsa -out server.key 2048
> openssl req -new -key server.key -out server.csr
> openssl x509 -req -days 3650 -sha256 -in server.csr -signkey server.key -out server.crt
> copy server.crt ssl_cert.pem
> type server.key >> ssl_cert.pem

"glpasswd"の簡単な作り方(ユーザ名root、パスワードtest)
> set <nul /p "x=root:mydomain.com:test" | openssl md5
  (出力される32文字のハッシュ値でパスワードを上書き↓)
> set <nul /p "x=root:mydomain.com:351eee77bbb11db9fef4870b0d78b061" >glpasswd

Luaのmg.write()について、成否のブーリアンを返すよう拡張しています。
機会があればCivetweb側に要望するつもりですが、その過程で仕様が変わる(例えば整数を返すようになるなど)可能性もあります。

■Lua edcbグローバル変数の仕様■
機能はEpgTimerSrv本体にあるメソッドとほぼ同じなので
C++を読める人はEpgTimerSrvMain.cppにある実装を眺めると良いかもしれない。

[略語の定義]
B:ブーリアン
I:整数
S:文字列
TIME:標準ライブラリos.date('*t')が返すテーブルと同じ
<テーブル名>:後述で定義するテーブル
～のリスト:添え字1からNまでN個の～を添え字順に格納したテーブル

htmlEscape:I
  文字列返却値の実体参照変換を指示するフラグ(+1=amp,+2=lt,+4=gt,+8=quot,+16=apos)
  初期値は0。
  例えばedcb.htmlEscape=15とすると'<&"テスト>'は'&lt;&amp;&quot;テスト&gt;'のように変換される。

S GetGenreName( 大分類*256+中分類:I )
  STD-B10のジャンル指定の文字列を取得する
  中分類を0xFFとすると大分類の文字列が返る。
  存在しないとき空文字列。
  例えばedcb.GetGenreName(0x0205)は'グルメ・料理'が返る。

S GetComponentTypeName( コンポーネント内容*256+コンポーネント種別:I )
  STD-B10のコンポーネント記述子の文字列を取得する
  存在しないとき空文字列。
  例えばedcb.GetComponentTypeName(0x01B1)は'映像1080i(1125i)、アスペクト比4:3'が返る。

S|nil Convert( to文字コード:S, from文字コード:S, 変換対象:S )
  文字コード変換する
  利用できる文字コードは'utf-8'または'cp932'のみ。
  変換に失敗すると空文字列、利用できない文字コードを指定するとnilが返る。
  例：os.execute(edcb.Convert('cp932','utf-8','echo 表が怖い & pause'))

Sleep( ミリ秒:I )
  スレッドの実行を中断する

S GetPrivateProfile( セクション:S, キー:S, 既定値:S|I|B, ファイル名:S )
  Win32APIのGetPrivateProfileStringを呼ぶ
  既定値がBのときはtrue=1、false=0に変換される。
  ファイル名はEDCBフォルダ配下に置かれたiniファイルを相対指定する。
  例：v=0+edcb.GetPrivateProfile('SET','HttpPort',5510,'EpgTimerSrv.ini')
  ファイル名が'Setting\\'で始まるときは「設定関係保存フォルダ」にリダイレクトされる。
  戻り値がUTF-16換算で8192文字以上の部分は切り捨てられる。
  特例的に以下の引数でEDCBフォルダのパスが返る。
  edcb.GetPrivateProfile('SET','ModulePath','','Common.ini')

B WritePrivateProfile( セクション:S, キー:S|nil, 値:S|I|B|nil, ファイル名:S )
  Win32APIのWritePrivateProfileStringを呼ぶ
  値がBのときはtrue=1、false=0に変換される。
  キーまたは値がnilのときの挙動はWritePrivateProfileStringと同じ。
  ファイル名についてはGetPrivateProfile()と同じ。
  書き込めたときtrueが返る。

B ReloadEpg()
  EPG再読み込みを開始する
  開始できたときtrueが返る。

ReloadSetting( ネットワーク設定を読み込むか:B )
  設定を再読み込みする
  引数をtrueにするとEpgTimerSrv.iniの以下のキーも読み込まれる(HTTPサーバは再起動する)。
  EnableTCPSrv, TCPPort, EnableHttpSrv, HttpPort, HttpPublicFolder, HttpAccessControlList, EnableDMS

B EpgCapNow()
  EPG取得開始を要求する
  要求が受け入れられたときtrueが返る。

<チャンネル情報>のリスト GetChDataList()
  チャンネルスキャンで得たチャンネル情報のリストを取得する(onid>tsid>sidソート)
  つまり"Setting\ChSet5.txt"の内容。

<サービス情報>のリスト|nil GetServiceList()
  EPG取得で得た全サービス情報を取得する(onid>tsid>sidソート)
  プロセス起動直後はnil(失敗)。

{minTime:TIME, maxTime:TIME}|nil GetEventMinMaxTime( ネットワークID:I, TSID:I, サービスID:I )
  指定サービスの全イベントについて最小開始時間と最大開始時間を取得する
  開始時間未定でないイベントが1つもなければnil。

<イベント情報>のリスト|nil EnumEventInfo( {onid:I|nil, tsid:I|nil, sid:I|nil}のリスト [, {startTime:TIME|nil, durationSecond:I|nil} ] )
  指定サービスの全イベント情報を取得する(onid>tsid>sid>eidソート)
  リストのいずれかにマッチしたサービスについて取得する。
  onid,tsid,sidフィールドを各々nilとすると、各々すべてのIDにマッチする。
  プロセス起動直後はnil(失敗)。
  第2引数にイベントの開始時間の範囲を指定できる。
  ・開始時間がstartTime以上～startTime+durationSecond未満のイベントにマッチ
  ・空テーブルのときは開始時間未定のイベントにマッチ

<イベント情報>のリスト|nil SearchEpg( <自動予約検索条件> )
<イベント情報>|nil SearchEpg( ネットワークID:I, TSID:I, サービスID:I, イベントID:I )
  イベント情報を検索する
  1引数のときは<自動予約検索条件>にマッチしたイベントを取得する。
  4引数のときは指定イベントを取得する。なければnilが返る。
  プロセス起動直後はnil(失敗)。

B AddReserveData( <予約情報> )
  予約を追加する
  失敗時はfalse。

B ChgReserveData( <予約情報> )
  予約を変更する
  失敗時はfalse。

DelReserveData( 予約ID:I )
  予約を削除する

<予約情報>のリスト GetReserveData()
<予約情報>|nil GetReserveData( 予約ID:I )
  予約を取得する(reserveIDソート)
  無引数のときは全予約を取得する。
  1引数のときは指定予約を取得する。なければnilが返る。

<録画済み情報>のリスト GetRecFileInfo()
<録画済み情報>|nil GetRecFileInfo( 情報ID:I )
  録画済み情報を取得する(idソート)
  無引数のときは全情報を取得する。
  1引数のときは指定情報を取得する。なければnilが返る。

DelRecFileInfo( 情報ID:I )
  録画済み情報を削除する

<チューナ予約情報>のリスト GetTunerReserveAll()
  チューナごとの予約の割り当て情報を取得する(tunerIDソート)
  ただし、リストの最終要素はチューナ不足の予約を表す。

<録画プリセット>のリスト EnumRecPresetInfo()
  録画プリセット情報を取得する(idソート)
  少なくともデフォルトプリセット(id==0)は必ず返る。

<自動予約登録情報>のリスト EnumAutoAdd()
  自動予約登録情報を取得する(dataIDソート)

<自動予約(プログラム)登録情報>のリスト EnumManuAdd()
  自動予約(プログラム)登録情報を取得する(dataIDソート)

DelAutoAdd( 登録ID:I )
  自動予約登録情報を削除する

DelManuAdd( 登録ID:I )
  自動予約(プログラム)登録情報を削除する

B AddOrChgAutoAdd( <自動予約登録情報> )
  自動予約登録情報を追加/変更する
  dataIDフィールドが0のとき追加の動作になる。
  失敗時はfalse。

B AddOrChgManuAdd( <自動予約(プログラム)登録情報> )
  自動予約(プログラム)登録情報を追加/変更する
  dataIDフィールドが0のとき追加の動作になる。
  失敗時はfalse。

I GetNotifyUpdateCount( 通知ID:I )
  更新通知のカウンタを取得する
  0から始まってイベントが発生するたびに1だけ増える数値が返る。
  取得できない通知IDを指定すると-1が返る。
  通知ID:
    1=EPGデータが更新された
    2=予約情報が更新された
    3=録画済み情報が更新された
    4=自動予約登録情報が更新された
    5=自動予約(プログラム)登録情報が更新された

<ファイル情報>のリスト ListDmsPublicFile()
  (document_root)/dlna/dms/PublicFileにあるファイルをリストする
  「DLNAのDMSぽい機能」用。

■テーブル定義■
<チャンネル情報>={
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  serviceType:I=サービスタイプ
  partialFlag:B=限定受信サービス(ワンセグ)かどうか
  serviceName:S=サービス名
  networkName:S=ネットワーク名
  epgCapFlag:B=EPGデータ取得対象かどうか
  searchFlag:B=検索時のデフォルト検索対象サービスかどうか
}

<サービス情報>={
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  service_type:I=サービス形式種別
  partialReceptionFlag:B=限定受信サービス(ワンセグ)かどうか
  service_provider_name:S=事業者名
  service_name:S=サービス名
  network_name:S=ネットワーク名
  ts_name:S=TS名
  remote_control_key_id:I=リモコンキー識別
}

<イベント情報>={
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  eid:I=イベントID
  startTime:TIME|nil=開始時間(不明のときnil)
  durationSecond:I|nil=総時間(不明のときnil)
  freeCAFlag:B=ノンスクランブルフラグ
  shortInfo:{
    event_name:S=イベント名
    text_char:S=情報
  }|nil=EPG基本情報(ないときnil、以下同様)
  extInfo:{
    text_char:S=詳細情報
  }|nil=EPG拡張情報
  contentInfoList:{
    content_nibble:I=大分類*256+中分類
    user_nibble:I=独自ジャンル大分類*256+独自ジャンル中分類
  }のリスト|nil=EPGジャンル情報
  componentInfo:{
    stream_content:I=コンポーネント内容
    component_type:I=コンポーネント種別
    component_tag:I=コンポーネントタグ
    text_char:S=コンポーネント記述
  }|nil=EPG映像情報
  audioInfoList:{
    stream_content:I=以下、STD-B10音声コンポーネント記述子を参照
    component_type:I=*
    component_tag:I=*
    stream_type:I=*
    simulcast_group_tag:I=*
    ES_multi_lingual_flag:B=*
    main_component_flag:B=*
    quality_indicator:I=*
    sampling_rate:I=*
    text_char:S=*
  }のリスト|nil=EPG音声情報
  eventGroupInfo:{
    group_type:I=グループ種別。必ず1(イベント共有)
    eventDataList:{onid:I, tsid:I, sid:I, eid:I}のリスト
  }|nil=EPGイベントグループ情報
  eventRelayInfo:{
    group_type:I=グループ種別
    eventDataList:{onid:I, tsid:I, sid:I, eid:I}のリスト
  }|nil=EPGイベントグループ(リレー)情報
}

<予約情報>={
  title:S=番組名
  startTime:TIME=録画開始時間
  durationSecond:I=録画総時間
  stationName:S=サービス名
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  eid:I=イベントID
  comment:S=コメント
  reserveID:I=予約ID
  overlapMode:I=かぶり状態
  startTimeEpg:TIME=予約時の開始時間
  recSetting:<録画設定>=録画設定
}

<録画設定>={
  recMode:I=録画モード
  priority:I=優先度
  tuijyuuFlag:B=イベントリレー追従するかどうか
  serviceMode:I=処理対象データモード
  pittariFlag:B=ぴったり?録画
  batFilePath:S=録画後BATファイルパス
  suspendMode:I=休止モード
  rebootFlag:B=録画後再起動する
  startMargin:I|nil=録画開始時のマージン(デフォルトのときnil)
  endMargin:I|nil=録画終了時のマージン(デフォルトのときnil)
  continueRecFlag:B=後続同一サービス時、同一ファイルで録画
  partialRecFlag:I=物理CHに部分受信サービスがある場合、同時録画する(=1)か否(=0)か
  tunerID:I=強制的に使用Tunerを固定
  recFolderList={
    recFolder:S=録画フォルダ
    writePlugIn:S=出力PlugIn
    recNamePlugIn:S=ファイル名変換PlugIn
  }のリスト=録画フォルダパス
  partialRecFolder={
    (recFolderListと同じ)
  }のリスト=部分受信サービス録画のフォルダ
}

<録画済み情報>={
  id:I=情報ID
  recFilePath:S=録画ファイルパス
  title:S=番組名
  startTime:T=開始時間
  durationSecond:I=録画時間
  serviceName:S=サービス名
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  eid:I=イベントID
  drops:I=ドロップ数
  scrambles:I=スクランブル数
  recStatus:I=録画結果のステータス
  startTimeEpg:T=予約時の開始時間
  comment:S=コメント
  programInfo:S=.program.txtファイルの内容
  errInfo:S=.errファイルの内容
  protectFlag:B=プロテクト
}

<チューナ予約情報>={
  tunerID:I=チューナID
  tunerName:S=BonDriverファイル名
  reserveList:予約ID:Iのリスト=そのチューナに割り当てられた予約
}

<録画プリセット>={
  id:I=プリセットID
  name:S=プリセット名
  recSetting:<録画設定>=プリセット録画設定
}

<自動予約登録情報>={
  dataID:I=登録ID
  addCount:I=予約追加カウント(参考程度)。登録時はnilで良い
  searchInfo:<自動予約検索条件>=検索条件
  recSetting:<録画設定>=録画設定
}

<自動予約検索条件>={
  ### 以下のプロパティはEnumAutoAddなどの取得系メソッドでnilになることはない
  andKey:S=キーワード
  notKey:S=NOTキーワード
  regExpFlag:B|nil=正規表現モード(省略時false)
  titleOnlyFlag:B|nil=番組名のみ検索対象にする(省略時false)
  aimaiFlag:B|nil=あいまい検索モード(省略時false)
  notContetFlag:B|nil=対象ジャンルNOT扱い(省略時false)
  notDateFlag:B|nil=対象期間NOT扱い(省略時false)
  freeCAFlag:I|nil=スクランブル放送(0=限定なし(省略時),1=無料のみ,2=有料のみ)
  chkRecEnd:B|nil=録画済かのチェックあり(省略時false)
  chkRecDay:I|nil=録画済かのチェック対象期間(省略時0)
  contentList:{
    content_nibble:I=大分類*256+中分類
  }のリスト=対象ジャンル
  dateList:{
    startDayOfWeek:I=検索開始曜日(0=日,1=月...)
    startHour:I=開始時
    startMin:I=開始分
    endDayOfWeek:I=検索終了曜日(0=日,1=月...)
    endHour:I=終了時
    endMin:I=終了分
  }のリスト|nil=対象期間(省略時空リスト)
  serviceList:{
    onid:I=ネットワークID
    tsid:I=TSID
    sid:I=サービスID
  }のリスト|nil=対象サービス(省略時空リスト)
  ### 以下のプロパティは取得系メソッドで常にnil
  network:I|nil=対象ネットワーク(+1=地デジ,+2=BS,+4=CS,+8=その他。該当サービスがserviceListに追加される)
  days:I|nil=対象期間(現時刻からdays*24時間以内。SearchEpg以外では無視される)
  days29:I|nil=対象期間(現時刻からdays*29時間以内。互換用なので使用しないこと。SearchEpg以外では無視される)
}

<自動予約(プログラム)登録情報>={
  dataID:I=登録ID
  dayOfWeekFlag:I=対象曜日(+1=日,+2=月...)
  startTime:I=録画開始時間(00:00を0として秒単位)
  durationSecond:I=録画総時間
  title:S=番組名
  stationName:S=サービス名
  onid:I=ネットワークID
  tsid:I=TSID
  sid:I=サービスID
  recSetting:<録画設定>=録画設定
}

<ファイル情報>={
  id:I=このファイルがリストされた順番
  name:S=ファイル名
  size:I=ファイルサイズ
}
