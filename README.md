EpgDataCap_Bon
==============
**BonDriver based multifunctional EPG software**

Documents are stored in the 'Document' directory.  
Configuration files are stored in the 'ini' directory.

**このForkについて**

master ブランチは
* xtne6f氏 (https://github.com/xtne6f/EDCB) の work-plus-s ブランチ
* tkntrec氏 (https://github.com/tkntrec/EDCB) の my-build-s ブランチ
* nekopanda氏 (https://github.com/nekopanda/EDCB) の develop ブランチ
を merge しまくりつつ、各氏のサブブランチも先取りしつつ、
自分の好みも混ぜ合わせてみるキメラのようなブランチです。

work ブランチはダイアログのリサイズに対応させるために
Margin による絶対座標指定をやめたため、xaml を全面的に
書き換えることになったものです。
UIパーツの追加などは他のパーツに依存せずに追加できるため
修正が簡単になるのですが、上記の方々等からの marge 作業が
困難になることが予想されるため master ブランチと分けています。
そのうち腹をくくって master にマージするとは思います。


なるべく機能削除はしないマージをしていきたいところですが、
機能削除による削除なのか
修正による削除なのか
コンフリクトによる削除なのか
一つ一つ確認するのが難しい状況です。

README関連のファイルはコンフリクトしやすいので、あまり修正していません。
各氏のREADMEを参照してください。
* https://github.com/xtne6f/EDCB/tree/work-plus-s/Document
* https://github.com/tkntrec/EDCB/tree/my-build-s/Document
* https://github.com/nekopanda/EDCB/tree/develop/Document
