EpgDataCap_Bon
==============
**BonDriver based multifunctional EPG software**

Documents are stored in the 'Document' directory.  
Configuration files are stored in the 'ini' directory.

**このフォークについて**

このフォークはxtne6fさんのフォーク([xtne6f@work-plus-s](https://github.com/xtne6f/EDCB/tree/work-plus-s))にちょびっとだけパッチを追加するブランチ(フォーク)です。機能に関する説明やビルド方法などはフォーク元の[xtne6f@work-plus-s](https://github.com/xtne6f/EDCB/tree/work-plus-s)を参照してください。フォーク元との関係は[xtne6f@NetworkGraph](https://github.com/xtne6f/EDCB/network)で確認することが出来ます。

なお、このフォークでは主にEpgTimerへの変更を行っていますが、あちこちコードを改変している上、かなりデンジャラスなコミット([ff60480](https://github.com/tkntrec/EDCB/commit/ff6048074a4a609fb22c78361682a3cb4cf4a593)とか)も混ざっていますので、使用の際はご注意を。

**主な変更点について**

* 検索(自動予約登録)と右クリックメニューまわりを中心に若干の機能追加・変更をしています。
* 「EPG予約条件」ウィンドウ関係
  * 「自動予約登録を削除」「予約全削除」「前へ」「次へ」ボタン追加。
  * 検索結果一覧の右クリックに「番組名で再検索(サブウィンドウ)」を追加。
* 右クリックメニュー関係
  * 各画面の右クリックメニューを「番組表」をベースにだいたい揃えた。
  * 右クリックメニュー項目の表示/非表示を選択出来るようにした。  
[設定]-[動作設定]-[その他]の[右クリックメニューの設定]から変更出来ます。
* その他
  * 各設定ウィンドウをESCで閉じられるようにした。
  * 追加設定の説明などを含むコミット  
設定画面ではなく、設定ファイル(XML)で直接指定するオプションについての説明です。  
[f52b17c](https://github.com/tkntrec/EDCB/commit/f52b17cd782a91b6c7da14069f986b428d0f4ddd) 無効予約アイテムの文字色の変更  
[44dd565](https://github.com/tkntrec/EDCB/commit/44dd565cc3c124f4db456b29343447b6dc11975a) 状態表示列の文字色の変更  
[1c22086](https://github.com/tkntrec/EDCB/commit/1c220862bc75b84465d1c524227dbac1c8ee3e3b) 各設定画面でのフォルダ選択時に「ファイル選択ダイアログ」を使用するオプション  
[c62755d](https://github.com/tkntrec/EDCB/commit/c62755d4869eeb808b5ba67ebac203413be3a12a) 「番組表へジャンプ」の強調表示時間の変更  
[c8e4bcf](https://github.com/tkntrec/EDCB/commit/c8e4bcfc0bda6df446f6cfdc2ff4f16935ea49b7) EpgTimerNWでサーバとの接続維持を試みるオプション  
[1326189](https://github.com/tkntrec/EDCB/commit/1326189b55bdac2bfb24e2f9dad68d702bd9a0ec) エラー列の背景色の変更  
[e3ebe61](https://github.com/tkntrec/EDCB/commit/e3ebe61addbebb357a7fad95a506014cec7c80c7) 録画済み一覧のドロップのエラー表示の閾値  

**ブランチついて**

このフォークのブランチは再作成(リベース)などで構成が変わることがあります。  
[branch:my-build-s](https://github.com/tkntrec/EDCB/tree/my-build-s)以外、特にビルドする意味はありません。  
[branch:my-ui-s](https://github.com/tkntrec/EDCB/tree/my-ui-s)はフォーク元[xtne6f@work-plus-s](https://github.com/xtne6f/EDCB/tree/work-plus-s)とのEpgTimer側の差分、[branch:my-work-s4](https://github.com/tkntrec/EDCB/tree/my-work-s4)はEpgTimerSrv側の差分です。
