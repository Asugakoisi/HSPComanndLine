using static HspcuiBase.Utils.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HspcuiBase.Compile
{
    public class CompileTextParser
    {
#pragma warning disable IDE0090 // 'new(...)' を使用する
        private readonly Dictionary<int, string> cpMessagesRegexEn = new Dictionary<int, string>() 
        {
            //Mesf
            { 92, @"Label definition not found \[(?<str>.*?)\]" },//[%s]
            { 93, @"Function not found \[(?<str>.*?)\]" },//[%s]
            { 96, @"Identifier '(?<str1>.*?)' has already defined in line (?<num>\d+) in \[(?<str2>.*?)\]" },//[%s]
            { 97, @"(?<str1>.*?)\((?<num1>\d+)\) : error (?<num2>\d+) : (?<str2>.*?) \((?<num3>\d+)行目\)" },
            { 98, "--> (?<str>.*?)" }, //97とセット
            //warning
            //Mesf
            { 101, @"Uninitalized variable ((?<str>.*?))" },//(%s).
            { 102, @"Warning:Old deffunc expression at (?<num>\d+)\.\[(?<str>.*?)\]" },//%d.[%s]
            //Mesf
            { 203, "Delete func (?<str>.*?)" },
            { 204, "Delete module (?<str>.*?)" },
            { 205, "String pool:(?<str>.*?)" },
            { 206, @"Code size \((?<num1>\d+)\) String data size \((?<num2>\d+)\) param size \((?<num3>\d+)\)" },
            { 207, @"Vars \((?<num1>\d+)\) Labels \((?<num2>\d+)\) Modules \((?<num3>\d+)\) Libs \((?<num4>\d+)\) Plugins \((?<num5>\d+)\)" },
            { 208, @"Output extra data field \((?<num>\d+)\)." },
            { 209, @"No error detected. \(total (?<num>\d+) bytes\)" }
        };
        private readonly Dictionary<int, string> cpMessagesRegexJa = new Dictionary<int, string>() 
        {
            //Mesf
            { 92, @"ラベルの定義が存在しません \[(?<str>.*?)\]" },//[%s]
            { 93, @"関数が定義されていません \[(?<str>.*?)\]" },//[%s]
            { 96, @"識別子「(?<str1>.*?)」の定義位置: line (?<num>\d+) in \[(?<str2>.*?)\]" },//[%s]
            { 97, @"(?<str1>.*?)((?<num1>\d+)) : error (?<num2>\d+) : (?<str2>.*?) ((?<num3>\d+)行目)" },
            { 98, "--> (?<st1>.*?)" }, //97とセット
            //warning
            //Mesf
            { 101, @"未初期化の変数があります\((?<str>.*?)\)" },//(%s)
            { 102, @"警告:古いdeffunc表記があります 行(?<num>\d+)\.\[(?<str>.*?)\]" },//%d.[%s]
            //Mesf
            { 203, "未使用の外部DLL関数の登録を削除しました (?<str>.*?)" },//%s
            { 204, "未使用のモジュールを削除しました (?<str>.*?)" },//%s
            { 205, "String pool:(?<str>.*?)" },
            { 206, @"Code size \((?<num1>\d+)\) String data size \((?<num2>\d+)\) param size \((?<num3>\d+)\)" },
            { 207, @"Vars \((?<num1>\d+)\) Labels \((?<num2>\d+)\) Modules \((?<num3>\d+)\) Libs \((?<num4>\d+)\) Plugins \((?<num5>\d+)\)" },
            { 208, @"Output extra data field \((?<num>\d+)\)." },
            { 209, @"No error detected. \(total (?<num>\d+) bytes\)" }
        };
        private readonly Dictionary<int, string> ppMessagesRegexEn = new Dictionary<int, string>()
        {
            //seterror
            { 48, "illegal macro parameter (?<str>.*?)" },
            { 49, @"macro syntax error \[(?<str>.*?)\]" },        //struct,macro展開[%s]
            { 59, @"invalid symbol \[(?<str>.*?)\]" },            //Const,Enum,Define,undef[%s]
            { 85, @"symbol in use \[(?<str>.*?)\]" },             //Const,Enum,Define,Defcfunc,deffunc,modfunc,struct,func,cmd,usecom,module[%s]
            //Mesf
            { 91, @"(?<num>\d+) unresolved macro\(s\)\.\[(?<str>.*?)\]" },//[%s]
            { 95, @"Source file not found.\[(?<str>.*?)\]" },//[%s]
            { 99, @"Error:(?<str1>.*?) in line (?<num>\d+) \[(?<str2>.*?)\]" },
            //Mesf
            { 210, @"Use file \[(?<str>.*?)\]" }//[%s]
        };
        private readonly Dictionary<int, string> ppMessagesRegexJa = new Dictionary<int, string>()
        {
            //seterror
            { 48, "(?<str>.*?) は不正なマクロパラメータです" },
            { 49, @"マクロの文法が正しくありません \[(?<str>.*?)\]" },        //struct,macro展開[%s]
            { 59, @"無効な名前です \[(?<str>.*?)\]" },            //Const,Enum,Define,undef[%s]
            { 85, @"定義済みの識別子は使用できません \[(?<str>.*?)\]" },//Const,Enum,Define,Defcfunc,deffunc,modfunc,struct,func,cmd,usecom,module[%s]
            //Mesf
            { 91, @"スタックが空になっていないマクロタグが(?<num>\d+)個あります \[(?<str>.*?)\]" },//[%s]
            { 95, @"スクリプトファイルが見つかりません \[(?<str>.*?)\]" },//[%s]
            { 99, @"Error:(?<str1>.*?) in line (?<num>\d+) \[(?<str2>.*?)\]" },
            //Mesf
            { 210, @"使用ファイル \[(?<str>.*?)\]" }//[%s]
        };
        private readonly Dictionary<int, string> dpmMessagesRegexEn = new Dictionary<int, string>()
        {
            //error
            { 1, @"No pack file \[(?<str>.*?)\]." },
            { 2, @"No such file.\[(?<str>.*?)\]" },
            { 3, @"Listing file \[(?<str>.*?)\] not found." },
            { 4, @"No File \[(?<str>.*?)\]" },
            { 7, @"Write error \[(?<str>.*?)\]." },
            //info
            { 201, @"Searching pack file \[(?<str>.*?)\]." },
            { 202, @"Take \[(?<str>.*?)\]\((?<num>\d+)\) from pack." },
            { 203, @"Listing file \[(?<str>.*?)\] analysis." },
            { 204, @"(?<str>.*?),(?<num16>[a-fA-F0-9]+),(?<num1>\d+),(?<num2>\d+)" },
            { 206, @"Make custom execute file \[(?<str>.*?)\]\((?<num>\d+)\)." }
        };
        private readonly Dictionary<int, string> dpmMessagesRegexJa = new Dictionary<int, string>()
        {
            //error
            { 1, @"パックファイルが見つかりません。\[(?<str>.*?)\]" },
            { 2, @"ファイルがありません。\[(?<str>.*?)\]" },
            { 3, @"リストファイル \[(?<str>.*?)\] が見つかりません。" },
            { 4, @"ファイルが見つかりません。 \[(?<str>.*?)\]" },
            { 7, @"書き込みエラー \[(?<str>.*?)\]." },
            //info
            { 201, @"パックファイル \[(?<str>.*?)\] を検索します。" },
            { 202, @"パックファイルから \[(?<str>.*?)\]\((?<num>\d+)\) を取得しました。" },
            { 203, @"リストファイル \[(?<str>.*?)\] を解析します." },
            { 204, @"(?<str>.*?),(?<num16>[a-fA-F0-9]+),(?<num1>\d+),(?<num2>\d+)" },
            { 206, @"カスタム実行ファイルを作成しました。\[(?<str>.*?)\]\((?<num>\d+)\)." }
        };
        private readonly Dictionary<int, string> cpMessagesEn = new Dictionary<int, string>()
        {
            //errormsg.cpp
            { 1, "Unknown code" },
            { 2, "Syntax error" },
            { 3, "Wrong expression (float)" },
            { 4, "Wrong expression (parameter)" },
            { 5, "Parameter not closed" },
            { 6, "Wrong array expression" },
            { 7, "Label name is already in use" },
            { 8, "Reserved name for label" },
            { 9, "Too many repeat level" },
            { 10, "break without repeat command" },
            { 11, "continue without repeat command" },
            { 12, "loop without repeat command" },
            { 13, "loop not found for repeat command" },
            { 14, "Not match if and else pair" },
            { 15, "Not match { and } pair" },
            { 16, "Not allow { and } in here" },
            { 17, "Illegal use of else command" },
            { 18, "Too many If command level" },
            { 19, "Fatal error occurs" },
            { 20, "Preprocessor syntax error" },
            { 21, "Error during register command" },
            { 22, "Only strings are acceptable" },
            { 23, "Wrong parameter name expression" },
            { 24, "No library name to bind" },
            { 25, "custom name already in use" },
            { 26, "parameter name already in use" },
            { 27, "Bad structure source expression" },
            { 28, "Bad structure expression" },
            { 29, "Bad import name" },
            { 30, "External func name in use" },
            { 31, "Incompatible type of import" },
            { 32, "Initalizer already exists" },
            { 33, "Terminator already exists" },
            { 34, "Wrong multi line string expression" },
            { 35, "Tag name already in use" },
            { 36, "No interface name to bind" },
            { 37, "No import index to bind" },
            { 38, "No import IID to bind" },
            { 39, "Uninitalized variable detected" },
            { 40, "Wrong name for variable" },
            //Mes
            { 86, "No file." },
            { 87, "Can't write output file." },
            //Mesf
            { 94, "Missing closing braces" },
            //info
            //Mes
            { 201, "use UTF-8 strings." },
            { 202, "output string map." }
        };
        private readonly Dictionary<int, string> cpMessagesJa = new Dictionary<int, string>()
        {
            { 1, "解釈できない文字コードです" },
            { 2, "文法が間違っています" },
            { 3, "小数の記述が間違っています" },
            { 4, "パラメーター式の記述が無効です" },
            { 5, "カッコが閉じられていません" },
            { 6, "配列の書式が間違っています" },
            { 7, "ラベル名はすでに使われています" },
            { 8, "ラベル名は指定できません" },
            { 9, "repeatのネストが深すぎます" },
            { 10, "repeatループ外でbreakが使用されました" },
            { 11, "repeatループ外でcontinueが使用されました" },
            { 12, "repeatループでないのにloopが使用されました" },
            { 13, "repeatループが閉じられていません" },
            { 14, "elseの前にifが見当たりません" },
            { 15, "{が閉じられていません" },
            { 16, "if命令以外で{～}が使われています" },
            { 17, "else命令の位置が不正です" },
            { 18, "if命令の階層が深すぎます" },
            { 19, "致命的なエラーです" },
            { 20, "プリプロセッサ命令が解釈できません" },
            { 21, "コマンド登録中のエラーです" },
            { 22, "プリプロセッサは文字列のみ受け付けます" },
            { 23, "パラメーター引数の指定が間違っています" },
            { 24, "ライブラリ名が指定されていません" },
            { 25, "命令として定義できない名前です" },
            { 26, "パラメーター引数名は使用されています" },
            { 27, "モジュール変数の参照元が無効です" },
            { 28, "モジュール変数の指定が無効です" },
            { 29, "外部関数のインポート名が無効です" },
            { 30, "拡張命令の名前はすでに使用されています" },
            { 31, "互換性のない拡張命令タイプを使用しています" },
            { 32, "コンストラクタは既に登録されています" },
            { 33, "デストラクタは既に登録されています" },
            { 34, "複数行文字列の終端ではありません" },
            { 35, "タグ名はすでに使用されています" },
            { 36, "インターフェース名が指定されていません" },
            { 37, "インポートするインデックスが指定されていません" },
            { 38, "インポートするIID名が指定されていません" },
            { 39, "未初期化変数を使用しようとしました" },
            { 40, "指定できない変数名です" },
            { 86, "ファイルがありません" },
            { 87, "出力ファイルを書き込めません" },
            { 94, "波括弧が閉じられていません" },
            { 201, "UTF-8 を用います" },
            { 202, "strmap を出力します" }
        };
        private readonly Dictionary<int, string> ppMessagesEn = new Dictionary<int, string>()
        {
            //SetError
            { 41, "abnormal calculation" },           //Const,Enum
            { 42, "expression syntax error" },        //Const,Enum
            { 43, "SJIS space code error" },
            { 44, "Reserved word syntax error" },     //macro展開
            { 45, "C-Type macro syntax error" },      //macro展開
            { 46, "too many macro parameters" },      //macro展開
            { 47, "macro buffer overflow" },          //macro展開
            { 48, "illegal macro parameter " },
            { 50, "macro parameter invalid" },        //macro展開
            { 51, "no default parameter" },           //macro展開
            { 52, "#if nested too deeply" },          //Switch
            { 53, "#endif without #if" },             //Switch
            { 54, "#else without #if" },              //Switch
            { 55, "#else after #else" },              //Switch
            { 56, "invalid addition suffix" },        //Include
            { 57, "invalid include suffix" },         //Include
            { 58, "too many include level" },         //Include
            { 59, "invalid symbol" },                 //Const,Enum,Define,undef
            { 60, "bad global syntax" },              //Const,Enum,struct,func,usecom
            { 61, "bad #const syntax" },              //Const
            { 62, "AHT parameter syntax error" },     //Const,Define
            { 63, "bad macro syntax" },               //Define
            { 64, "bad default value" },              //Define
            { 65, "macro contains no data" },         //Define
            { 66, "bad macro parameter expression" }, //Define
            { 67, "module name not found" },          //Defcfunc,deffunc,modfunc,modinit,modterm
            { 68, "invalid func name" },              //Defcfunc,deffunc,modfunc,func,cmd
            { 69, "invalid func param" },             //Defcfunc,deffunc,modfunc,modinit,modterm
            { 70, "invalid struct param" },           //struct
            { 71, "invalid COM symbol name" },        //usecom
            { 72, "invalid module name" },            //module
            { 73, "not in global mode" },             //module
            { 74, "bad module name" },                //module
            { 75, "invalid module param" },           //module
            { 76, "already in global mode" },         //global
            { 77, "invalid AHT option name" },        //aht
            { 78, "illegal ahtmes parameter" },       //ahtmes
            { 79, "invalid ahtmes format" },          //ahtmes
            { 80, "invalid pack name" },              //pack,epack
            { 81, "illegal option name" },            //packopt,cmpopt,bootopt
            { 82, "illegal option parameter" },       //packopt,cmpopt,bootopt
            { 83, "illegal runtime name" },           //runtime
            { 84, "Endless macro loop" },
            //Mes
            { 88, "Can't write output file." },
            { 89, "Can't write packfile." },
            { 90, "Fatal error reported." },
            //infomation
            //Mes
            { 200, "packfile generated." }
        };
        private readonly Dictionary<int, string> ppMessagesJa = new Dictionary<int, string>()
        {
            //SetError
            { 41, "正しい計算ではありません" },           //Const,Enum
            { 42, "式が正しくありません" },        //Const,Enum
            { 43, "スクリプト内に全角スペースが存在します" },
            { 44, "マクロに代入しようとしました" },     //macro展開
            { 45, "ctypeマクロの直後には、丸括弧でくくられた引数リストが必要です" },      //macro展開
            { 46, "マクロの引数が多すぎます" },      //macro展開
            { 47, "マクロを展開できませんでした" },          //macro展開
            { 48, "不正なマクロパラメータです " },
            { 50, "マクロパラメータが無効です " },        //macro展開
            { 51, "デフォルトパラメータのないマクロの引数は省略できません" },           //macro展開
            { 52, "#if のネストが深すぎます" },          //Switch
            { 53, "#endif に対応する #if が見つかりません" },             //Switch
            { 54, "#else に対応する #if が見つかりません" },              //Switch
            { 55, "#else のあとに #else があります" },              //Switch
            { 56, "#addition の引数が不正です" },        //Include
            { 57, "#include の引数が不正です" },         //Include
            { 58, "ファイル結合の深度が深すぎます" },         //Include
            { 59, "不正なキーワード名です" },                 //Const,Enum,Define,undef
            { 60, "global指定のあとが正しくありません" },              //Const,Enum,struct,func,usecom
            { 61, "#constマクロが不正です" },              //Const
            { 62, "AHT パラメータの文法が正しくありません" },     //Const,Define
            { 63, "マクロの文法が正しくありません" },               //Define
            { 64, "初期値が異常です" },              //Define
            { 65, "マクロにデータがありません" },         //Define
            { 66, "マクロパラーメータの式が正しくありません" }, //Define
            { 67, "モジュール名が見つかりません" },          //Defcfunc,deffunc,modfunc,modinit,modterm
            { 68, "無効な関数名です" },              //Defcfunc,deffunc,modfunc,func,cmd
            { 69, "無効な関数の引数です" },             //Defcfunc,deffunc,modfunc,modinit,modterm
            { 70, "無効な#structの引数です" },           //struct
            { 71, "無効なCOMキーワード名です" },        //usecom
            { 72, "無効なモジュール名です" },            //module
            { 73, "モジュールが閉じられていません" },             //module
            { 74, "良くないモージュール名です" },                //module
            { 75, "モジュールの引数が正しくありません" },           //module
            { 76, "#module と対応していない #global があります" },         //global
            { 77, "無効なAHTオプション名です" },        //aht
            { 78, "ahtmes のパラメータが不正です" },       //ahtmes
            { 79, "無効な ahtmes のフォーマットです" },          //ahtmes
            { 80, "無効な pack 名です" },              //pack,epack
            { 81, "オプション名が不正です" },            //packopt,cmpopt,bootopt
            { 82, "オプションパラメータが不正です" },       //packopt,cmpopt,bootopt
            { 83, "ランタイム名が正しくありません" },           //runtime
            { 84, "マクロが無限ループしています" },
            { 88, "プリプロセッサファイルの出力に失敗しました" },
            { 89, "packfileの出力に失敗しました" },
            { 90, "重大なエラーが検出されています" }
        };
        private readonly Dictionary<int, string> dpmMessagesEn = new Dictionary<int, string>()
        {
            //error
            { 5, "File write error." },
            { 6, "Not found hsp index." },
            //info
            { 205, "Found hsp index in $%05lx/$%05lx." }
        };
        private readonly Dictionary<int, string> dpmMessagesJa = new Dictionary<int, string>()
#pragma warning restore IDE0090 // 'new(...)' を使用する
        {
            //error
            { 5, "ファイル書き込みエラー。" },
            { 6, "HSPインデックスが見つかりません。" },
            //info
            { 205, "$%05lx/$%05lxにHSPインデックスが見つかりました。" }
        };
        private int errorMsg = -1;
        public List<string> Logos { get; private set; }
        public List<string> Errors { get; private set; }
        public List<string> Warns { get; private set; }
        public List<string> Infomation { get; private set; }
        public string ExeName { get; private set; } = string.Empty;

        private KeyValuePair<int, string> ConvertPPMsg(string message, string lang, KeyValuePair<int, string> s)
        {
            if (lang.Equals("ja") && s.Key != 99)
            {
                var matchs = Regex.Match(message.TrimStart('#'), s.Value);
                if (matchs.Index == 6)
                    errorMsg = s.Key;
                return new KeyValuePair<int, string>(s.Key, ppMessagesRegexJa[s.Key].Replace(@"(?<num>\d+)", matchs.Groups["num"].Value)
                    .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]").Replace("(?<str>.*?)", matchs.Groups["str"].Value));
            }
            else if (lang.Equals("en") && s.Key != 99)
            {
                var matchs = Regex.Match(message.TrimStart('#'), s.Value);
                if (matchs.Index == 6)
                    errorMsg = s.Key;
                return new KeyValuePair<int, string>(s.Key, ppMessagesRegexEn[s.Key].Replace(@"(?<num>\d+)", matchs.Groups["num"].Value)
                    .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]").Replace("(?<str>.*?)", matchs.Groups["str"].Value));
            }
            else
                return new KeyValuePair<int, string>(s.Key, message.TrimStart('#'));
        }

        private KeyValuePair<int, string> ConvertCPMsg(string message, string lang, KeyValuePair<int, string> s)
        {
            if (s.Key > 205 || s.Key == 97 || s.Key == 98)
                return new KeyValuePair<int, string>(s.Key, message.TrimStart('#'));
            if (lang.Equals("ja"))
            {
                var matchs = Regex.Match(message.TrimStart('#'), s.Value);
                if (matchs.Index == 6)
                    errorMsg = s.Key;
                return new KeyValuePair<int, string>(s.Key, cpMessagesRegexJa[s.Key].Replace(@"(?<num>\d+)", matchs.Groups["num"].Value)
                    .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]").Replace(@"\((?<str>.*?)\)", $"({matchs.Groups["str"].Value})").Replace("(?<str>.*?)", matchs.Groups["str"].Value));
            }
            else if (lang.Equals("en"))
            {
                var matchs = Regex.Match(message.TrimStart('#'), s.Value);
                if (matchs.Index == 6)
                    errorMsg = s.Key;
                return new KeyValuePair<int, string>(s.Key, cpMessagesRegexEn[s.Key].Replace(@"(?<num>\d+)", matchs.Groups["num"].Value)
                    .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]").Replace("(?<str>.*?)", matchs.Groups["str"].Value));
            }
            else
                return new KeyValuePair<int, string>(s.Key, message.TrimStart('#'));
        }

        private KeyValuePair<int, string> ConvertDpmMsg(string message, string lang, KeyValuePair<int, string> s)
        {
            var matchs = Regex.Match(message.TrimStart('#'), s.Value);
            if (s.Key == 206) ExeName = matchs.Groups["str"].Value;
            if (lang.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                if (s.Key == 204)
                    return new KeyValuePair<int, string>(s.Key, dpmMessagesRegexJa[s.Key].Replace(@"(?<num1>\d+)", matchs.Groups["num1"].Value)
                        .Replace(@"(?<num2>\d+)", matchs.Groups["num2"].Value).Replace(@"(?<num16>\d+)", matchs.Groups["num16"].Value)
                        .Replace(@"(?<str>.*?)", matchs.Groups["str"].Value));
                else
                    return new KeyValuePair<int, string>(s.Key, dpmMessagesRegexJa[s.Key].Replace(@"\((?<num>\d+)\)", $"({matchs.Groups["num"].Value})")
                        .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]"));
            }
            else if (lang.Equals("en", StringComparison.OrdinalIgnoreCase))
            {
                if (s.Key == 204)
                    return new KeyValuePair<int, string>(s.Key, dpmMessagesRegexEn[s.Key].Replace(@"(?<num1>\d+)", matchs.Groups["num1"].Value)
                        .Replace(@"(?<num2>\d+)", matchs.Groups["num2"].Value).Replace(@"(?<num16>\d+)", matchs.Groups["num16"].Value)
                        .Replace(@"(?<str>.*?)", matchs.Groups["str"].Value));
                else
                    return new KeyValuePair<int, string>(s.Key, dpmMessagesRegexEn[s.Key].Replace(@"\((?<num>\d+)\)", $"({matchs.Groups["num"].Value})")
                        .Replace(@"\[(?<str>.*?)\]", $"[{matchs.Groups["str"].Value}]"));
            }
            else
                return new KeyValuePair<int, string>(s.Key, message.TrimStart('#'));
        }

        public void ParsDPM(List<string> messages, string lang)
        {
            Errors = new List<string>();
            Infomation = new List<string>();
            Logos = new List<string>();
            if (lang.Length > 2)
                lang = lang.Remove(lang.IndexOf('-'));
            var error = new SortedDictionary<int, string>();
            var errorRegex = new Dictionary<int, string>();
            var info = new SortedDictionary<int, string>();
            var infoRegex = new Dictionary<int, string>();
            Logos.Add(messages[0]);
            foreach (var message in messages)
            {
                var dpmMessages = dpmMessagesEn.Where(s => s.Key <= 100 && message.TrimStart('#').StartsWith(s.Value))
                                               .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("ja") ? dpmMessagesJa[s.Key] : message.TrimStart('#')));
                if (dpmMessages.Count() == 0)
                    dpmMessages = dpmMessagesJa.Where(s => s.Key <= 100 && message.TrimStart('#').StartsWith(s.Value))
                                               .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("en") ? dpmMessagesEn[s.Key] : message.TrimStart('#')));
                error.AddRange(dpmMessages);
                var dpmMessagesRegex = dpmMessagesRegexEn.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value))
                                                         .Select(s => ConvertDpmMsg(message, lang, s));
                if (dpmMessagesRegex.Count() == 0)
                    dpmMessagesRegex = dpmMessagesRegexJa.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value))
                                                         .Select(s => ConvertDpmMsg(message, lang, s));
                errorRegex.AddRange(dpmMessagesRegex);
                dpmMessages = dpmMessagesEn.Where(s => s.Key > 200 && message.TrimStart('#').Equals(s.Value))
                                           .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("ja") ? dpmMessagesJa[s.Key] : message.TrimStart('#')));
                if (dpmMessages.Count() == 0)
                    dpmMessages = dpmMessagesJa.Where(s => s.Key > 200 && message.TrimStart('#').Equals(s.Value))
                                               .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("en") ? dpmMessagesEn[s.Key] : message.TrimStart('#')));
                info.AddRange(dpmMessages);
                dpmMessagesRegex = dpmMessagesRegexEn.Where(s => s.Key >= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                if (dpmMessagesRegex.Count() == 0)
                    dpmMessagesRegex = dpmMessagesRegexJa.Where(s => s.Key >= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                infoRegex.AddRange(dpmMessagesRegex.Select(s => ConvertDpmMsg(message, lang, s)));
            }
            error.AddRange(errorRegex);
            Errors.AddRange(error.Select(s => $"HSP{s.Key:d3}: {s.Value}"));
            info.AddRange(infoRegex);
            Infomation.AddRange(info.Select(s => s.Value));
        }

        public void Pars(List<string> messages, string lang)
        {
            Errors = new List<string>();
            Warns = new List<string>();
            Infomation = new List<string>();
            Logos = new List<string>();
            string file = "";
            int line = -1;
            if (lang.Length > 2)
                lang = lang.Remove(lang.IndexOf('-'));
            int next = messages.FindIndex((s) => s.StartsWith("#HSP code generator"));
            if (next == -1)
                next = messages.Count;
            var lookmessages = messages.Select((value, index) => new { value, index }).ToLookup(t => t.index < next ? "pp" : "cmp", t => t.value);
            var error = new Dictionary<int, string>();
            var errorRegex = new Dictionary<int, string>();
            var info = new Dictionary<int, string>();
            var infoRegex = new Dictionary<int, List<string>>();
            // Preprocess Message
            var sepalatedMessages = new List<string[]>();
            var tmp = lookmessages["pp"].ToList();
            while (true)
            {
                int fatalErrorReported = tmp.FindIndex(message => message.StartsWith(ppMessagesEn[90]) || message.StartsWith(ppMessagesJa[90]));
                if (fatalErrorReported == -1)
                    break;
                sepalatedMessages.Add(tmp.Take(fatalErrorReported + 1).ToArray());
                tmp.RemoveRange(0, fatalErrorReported + 1);
                if (tmp.Count == 0)
                    break;
            }
            if (sepalatedMessages.Count == 0)
                sepalatedMessages.Add(lookmessages["pp"].ToArray());
            Logos.Add(lookmessages["pp"].First().TrimStart('#'));
            foreach (var sepalatedMessage in sepalatedMessages)
            {
                foreach (var message in sepalatedMessage)
                {
                    // get error preprocess message
                    var ppMessages = ppMessagesEn.Where(s => s.Key <= 100 && message.TrimStart('#').Contains(s.Value))
                                                 .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("ja") ? ppMessagesJa[s.Key] : message.TrimStart('#')));
                    if (ppMessages.Count() == 0)
                    {
                        ppMessages = ppMessagesJa.Where(s => s.Key <= 100 && message.TrimStart('#').Contains(s.Value))
                                                 .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("en") ? ppMessagesEn[s.Key] : message.TrimStart('#')));
                    }
                    error.AddRange(ppMessages);
                    var ppMessagesRegex = ppMessagesRegexEn.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    if (ppMessagesRegex.Count() == 0)
                    {
                        ppMessagesRegex = ppMessagesRegexJa.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    }
                    errorRegex.AddRange(ppMessagesRegex.Select(s => ConvertPPMsg(message, lang, s)));
                    // get info preprocess message
                    ppMessagesRegex = ppMessagesRegexEn.Where(s => s.Key >= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    if (ppMessagesRegex.Count() == 0)
                    {
                        ppMessagesRegex = ppMessagesRegexJa.Where(s => s.Key >= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    }
                    infoRegex.AddRange(ppMessagesRegex.Select(s => ConvertPPMsg(message, lang, s)));
                }
                // #Error:%s in line %d [%s] の処理
                if (errorRegex.ContainsKey(99))
                {
                    var errorLocation = Regex.Match(errorRegex[99], ppMessagesRegexEn[99]);
                    file = errorLocation.Groups["str2"].Value;
                    line = int.Parse(errorLocation.Groups["num"].Value);
                    if (errorMsg >= 0)
                    {
                        Errors.Add($"HSP{errorMsg:d3}: {errorRegex[errorMsg]} (line {line} in {file})");
                        errorRegex.Remove(errorMsg);
                        errorRegex.Remove(99);
                        errorMsg = -1;
                    }
                    else if (error.Count == 2 && error.ContainsKey(90))
                    {
                        Errors.Add($"HSP{error.First().Key:d3}: {error.First().Value} (line {line} in {file})");
                        error.Clear();
                        errorRegex.Remove(99);
                    }
                    // ここはいらないはず
                    else if (error.Count > 2 && error.ContainsKey(90))
                    {
                        foreach (var msg in ppMessagesEn)
                        {
                            if (msg.Value == errorLocation.Groups["str1"].Value)
                            {
                                if (lang.Equals("ja"))
                                    Errors.Add($"HSP{msg.Key:d3}: {ppMessagesJa[msg.Key]} (line {line} in {file})");
                                else
                                    Errors.Add($"HSP{msg.Key:d3}: {msg.Value} (line {line} in {file})");
                                error.Remove(msg.Key);
                                errorRegex.Remove(99);
                            }
                        }
                        foreach (var msg in ppMessagesJa)
                        {
                            if (msg.Value == errorLocation.Groups["str1"].Value)
                            {
                                if (lang.Equals("en"))
                                    Errors.Add($"HSP{msg.Key:d3}: {ppMessagesEn[msg.Key]} (line {line} in {file})");
                                else
                                    Errors.Add($"HSP{msg.Key:d3}: {msg.Value} (line {line} in {file})");
                                error.Remove(msg.Key);
                                errorRegex.Remove(99);
                            }
                        }
                    }
                }
                Errors.AddRange(errorRegex.Select(s => $"HSP{s.Key:d3}: {s.Value}"));
                Errors.AddRange(error.Where(s => s.Key != 90).Select(s => $"HSP{s.Key:d3}: {s.Value}"));
                Infomation.AddRange(infoRegex.Aggregate((new List<string>()).Select(s => s), (a,b)=>a.Concat(b.Value).ToList()));
                error.Clear();
                errorRegex.Clear();
                infoRegex.Clear();
            }
            // Compile Message
            if (lookmessages["cmp"].Any())
            {
                Logos.Add(lookmessages["cmp"].First().TrimStart('#'));
                foreach (var message in lookmessages["cmp"])
                {
                    // get compile error message
                    var cpMessages = cpMessagesEn.Where(s => s.Key <= 100 && message.TrimStart('#').Equals(s.Value))
                                                 .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("ja") ? cpMessagesJa[s.Key] : message.TrimStart('#')));
                    if (cpMessages.Count() == 0)
                        cpMessages = cpMessagesJa.Where(s => s.Key <= 100 && message.TrimStart('#').Equals(s.Value))
                                                 .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("en") ? cpMessagesEn[s.Key] : message.TrimStart('#')));
                    error.AddRange(cpMessages);
                    var cpMessagesRegex = cpMessagesRegexEn.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value))
                                                           .Select(s => ConvertCPMsg(message, lang, s));
                    if (cpMessagesRegex.Count() == 0)
                        cpMessagesRegex = cpMessagesRegexJa.Where(s => s.Key <= 100 && Regex.IsMatch(message.TrimStart('#'), s.Value))
                                                           .Select(s => ConvertCPMsg(message, lang, s));
                    errorRegex.AddRange(cpMessagesRegex);
                    // get compile warning message
                    var warn = cpMessagesRegexEn.Where(s => s.Key > 100 && s.Key <= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value)).Select(s => ConvertCPMsg(message, lang, s)).Select(s => $"HSP{s.Key:d3}: {s.Value}");
                    if (warn.Count() == 0)
                    {
                        warn = cpMessagesRegexJa.Where(s => s.Key > 100 && s.Key <= 200 && Regex.IsMatch(message.TrimStart('#'), s.Value)).Select(s => ConvertCPMsg(message, lang, s)).Select(s => $"HSP{s.Key:d3}: {s.Value}");
                    }
                    Warns.AddRange(warn);
                    // get compile message
                    cpMessages = cpMessagesEn.Where(s => s.Key > 200 && message.TrimStart('#').Equals(s.Value))
                                             .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("ja") ? cpMessagesJa[s.Key] : message.TrimStart('#')));
                    if(cpMessages.Count() == 0)
                        cpMessages = cpMessagesJa.Where(s => s.Key > 200 && message.TrimStart('#').Equals(s.Value))
                                                 .Select(s => new KeyValuePair<int, string>(s.Key, lang.Equals("en") ? cpMessagesEn[s.Key] : message.TrimStart('#')));
                    info.AddRange(cpMessages);
                    cpMessagesRegex = cpMessagesRegexEn.Where(s => s.Key > 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    if (cpMessagesRegex.Count() == 0)
                        cpMessagesRegex = cpMessagesRegexJa.Where(s => s.Key > 200 && Regex.IsMatch(message.TrimStart('#'), s.Value));
                    infoRegex.AddRange(cpMessagesRegex.Select(s => ConvertCPMsg(message, lang, s)));
                }
                file = "";
                line = -1;
                if (errorRegex.ContainsKey(97))
                {
                    var errorLocation = Regex.Match(errorRegex[97], cpMessagesRegexEn[97]);
                    file = errorLocation.Groups["str1"].Value;
                    line = int.Parse(errorLocation.Groups["num3"].Value);
                    int errNum = int.Parse(errorLocation.Groups["num2"].Value);
                    if (errorMsg >= 0)
                    {
                        Errors.Add($"HSP{errorMsg:d3}: {errorRegex[errorMsg]} (line {line} in {file})");
                        errorRegex.Remove(errorMsg);
                        errorMsg = -1;
                    }
                    else if (lang.Equals("ja"))
                        Errors.Add($"HSP{errNum:d3}: {cpMessagesJa[errNum]} (line {line} in {file})");
                    else
                        Errors.Add($"HSP{errNum:d3}: {errorLocation.Groups["str2"].Value} (line {line} in {file})");
                    if (errorRegex.ContainsKey(98))
                    {
#pragma warning disable IDE0056 // インデックス演算子を使用
                        Errors[Errors.Count - 1] += Environment.NewLine + "        " + errorRegex[98];
#pragma warning restore IDE0056 // インデックス演算子を使用
                        errorRegex.Remove(98);
                    }
                    errorRegex.Remove(97);
                }
                Errors.AddRange(error.Select(s => $"HSP{s.Key:d3}: {s.Value}"));
                Errors.AddRange(errorRegex.Select(s => $"HSP{s.Key:d3}: {s.Value}"));
                Infomation.AddRange(info.Select(s => s.Value));
                Infomation.AddRange(infoRegex.Where(s => s.Key < 206).Aggregate((new List<string>()).Select(s=>s), (a, b) => a.Concat(b.Value)));
            }
        }
    }
}
