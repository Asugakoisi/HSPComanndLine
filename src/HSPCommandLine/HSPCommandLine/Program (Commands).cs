using HspcuiBase.Compile;
using HspcuiBase.Options;
using static HSPCommandLine.Utils.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HSPCommandLine
{
    partial class Program
    {
        /// <summary>
        /// profile コマンドの処理
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功したかどうか</returns>
        private static void Profile(string[] args)
        {
            string profileName = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "add" when i == 0:
                        isAddProfile = true;
                        if (!args[i + 1].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            addProfileName = args[++i];
                        break;
                    case "delete" when i == 0:
                        isDeleteProfile = true;
                        deleteProfileName = args[++i];
                        break;
                    case "view" when i == 0:
                        isOutputOptions = true;
                        profileName = args[++i];
                        break;
                    case "--help":
                        isOutputHelpTexts = true;
                        break;
                    default:
                        break;
                }
            }
            // ロゴ表示
            Console.WriteLine($"hspcui ver{typeof(Program).Assembly.GetName().Version} / {typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}");
            // オプションチェック時のエラー出力
            if (!string.IsNullOrEmpty(messages))
                Console.Write(messages);
            ConfigRead();
            SettingSystemPath();
            // --help オプション
            if (isOutputHelpTexts)
            {
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                string helpKeyword = "profile";
                if (isAddProfile)
                    helpKeyword += " add";
                else if (isDeleteProfile)
                    helpKeyword += " delete";
                else if (isOutputOptions)
                    helpKeyword += " view";
                Console.Write(help.GetHelpTests(new string[1] { helpKeyword }));
            }
            // --profileaオプション
            else if (isAddProfile)
            {
                ProfileaOption(args);
            }
            // --profiled オプション
            else if (isDeleteProfile)
            {
                ProfiledOption();
            }
            // view コマンド
            else if (!string.IsNullOrEmpty(profileName))
            {
                if (Config is null)
                    Config = new Config(isCatchErrors);
                var options = Config.GetProfileCommand(profileName);
                Console.WriteLine($"{profileName} profile: {string.Join(' ', options)}");
            }
        }

        /// <summary>
        /// template コマンドの処理
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功したかどうか</returns>
        private static void Template(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "copy" when (i == 0):
                        isCopyTemplateDirectory = true;
                        break;
                    case "add" when (i == 0):
                        isAddTemplateDirectory = true;
                        break;
                    case "delete" when (i == 0):
                        isDelTemplateDirectory = true;
                        break;
                    case "--tempo":
                    case "-o":
                        outputTemplateDirectoryName = args[++i].TrimEnd(Path.DirectorySeparatorChar);
                        break;
                    case "--tmppath":
                    case "-t":
                        templateDirectoryPath = args[++i];
                        break;
                    case "--syspath":
                    case "-s":
                        sysDirectoryPath = args[++i];
                        break;
                    case "--help":
                    case "-h":
                        isOutputHelpTexts = true;
                        break;
                    case "-E":
                        isCatchErrors = false;
                        messages += Properties.Resources.ErrorIgnore + Environment.NewLine;
                        break;
                    case "--nologo":
                        isViewLogo = false;
                        break;
                    default:
                        if (args[i].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                        {
                            messages += string.Format(Properties.Resources.InvalidOption, args[i]) + Environment.NewLine;
                        }
                        else
                        {
                            if (isCopyTemplateDirectory)
                                templateDirectoryName = args[i];
                            else if (isAddTemplateDirectory)
                                addTemplateDirectoryPath = args[i];
                            else if (isDelTemplateDirectory)
                                delTemplateDirectoryName = args[i];
                        }
                        break;
                }
            }
            // ロゴ表示
            if (isViewLogo) Console.WriteLine($"hspcui ver{typeof(Program).Assembly.GetName().Version} / {typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}");
            // オプションチェック時のエラー出力
            if (!string.IsNullOrEmpty(messages))
                Console.Write(messages);
            ConfigRead();
            SettingSystemPath();
            // --help オプション
            if (isOutputHelpTexts)
            {
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                string helpKeyword = "template";
                if (isCopyTemplateDirectory)
                    helpKeyword += " copy";
                else if (isDelTemplateDirectory)
                    helpKeyword += " delete";
                else if (isAddTemplateDirectory)
                    helpKeyword += " add";
                Console.Write(help.GetHelpTests(new string[1] { helpKeyword }));
                return;
            }
            SettingTemplatesPath();
            // -temp オプション
            if (isCopyTemplateDirectory)
            {
                if (string.IsNullOrEmpty(templateDirectoryName))
                    templateDirectoryName = "default";
                TryTempOption();
            }
            // --tempd オプション
            if (isDelTemplateDirectory)
            {
                if (string.IsNullOrEmpty(delTemplateDirectoryName))
                    delTemplateDirectoryName = "default";
                TempdOption();
            }
            // --tempa オプション
            if (isAddTemplateDirectory)
            {
                TryTempaOption();
            }
        }

        /// <summary>
        /// config コマンドの処理
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功したかどうか</returns>
        private static void Conf(List<string> args)
        {
            if (args.Remove("-E"))
                isCatchErrors = false;
            if (args.Remove("--view"))
                isOutputOptions = true;
            Config = new Config(isCatchErrors);
            if (args.Contains("--help") || args.Contains("-h"))
            {
                ConfigRead();
                SettingSystemPath();
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                Console.Write(help.GetHelpTests(new string[1] { "config" }));
            }
            else if (args[0].Equals("system", StringComparison.OrdinalIgnoreCase))
            {
                if (isOutputOptions)
                    Console.WriteLine(Config.SystemDirectory);
                else
                    Config.SystemPathSave(args[1]);
            }
            else if (args[0].Equals("common", StringComparison.OrdinalIgnoreCase))
            {
                if (isOutputOptions)
                    Console.WriteLine(Config.CommonDirectory);
                else
                    Config.CommonPathSave(args[1]);
            }
            else if (args[0].Equals("runtime", StringComparison.OrdinalIgnoreCase))
            {
                if (isOutputOptions)
                    Console.WriteLine(Config.RuntimeDirectory);
                else
                    Config.RuntimePathSave(args[1]);
            }
            else if (args[0].Equals("templates", StringComparison.OrdinalIgnoreCase))
            {
                if (isOutputOptions)
                    Console.WriteLine(Config.TemplatesDirectory);
                else
                    Config.TemplatesPathSave(args[1]);
            }
            else
                Console.WriteLine(Properties.Resources.InvalidOption, args[0]);
        }

        /// <summary>
        /// run コマンドの処理
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功したかどうか</returns>
        private static void Run(string[] args)
        {
            bool isOption = false;
            isExecute = true;
            int i;
            for (i = 0; i < args.Length; i++)
            {
                if (isOption)
                    break;
                switch (args[i])
                {
                    case "-r0":
                        isExecute = false;
                        break;
                    case "-E":
                        isCatchErrors = false;
                        messages += Properties.Resources.ErrorIgnore + Environment.NewLine;
                        break;
                    case "--nologo":
                        isViewLogo = false;
                        break;
                    case "--":
                        isOption = true;
                        break;
                    case "--help":
                    case "-h":
                        isOutputHelpTexts = true;
                        break;
                    default:
                        if (args[i].StartsWith("--syspath", StringComparison.OrdinalIgnoreCase))
                        {
                            sysDirectoryPath = args[++i];
                        }
                        else if (args[i].StartsWith("--rtmpath", StringComparison.OrdinalIgnoreCase))
                        {
                            runtimeDirectoryPath = args[++i];
                        }
                        else if (args[i].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                        {
                            messages += string.Format(Properties.Resources.InvalidOption, args[i]) + Environment.NewLine;
                        }
                        else
                        {
                            sourceFilePath = args[i];
                        }
                        break;
                }
            }
            // ロゴ表示
            if (isViewLogo) Console.WriteLine($"hspcui ver{typeof(Program).Assembly.GetName().Version} / {typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}");
            // オプションチェック時のエラー出力
            if (!string.IsNullOrEmpty(messages))
                Console.Write(messages);
            // configの読み込み
            ConfigRead();
            SettingSystemPath();
            if (isOutputHelpTexts)
            {
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                Console.Write(help.GetHelpTests(new string[1] { "run" }));
                return;
            }
            SettingRuntimePath();
            if (sourceFilePath == string.Empty && !isNoCompile)
            {
                OutputError(Properties.Resources.DidNotSpecifySrcFile, isCatchErrors);
            }
            if (!File.Exists(sourceFilePath) && !isNoCompile)
            {
                OutputError(string.Format(Properties.Resources.NotFoundSpecifySrcFile, sourceFilePath), isCatchErrors);
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var compile = new Compile(sourceFilePath, commonDirectoryPath, sysDirectoryPath);
            if (isExecute)
            {
                options = string.Join(" ", args[i..]);
                compile.ExcuteAx(sourceFilePath, options);
                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage));
            }
            else
            {
                string runtimeName = Compile.GetRuntime(!string.IsNullOrEmpty(outputFileName) ? outputFileName : sourceFilePath, sysDirectoryPath);
                Console.WriteLine($"Runtime[{runtimeName}]");
            }
        }

        /// <summary>
        /// build コマンドの処理
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        /// <returns>成功したかどうか</returns>
        private static bool Build(string[] args)
        {
            isNoCompile = false;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
#if !LINUX
                    case "-a":
                        isCharsetCheck = true;
                        break;
#else
                    case "-a":
                        messages += string.Format(Properties.Resources.ErrorOS, "-a") + Environment.NewLine;
                        break; 
#endif
                    case "-c":
                        ppout |= PreprocessOptions.Ver26;
                        break;
                    case "-C":
                        isFourceCurrentDirectory = true;
                        break;
                    case "-d":
                        mode |= ModeOptions.Debug;
                        break;
                    case "-D":
                        isDeleteFiles = true;
                        break;
                    case "-E":
                        isCatchErrors = false;
                        messages += Properties.Resources.ErrorIgnore + Environment.NewLine;
                        break;
                    case "-i":
                        if (!isCharsetCheck) ppout |= PreprocessOptions.InputUTF8;
                        break;
#if !LINUX
                    case "-j":
                        isNoCompile = false;
                        break;
#else
                    case "-j":
                        messages += string.Format(Properties.Resources.ErrorOS, "-j") + Environment.NewLine;
                        break;
#endif
                    case "-p":
                        mode |= ModeOptions.OnlyPreprocess;
                        break;
#if !LINUX
                    case "-u":
                        mode |= ModeOptions.OutputUTF8;
                        isConvertSourceFile = true;
                        break;
                    case "-n":
                    case "--notasminfo":
                        isCreateAsmInfo = false;
                        break;
#else
                    case "-u":
                        isNoCompile = false;
                        break;
                    case "-n":
                    case "--notasminfo":
                        messages += string.Format(Properties.Resources.ErrorOS, "--notasminfo") + Environment.NewLine;
                        break;
#endif
                    case "--nologo":
                        isViewLogo = false;
                        break;
                    case "--newcmpmes":
                        isViewNewCmpMessage = true;
                        break;
                    case "--help":
                    case "-h":
                        isOutputHelpTexts = true;
                        break;
                    default:
                        try
                        {
                            if (args[i].StartsWith("-o", StringComparison.OrdinalIgnoreCase) || args[i].StartsWith("--outname", StringComparison.OrdinalIgnoreCase))
                            {
                                outputFileName = args[++i];
                            }
                            else if (args[i].StartsWith("-t", StringComparison.OrdinalIgnoreCase) || args[i].StartsWith("--type", StringComparison.OrdinalIgnoreCase))
                            {
                                string type = args[++i];
                                if (type.Equals("ax", StringComparison.OrdinalIgnoreCase))
                                    isNoCompile = false;
                                else if(type.Equals("exe", StringComparison.OrdinalIgnoreCase))
                                {
#if !LINUX
                                    // P オプション
                                    ppout |= PreprocessOptions.MakePackfile;
                                    // m オプション
                                    isMakeExe = true;
#else
                                    messages += string.Format(Properties.Resources.ErrorOS, "--type exe") + Environment.NewLine;
#endif
                                }
                                else if (type.Equals("strmap", StringComparison.OrdinalIgnoreCase))
                                {
                                    // s オプション
                                    mode |= ModeOptions.OutputStrmap;
                                }
                            }
                            else if (args[i].StartsWith("--syspath", StringComparison.OrdinalIgnoreCase))
                            {
                                sysDirectoryPath = args[++i];
                            }
                            else if (args[i].StartsWith("--compath", StringComparison.OrdinalIgnoreCase))
                            {
                                commonDirectoryPath = args[++i];
                            }
                            else if (args[i].StartsWith("--rtmpath", StringComparison.OrdinalIgnoreCase))
                            {
                                runtimeDirectoryPath = args[++i];
                            }
                            else if (args[i].StartsWith("--platform", StringComparison.OrdinalIgnoreCase))
                            {
#if !LINUX
                                string arg = args[++i];
                                string p = !string.IsNullOrEmpty(arg) ? arg : "x86";
                                if (p.Equals("x64", StringComparison.OrdinalIgnoreCase))
                                    platform = Platform.x64;
                                else
                                    platform = Platform.x86;
#else
                                messages += string.Format(Properties.Resources.ErrorOS, "--platform") + Environment.NewLine;
#endif
                            }
                            else if (args[i].StartsWith("--refname", StringComparison.OrdinalIgnoreCase))
                            {
#if !LINUX
                                refName = args[++i];
#else
                                Console.WriteLine(Properties.Resources.ErrorOS, "--refname");
#endif
                            }
                            else if (args[i].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            {
                                messages += string.Format(Properties.Resources.InvalidOption, args[i]) + Environment.NewLine;
                            }
                            else
                            {
                                sourceFilePath = args[i];
                            }
                        }
                        catch (Exception ex)
                        {
                            if (isCatchErrors)
                                throw;
                            else
                                messages += string.Format(ex.Message) + Environment.NewLine;
                        }
                        break;
                }
            }
#if LINUX
            // -u オプションを強制する
            if(!isNoCompile){
                mode |= ModeOptions.OutputUTF8;
                isConvertSourceFile = true;
            }
#endif
            // ロゴ表示
            if (isViewLogo) Console.WriteLine($"hspcui ver{typeof(Program).Assembly.GetName().Version} / {typeof(Program).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}");
            // オプションチェック時のエラー出力
            if (!string.IsNullOrEmpty(messages))
                Console.Write(messages);
            // configの読み込み
            ConfigRead();
            SettingSystemPath();
            if (isOutputHelpTexts)
            {
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                Console.Write(help.GetHelpTests(new string[1] { "build" }));
                return true;
            }
            SettingCommonPath();
            SettingRuntimePath();
            if (sourceFilePath == string.Empty)
            {
                OutputError(Properties.Resources.DidNotSpecifySrcFile, isCatchErrors);
            }
            if (!File.Exists(sourceFilePath))
            {
                OutputError(string.Format(Properties.Resources.NotFoundSpecifySrcFile, sourceFilePath), isCatchErrors);
            }
            // -C オプション
            if (isFourceCurrentDirectory)
            {
                SetCurrentDirectory();
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // 文字コードの判別
            if (isCharsetCheck)
            {
                CharsetCheck();
            }
            // ソースコードのエンコード
            if (isConvertSourceFile)
            {
                EncodeSourceFile();
            }
            Console.WriteLine();
            // コンパイル
            var compile = new Compile(sourceFilePath, commonDirectoryPath, sysDirectoryPath);
            bool res = false;
            // --platform -i -a(utf8) オプション
            if (isCreateAsmInfo)
            {
                res = CreateAsmInfo(compile);
            }
            // -m オプション
            if (isMakeExe)
            {
#if !LINUX
                // start.ax 生成 (-P オプションでコードから packfile 生成)
                outputFileName = Path.Combine(Path.GetDirectoryName(sourceFilePath), "start.ax");
                CreateAx(compile, out res, out CompileTextParser parser);
                if (res)
                {
                    // packfile に基づいて自動実行ファイル作成
                    res = CreateExe(compile, parser);
                }
#endif
            }
            // -m オプションなし
            else
            {
                CreateAx(compile, out res, out CompileTextParser parser);
            }
            // -D オプション
            if (isDeleteFiles)
            {
                DeleteFiles();
            }
            return res;
        }
    }
}
