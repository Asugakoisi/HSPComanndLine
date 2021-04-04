# HSPComanndLine
HSPのコマンドラインインターフェース。hspcのパクリ。  
**HSP Dish や AHT のコンパイルはできません。従来通りHSPエディタからコンパイルしてください。**  
注意は見てください。

# やること
[Release](https://github.com/Asugakoisi/HSPComanndLine/releases/tag/v0.7.0.0) をダウンロードして、中身をHSPシステムフォルダにコピーするか、  
以下の二つのことをしてください。  
1. `src\json\ja`と`src\json\en`ディレクトリを丸ごと`HSPシステムフォルダ`にコピーする。
2. `src\bin\win`ディレクトリの中身を`HSPシステムフォルダ`にコピーする。
3. `src\bin\win`ディレクトリの中身を`HSPシステムフォルダ\en`にコピーする。  

Linuxでは`win`ディレクトリではなく、`linux`ディレクトリの中身をコピーしてください。
  
つまり、こうなっていればいいです。  
HSPディレクトリ:  
![HSPディレクトリのスクショ1](docs/img/hspdirectory1.png)  
![HSPディレクトリのスクショ2](docs/img/hspdirectory2.png)  
  
HSPディレクトリ\en:  
![HSPディレクトリ\enのスクショ](docs/img/hspdirectory_en.png)  
  
HSPディレクトリ\ja:  
![HSPディレクトリ\jaのスクショ](docs/img/hspdirectory_ja.png)  

ヘルプ機能を使わない人は `ja`と`en`ディレクトリはいりません。  

# 注意
hspcui を使うには、[HSP3.6β4](https://www.onionsoft.net/wp/archives/3274)以降が必要です。(strmap機能対応のため。)  
また、**Linux版では`-P`,`-m`,`-w`,`--platform`オプションが使用できません**。  
さらに、Linuxでは文字コードが`UTF-8`であることから`-u`オプションが常に有効化されます。  

# 英語モード(--lang=en)について
ここで表示される英語は正しいものとは限りません。  
修正案があれば [Issues](https://github.com/Asugakoisi/HSPComanndLine/issues) で報告してください。  

# 使い方
こいつには残念ながら`-CPmD`とか`command`を理解できる能力はないので、一個づつ`-C -P -m -D`と入力してください。  
基本的に、hspc と hspcmp.exe のオプションに対応しています。  
コマンドの説明は `hspcui --help --ls`を参照してください。  

# ヘルプ機能
オプションの説明や使用例を見ることができます。  
`hspcui --help 調べるオプション`とすることで検索できます。  
ただし、`調べるオプション`にはハイフンや`???`はいりませんが、オプションに`=`が含まれるものは`=`の省略はできません。  
例)
```cmd
hspcui --help a
hspcui --help o
hspcui --help profile=
```

# テンプレート機能
これは開発でよく使うディレクトリ＆ファイルを簡単に複製できるようになるユーリティ機能です。  
hspc.exeがあるディレクトリ上に templates ディレクトリ が存在すれば、その中のディレクトリ達をテンプレート元として認識します。  
  
 `--tempo=` オプションについて  
このオプションは `--temp` `--temp=` `--tempa=`オプションの時に適応されます。  
`--temp` `--temp=`オプションでは、カレントディレクトリにコピーする際のディレクトリ名が変更され、 `--tempa=`オプションでは、テンプレートディレクトリにコピーする際のディレクトリ名が変更されます。  
例)  
`--temp=test --tempo=test_copy`の時   
- `template\test`の内容が `カレントディレクトリ\test_copy`にコピーされる。  

`--tempa=test --tempo=test_copy`の時  
- `カレントディレクトリ\test`の内容が `template\test_copy`にコピーされる。  

# プロファイル機能  
この機能は hspcui 独自の機能です。  
よく使うオプションをこの機能を使うことで省略することができます。  
プロファイルは、ユーザーファルダ(例えば、`C:\Users\Asugakoisi`)にある hspcuiconfig.json に登録できます。  
例）hspcuiconfig.json  
```JSON
{
  "$schema": "https://raw.githubusercontent.com/Asugakoisi/HSPComanndLine/main/src/json/hspcuiconfig-schema.json",
  "ver": "0.6.0.0-0.7.0.0",
  "profiles": [
    {
      "id": 0,
      "name": "Build",
      "options": [
        "-P",
        "-m",
        "-C",
        "-D"
      ]
    }
  ]
}
```
この時、  
```cmd
hspcui --profile=0 source.hsp
hspcui --profile=Build source.hsp
```
は  
```cmd
hspcui -P -m -C -D source.hsp
```
と同じになります。  
プロファイルで**指定できないオプション**は以下の表にある通りです。  
| 指定できないオプション |
| :----: |
| --help |
| --online |
| --lang= |

# AssemblyInfo.hsp 作成機能(プレリリース)
**この機能は予告なく変更される恐れがあります。**  
この機能は hspcui 独自の機能です。  
この機能により、`-o`,`--outname`オプションを使用して、作成する自動実行ファイル名を指定できるようになりました。  
また、`-a`,`-i` オプションでソースコードが`UTF-8`ときにはランタイムが自動的に`hsp3utf`になるようになります。  
さらに、`--platform=`オプションを指定することで 64bit か 32bit アプリケーションであるかを指定できます。  
  
実際は適宜以下のマクロを`AssemblyInfo.hsp`に追加して、ソースファイルの一行目に`#include "AssemblyInfo.hsp"`を追加しているだけです。  
```HSP
#include "hsp3utf.as"
#include "hsp3_64.as"
#packopt name "ファイル名"
```  
もちろん、ソースコード側での指定が優先されます。  

## `--notasminfo`オプションについて
このオプションを指定することで AssemblyInfo.hsp を作成しないようにできます。  
ただし、自動実行ファイル名を指定できませんし、`--platform=`オプションは無効化されます。  
また、ソースコードが`UTF-8`の場合はソースコード側で`hsp3utf`ランタイムの指定を忘れないでください。  

## linuxにおける AssemblyInfo.hsp について
現在 Linux ではこの機能が効果を発揮する事はないため、コンパイル時には`--notasminfo`オプションをつけることを推奨します。  

# HSPCタスク  
MSBuildでこのタスクを使うとHSPスクリプトファイルのコンパイルができるようになります。  
また使用する際は、`HSPC.dll`と`hspcmp.dll`を同じディレクトリに配置してください。  
  
例）sample.hspproj
```xml
<Project DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003"
         ToolsVersion="16">
  <PropertyGroup>
    <HSPPath>C:\hsp36beta</HSPPath>
  </PropertyGroup>
  <UsingTask TaskName="HSPC.Hspc" AssemblyFile="$(HSPPath)\HSPC.dll" Architecture="x86"/>
  <ItemGroup>
    <Compile Include="source.hsp"/>
  </ItemGroup>
  <Target Name="Build">
    <Hspc SourceFile="$(Compile)" SystemDirectory="$(HSPPath)"/>
  </Target>
</Project>
```  
  
Hspcタスクの属性一覧  
| 属性名 | 型 | デフォルト | 説明 |  
| :----: | :----: | :----: | ---- |
| AddDegugInfo | bool | false | デバッグ情報を付与してコンパイルします。 |
| AsHSP26 | bool | false | HSP ver2.6 としてコンパイルします。 |
| CommonDirectory | string | string.Empty | common フォルダを指定します。 |
| CreateAssmblyInfo | bool | true | AssemblyInfo.hsp ファイルを生成するかどうか。 |
| DeleteFiles | bool | false | コンパイル成果物の内、AssemblyInfo.hsp と自動実行ファイル以外を削除します。 |
| MakeExe | bool | false | 自動実行ファイルを作成します。(packfile は自動生成されます) |
| Nologo | bool | false | 著作権情報を表示しません。 |
| OnDebugWindow | bool | false | デバッグウインドウ表示フラグを設定します。 |
| Output | string | string.Empty | コンパイルした出力するファイル名を指定します。 |
| Platform | string | x86 | プラットフォームを指定します。指定できるのは x86 か x64 の二つです。 |
| RefName | string | string.Empty | 表示するソースファイル名を変更します。 |
| RuntimeDirectory | string | string.Empty | runtime フォルダを指定します。 |
| SourceFile | string | null | 必須項目。コンパイルするファイルを指定します。 |
| SystemDirectory | string | null | 必須項目。HSPシステムフォルダを指定します。 |

カスタムタスクの作成については[タスクの作成](https://docs.microsoft.com/ja-jp/visualstudio/msbuild/task-writing?view=vs-2019)を見てください。  

# オプション一覧  
一文字オプション  
| オプション | 説明 |
| :----: | ---- |
| -a | ソースコードの文字コードを自動で判断します。 |
| -c | HSP ver2.6 としてコンパイルします。 |
| -C | カレントディレクトリをソースファイルが存在するディレクトリに変更します。 |
| -d | デバッグ情報を付与してコンパイルします。 |
| -D | コンパイル成果物の内、AssemblyInfo.hsp と自動実行ファイル以外を削除します。 |
| -E | hspcui 内で発生したエラーを無視して実行を続けます。 |
| -h??? | 指定したHSP命令を検索します。 |
| -i | ソースファイルが utf-8 であることを示します。 |
| -j | ソースファイルが shift_jis であることを示します。 |
| -m | 自動実行ファイルを作成します。 |
| -o??? | コンパイルした出力するファイル名を指定します。 |
| -p | プリプロセスのみを行います。 |
| -P | packfile を指定されたソースコードから作成します。<br>-D オプションを使用することで削除できます。 |
| -r | 指定されたソースファイルの実行をし、終了コードを出力します。<br>また、-mオプション指定時にはコンパイルした自動実行ファイルを実行します。 |
| -r= | 指定されたソースファイルを指定された引数を与えて実行し、終了コードを出力します。<br>また、コンパイル時にはその成果物を実行します。 |
| -s | 指定されたソースファイルから strmap を作成します。 |
| -u | ソースファイルをUTF-8にエンコードし、文字列データをUTF-8形式で出力します。 |
| -w | デバッグウインドウ表示フラグを設定します。 |

複数文字オプション  
| オプション | 説明　|
| :-----: | ---- |
| --compath= | common フォルダを指定します。 |
| --lang= | 実行時の言語を指定します。指定できる言語は ja か en です。| 
| --license | hspcui のライセンスを表示します。 |
| --nologo | 著作権情報を表示しません。 |
| --notasminfo | AssemblyInfo.hsp ファイルを生成しません。 |
| --oldcmpmes | 従来通りのコンパイルメッセージを表示します。 |
| --online | -h オプションの検索時にオンラインで検索します。 |
| --outname= | コンパイルした出力するファイル名を指定します。 |
| --platform= | プラットフォームを指定します。指定できるのは x86 か x64 の二つです。 |
| --profile= | 指定したプロファイルを実行します。 |
| --profilea= | 指定されたプロファイルID又はプロファイル名でプロファイルを追加します。 |
| --profiled= | 指定されたプロファイルID又はプロファイル名でプロファイルを削除します。 |
| --refname= | 表示するソースファイル名を変更します。 |
| --rtmpath= | runtime フォルダを指定します。 |
| --see | hspcui で指定したオプションを表示します。 |
| --syspath= | HSPシステムフォルダを指定します。 |
| --temp | templates\default ディレクトリをカレントディレクトリにコピーします。 |
| --temp= | 指定されたテンプレートディレクトリをカレントディレクトリにコピーします。 |
| --tempd | templates\\default ディレクトリを削除します。 |
| --tempd= | 指定されたテンプレートディレクトリを削除します。 |
| --tempa= | 指定したディレクトリを templates ディレクトリにコピーします。 |
| --tempo= | コピー先でのディレクトリ名を指定します。 |
| --tmppath= | templates フォルダを指定します。 |
| --version | hspcui のバージョンを表示します。 |


# 履歴
04/04 Version 0.7.0.0 公開  
- 共通事項
  - `-u`オプションの動作が変更されました。
  - `--refname`, `--tmppath=`オプションが追加されました。  

- Linux版
  - `-r0`オプションに対応しました。
  - `--platform=`オプションが無効化されました。
  - `-u`オプションが常に有効化されるようになりました。

03/18 Version 0.6.0.0 公開  
- プロファイルのjsonスキーマが変わりました。
- コマンドラインからプロファイルの登録・削除ができるようになりました。
- Linux版の hspcui を公開しました。

02/26 Version 0.5.1.0 公開  
- ヘルプが表示されないエラーを修正しました。

02/12 Version 0.5.0.0 公開  
- MSBuildでのHSPコンパイルタスクの`HSPC.dll`が公開されました。  

01/31 Version 0.4.0.0 公開  
- コンパイラメッセージが MSBuild 風になりました。
- `--nologo`オプションでコンパイラの著作権情報を非表示にできます
- `--oldcmpmes`オプションでコンパイラメッセージの表示を従来のものにできます

01/20 Version 0.3.0.0 公開  
- `--profile=`オプションでプロファイル名が指定できるようになりました
- `--see`オプションで hspcui に指定したオプションが出力されます
  - `--profile=`オプションと併用することで指定したプロファイルのコマンドを表示することができます
- `--notasminfo` オプションで`AssemblyInfo.hsp`を作成しないようにできます

01/16 Version 0.2.1.0 公開
- `--profile=`オプションで`--platform=x64`を指定して、そのあと`--platform=x86`を指定しても変更されない  
- `AssemblyInfo.hsp` が作成されないことがある  

01/16 Version 0.2.0.0 公開  
- `--platform=` オプションの追加  
- `AssemblyInfo.hsp`を作成する機能の追加  
- 上記の機能の追加による `-o`,`--outname=`,`-a`,`-i` オプションの動作変更  
- `--help --ls` オプションで表示される情報を減らしました  

01/11 Version 0.1.1.0 公開  
- `--version`、`--license` オプションの追加  
- `--version`、`--license` オプションのヘルプ追加  

01/10 Version 0.1.0.0 公開  

# こっちもみてね
[hspc の公開ページ](http://dev.onionsoft.net/seed/info.ax?id=1392)  
[HSP 公式サイト](http://hsp.tv/index2.html)
[MSBuild](https://docs.microsoft.com/ja-jp/visualstudio/msbuild/msbuild?view=vs-2019)
