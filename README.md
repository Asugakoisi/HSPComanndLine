# HSPComanndLine
HSPのコマンドラインインターフェース。hspcのパクリ。

# やること
[Release](https://github.com/Asugakoisi/HSPComanndLine/releases/tag/v0.1.1.0) をダウンロードして、中身をHSPシステムフォルダにコピーするか、  
以下の3ステップをやる。  
1. src\json\ja ディレクトリを丸ごとHSPシステムフォルダにコピーする。
2. src\json\en の中身を src\bin\en にコピーする
3. src\binディレクトリの中身を全てHSPシステムフォルダにコピーする。
  
つまり、こうなっていればいい。  
HSPディレクトリ:  
![HSPディレクトリのスクショ1](docs/img/hspdirectory1.png)  
![HSPディレクトリのスクショ2](docs/img/hspdirectory2.png)  
  
HSPディレクトリ\en:  
![HSPディレクトリ\enのスクショ](docs/img/hspdirectory_en.png)  
  
HSPディレクトリ\ja:  
![HSPディレクトリ\jaのスクショ](docs/img/hspdirectory_ja.png)  

# 注意
これを使うには、[HSP3.6β4](https://www.onionsoft.net/wp/archives/3274)以降が必要です。(strmap機能を使うため。)  

# 英語モード(--lang=en)について
ここで表示される英語は正しいものとは限りません。  
修正案があればIssuesで報告してください。  

# 使い方
こいつには残念ながら`-CPmD`とか`command`を理解できる能力はないので、一個づつ`-C -P -m -D`と入力してください。  
基本的に、hspc と hspcmp.exe のオプションに対応しております。  
コマンドの説明は `hspc --help --ls`を参照してください。  

# AssemblyInfo.hsp 作成機能
この機能は hspcui 独自の機能です。  
この機能により、`-o`,`--outname`オプションを使用して作成する自動実行ファイル名を指定できるようになりました。  
また、ランタイム(`hsp3utf`、`hsp3_64`)を指定することができるようになりました。
そのために、`-a`,`-i` オプションでソースコードが`UTF-8`ときにはランタイムが自動的に`hsp3utf`になります。   
  
実際は適宜以下のマクロを`AssemblyInfo.hsp`に追加して、ソースファイルの一行目に`#include "AssemblyInfo.hsp"`を追加しているだけです。  
```HSP
#include "hsp3utf.as"
#include "hsp3_64.as"
#packopt name "ファイル名"
```  

もちろん、ソースコード側での指定が優先されます。  

# 履歴
01/16 Version 0.2.0.0 公開  
- `--platform=` オプションの追加  
- AssemblyInfo.hsp を作成する機能の追加  
- 上記の機能の追加による `-o`,`--outname=`,`-a`,`-i` オプションの動作変更  
- `--help --ls` オプションで表示される情報を減らしました。  

01/11 Version 0.1.1.0 公開  
- `--version`、`--license` オプションの追加  
- `--version`、`--license` オプションのヘルプ追加  

01/10 Version 0.1.0.0 公開  

# こっちもみてね
[hspc の公開ページ](http://dev.onionsoft.net/seed/info.ax?id=1392)  
[HSP 公式サイト](http://hsp.tv/index2.html)
