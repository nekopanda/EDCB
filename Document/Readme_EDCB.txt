BonDriverを使用したEPGデータの取得、予約録画を行うためのツール群です。


◆このプログラムを使用して発生した全ての問題に対して責任は持ちません。


■注意■
　すべてを新規作成したので人柱１０以前の過去バージョンとは互換性がなく
　なっています。
　設定ファイルも互換性がないため、人柱１０以前からのバージョンアップの
　場合は一度すべてを削除してください。
　まだ動作の不安定な部分やおかしい部分などが残っている可能性があります。
　スクランブルを解除する機能はありません。


■動作環境■
OS    ：Windows XP SP3以降
CPU   ：Intel Core 2 Duo以上推奨
メモリ：1GB以上推奨

動作には.NetFramework4.0とVC++2010のランタイムが必要です。
他に各BonDriverや外部モジュールで使用するランタイムが必要な場合があります。

Microsoft .NET Framework 4 (スタンドアロンのインストーラー)
http://www.microsoft.com/downloads/details.aspx?familyid=0A391ABD-25C1-4FC0-919F-B21F31AB88B7&displaylang=ja
Microsoft Visual C++ 2010 再頒布可能パッケージ (x86)
http://www.microsoft.com/downloads/details.aspx?FamilyID=a7b7a05e-6de6-4d3a-a423-37bf0912db84&displaylang=ja
Microsoft Visual C++ 2010 再頒布可能パッケージ (x64)
http://www.microsoft.com/downloads/details.aspx?familyid=BD512D9E-43C8-4655-81BF-9350143D5867&displaylang=ja


■64bitOSへの対応■
　x64フォルダにあるモジュールは64bitでビルドしたモジュールになっていま
　す。64bitネイティブで動作することが可能です。
　64bit版を使用するにはBonDriverも64bitでビルドされている必要があります。
　32bitのモジュールが１つでも必要な場合は使用できません。


■ソース（src.zip）の取り扱いについて■
　特にGPLとかにはしないのでフリーソフトに限っては自由に改変してもらった
　り組み込んでもらって構わないです。
　改変したり組み込んだりして公開する場合は該当部分のソースぐらいは一緒
　に公開してください。（強制ではないので別に公開しなくてもいいです）
　商用、シェアウェアなどに許可なく組み込むのは不可です。


■EpgDataCap3.dll、CtrlCmdCLI.dll、SendTSTCP.dll、BonDriver_TCP.dllの
取り扱いについて
　フリーソフトに組み込む場合は特に制限は設けません。
　このdllを使用したことによって発生した問題について保証は一切行いません。
　商用、シェアウェアなどに許可なく組み込むのは不可です。


■twitter.dllの取り扱いについて
　そのままEpgDataCap_Bon以外のソフトで使用するのは不可です。
　APIキーを所得し、ビルドし直したものを使用するのは構いません。
　商用、シェアウェアなどに許可なく組み込むのは不可です。


■基本的な使用準備■
　１．64bitOSで64bitネイティブで動作させるには「x64」フォルダを、
　　　それ以外の場合は「x86」フォルダを使用してください。

　２．使用デバイス用のBonDriverを用意し、BonDriver フォルダに入れる。
　　　BonDriverによってはiniファイルなどで設定できる内容があるので、あ
　　　らかじめ設定をしておく

　３．EpgDataCap_Bon.exeを起動し、「設定」→「基本設定」タブで「設定関
　　　係保存フォルダ」を設定する。

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


■バグ報告について
　http://2sen.dip.jp/dtv/の掲示板をバグ報告用として利用させて頂いてます
　が、アクセス規制の煽りを食らって書き込みやアップロードできない状態に
　なることがあります。
　バグ報告に関するレスは修正という形で取らせてもらうのが大半になると思
　います。
　バグ報告以外の内容に関しては基本的に対応は行いません。
　要望に関しては簡単にできそうな物なら組み込んでいこうと考えています。
　◆バグ報告には、使用環境（ハード、アプリ、BonDriver、バージョンなど）、
　◆何から予約登録を行い、どのような設定でだったかなどの詳細も記載願い
　◆ます。
　◆多種多様な環境を構築できるようになってきたため、詳細の記載なき報告
　◆は基本的に確認を行いません（行えません）。
　◆DbgView、DbgMonなどを使用してログの取得を行ってもらえると、解決の糸
　◆口が見つかりやすくなります。
　◆再現性のある予約録画に関する内容はログの取得を行ってください。ログ
　◆のないものは基本的に調査を行えません。


■動作確認環境■
OS    　　：Windows7 Ultimate 64bit版
CPU   　　：Intel Core i7 860
メモリ　　：4GB
VGA       ：ATI Radeon HD 4670
チューナー：PT1×1、PT2×1
