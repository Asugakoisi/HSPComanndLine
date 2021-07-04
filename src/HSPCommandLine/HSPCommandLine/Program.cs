using HspcuiBase.Options;
using HspcuiBase.Compile;
using static HSPCommandLine.Utils.Util;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace HSPCommandLine
{
    partial class Program
    {
        private static string options = string.Empty;
        private static string refName = string.Empty;
        private static string messages = string.Empty;
        private static string searchWord = string.Empty;
        private static string sourceFilePath = string.Empty;
        private static string outputFileName = string.Empty;
        private static string addProfileName = string.Empty;
        private static string sysDirectoryPath = string.Empty;
        private static string deleteProfileName = string.Empty;
        private static string commonDirectoryPath = string.Empty;
        private static string runtimeDirectoryPath = string.Empty;
        private static string templateDirectoryPath = string.Empty;
        private static string templateDirectoryName = string.Empty;
        private static string addTemplateDirectoryPath = string.Empty;
        private static string delTemplateDirectoryName = string.Empty;
        private static string outputTemplateDirectoryName = string.Empty;
        private static bool isExecute = false;
        private static bool isMakeExe = false;
        private static bool isViewLogo = true;
        private static bool isNoCompile = true;
        private static bool isAddProfile = false;
        private static bool isCatchErrors = true;
        private static bool isDebugWindow = false;
        private static bool isDeleteFiles = false;
        private static bool isCharsetCheck = false;
        private static bool isCreateAsmInfo = true;
        private static bool isDeleteProfile = false;
        private static bool isOutputOptions = false;
        private static bool isOutputHelpTexts = false;
        private static bool isConvertSourceFile = false;
        private static bool isOutputRuntimeName = false;
        private static bool isViewNewCmpMessage = false;
        private static bool isAddTemplateDirectory = false;
        private static bool isDelTemplateDirectory = false;
        private static bool isCopyTemplateDirectory = false;
        private static bool isFourceCurrentDirectory = false;
        private static Platform platform = Platform.x86;
        private static ModeOptions mode = ModeOptions.None;
        private static PreprocessOptions ppout = PreprocessOptions.None;
        private static Config Config;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[2] { "--help", "--ls" };
            }
            else
            {
                var tmp = args.ToList();
                if (tmp.Contains("--version"))
                {
                    Console.WriteLine("Version: " + typeof(Program).Assembly.GetName().Version);
                    return;
                }
                if (tmp.Contains("--license"))
                {
                    Console.WriteLine(Properties.Resources.LICENSE);
                    return;
                }
                try
                {
                    var cul = tmp.Where((arg) => arg.StartsWith("--lang=", StringComparison.OrdinalIgnoreCase)).Select((arg) => arg[7..]).ToList();
                    if (cul.Count != 0)
                    {
                        var culture = new CultureInfo(cul[^1]);
                        CultureInfo.CurrentCulture = culture;
                        CultureInfo.CurrentUICulture = culture;
                        CultureInfo.DefaultThreadCurrentCulture = culture;
                        CultureInfo.DefaultThreadCurrentUICulture = culture;
                        Debug.WriteLine($"set to {culture.Name}");
                        foreach (var item in cul)
                        {
                            tmp.Remove("--lang=" + item);
                        }
                    }
                }
                catch { }
                var pro = tmp.Where((arg) => arg.StartsWith("--profile=", StringComparison.OrdinalIgnoreCase)).Select((arg) => arg[10..]).ToList();
                if (pro.Count != 0)
                {
                    Config = new Config(true);
                    foreach (var item in pro)
                    {
                        var newArgs = Config.GetProfileCommand(item);
                        if (newArgs is not null)
                        {
                            newArgs.AddRange(tmp.Where((arg) => !arg.StartsWith("--lang=")));
                            tmp = newArgs;
                        }
                        tmp.Remove("--profile=" + item);
                    }
                }
                args = tmp.ToArray();
            }
            Properties.Resources.Culture = CultureInfo.CurrentUICulture;

            int count = 0;
            foreach (var arg in args)
            {
                if (isOutputHelpTexts)
                    break;
                switch (arg)
                {
                    case "build":
                        Build(args[1..]);
                        return;
                    case "run":
                        Run(args[1..]);
                        return;
                    case "config":
                        Conf(args[1..].ToList());
                        return;
                    case "template":
                        Template(args[1..]);
                        return;
                    case "profile":
                        Profile(args[1..]);
                        return;
#if !LINUX
                    case "-a":
                        isCharsetCheck = true;
                        isNoCompile = false;
                        break;
#else
                   case "-a":
                        messages += string.Format(Properties.Resources.ErrorOS, "-a") + Environment.NewLine;
                        break; 
#endif
                    case "-c":
                        ppout |= PreprocessOptions.Ver26;
                        isNoCompile = false;
                        break;
                    case "-C":
                        isFourceCurrentDirectory = true;
                        break;
                    case "-d":
                        mode |= ModeOptions.Debug;
                        isNoCompile = false;
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
                        isNoCompile = false;
                        break;
#if !LINUX
                    case "-j":
                        isNoCompile = false;
                        break;
                    case "-m":
                        isMakeExe = true;
                        isNoCompile = false;
                        break;
#else
                    case "-j":
                        messages += string.Format(Properties.Resources.ErrorOS, "-j") + Environment.NewLine;
                        break;
                    case "-m":
                        messages += string.Format(Properties.Resources.ErrorOS, "-m") + Environment.NewLine;
                        break;
#endif
                    case "-p":
                        mode |= ModeOptions.OnlyPreprocess;
                        isNoCompile = false;
                        break;
#if !LINUX
                    case "-P":
                        ppout |= PreprocessOptions.MakePackfile;
                        isNoCompile = false;
                        break;
#else
                    case "-P":
                        messages += string.Format(Properties.Resources.ErrorOS, "-r") + Environment.NewLine;
                        break;
#endif
                    case "-r0":
                        isOutputRuntimeName = true;
                        break;
                    case "-r":
                        isExecute = true;
                        break;
                    case "-s":
                        mode |= ModeOptions.OutputStrmap;
                        isNoCompile = false;
                        break;
#if !LINUX
                    case "-u":
                        mode |= ModeOptions.OutputUTF8;
                        isNoCompile = false;
                        isConvertSourceFile = true;
                        break;
                    case "-w":
                        isDebugWindow = true;
                        isNoCompile = false;
                        break;
                    case "--notasminfo":
                        isCreateAsmInfo = false;
                        break;
#else
                    case "-u":
                        isNoCompile = false;
                        break;
                    case "-w":
                        messages += string.Format(Properties.Resources.ErrorOS, "-w") + Environment.NewLine;
                        break;
#endif
                    case "--see":
                        isOutputOptions = true;
                        break;
                    case "--help":
                        isOutputHelpTexts = true;
                        break;
                    case "--temp":
                        isCopyTemplateDirectory = true;
                        templateDirectoryName = "default";
                        break;
                    case "--tempd":
                        isDelTemplateDirectory = true;
                        delTemplateDirectoryName = "default";
                        break;
                    case "--nologo":
                        isViewLogo = false;
                        break;
                    case "--newcmpmes":
                        isViewNewCmpMessage = true;
                        break;
                    default:
                        try
                        {
                            if (arg.StartsWith("-o", StringComparison.OrdinalIgnoreCase))
                            {
                                outputFileName = !string.IsNullOrEmpty(arg[2..]) ? arg[2..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyOutputFile);
                                isNoCompile = false;
                            }
                            else if (arg.StartsWith("--outname=", StringComparison.OrdinalIgnoreCase))
                            {
                                outputFileName = !string.IsNullOrEmpty(arg[10..]) ? arg[10..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyOutputFile);
                                isNoCompile = false;
                            }
                            else if (arg.StartsWith("--syspath=", StringComparison.OrdinalIgnoreCase))
                            {
                                sysDirectoryPath = !string.IsNullOrEmpty(arg[10..]) ? arg[10..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifySysDirectory);
                            }
                            else if (arg.StartsWith("--compath=", StringComparison.OrdinalIgnoreCase))
                            {
                                commonDirectoryPath = !string.IsNullOrEmpty(arg[10..]) ? arg[10..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyComDirectory);
                            }
                            else if (arg.StartsWith("--rtmpath=", StringComparison.OrdinalIgnoreCase))
                            {
                                runtimeDirectoryPath = !string.IsNullOrEmpty(arg[10..]) ? arg[10..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyRtmDirectory);
                            }
                            else if (arg.StartsWith("--tmppath=", StringComparison.OrdinalIgnoreCase))
                            {
                                templateDirectoryPath = !string.IsNullOrEmpty(arg[10..]) ? arg[10..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyTmpDirectory);
                            }
                            else if (arg.StartsWith("-h", StringComparison.OrdinalIgnoreCase))
                            {
                                searchWord = !string.IsNullOrEmpty(arg[2..]) ? arg[2..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifySearchWord);
                            }
                            else if (arg.StartsWith("-r=", StringComparison.OrdinalIgnoreCase))
                            {
                                options = !string.IsNullOrEmpty(arg[3..]) ? arg[3..]
                                    : throw new Exception(Properties.Resources.DidNotSpecifyExecOption);
                                isExecute = true;
                            }
                            else if (arg.StartsWith("--platform=", StringComparison.OrdinalIgnoreCase))
                            {
#if !LINUX
                                string p = !string.IsNullOrEmpty(arg[11..]) ? arg[11..] : "x86";
                                if (p.Equals("x64", StringComparison.OrdinalIgnoreCase))
                                    platform = Platform.x64;
                                else
                                    platform = Platform.x86;
                                isNoCompile = false;
#else
                                messages += string.Format(Properties.Resources.ErrorOS, "--platform=") + Environment.NewLine;
#endif
                            }
                            else if (arg.StartsWith("--temp=", StringComparison.OrdinalIgnoreCase))
                            {
                                templateDirectoryName = !string.IsNullOrEmpty(arg[7..]) ? arg[7..].TrimEnd(Path.DirectorySeparatorChar) : "default";
                                isCopyTemplateDirectory = true;
                            }
                            else if (arg.StartsWith("--tempa=", StringComparison.OrdinalIgnoreCase))
                            {
                                addTemplateDirectoryPath = !string.IsNullOrEmpty(arg[8..]) ? arg[8..].TrimEnd(Path.DirectorySeparatorChar)
                                    : throw new Exception(Properties.Resources.DidNotSpecifySrcDirectory);
                                isAddTemplateDirectory = true;
                            }
                            else if (arg.StartsWith("--tempd=", StringComparison.OrdinalIgnoreCase))
                            {
                                delTemplateDirectoryName = !string.IsNullOrEmpty(arg[8..]) ? arg[8..].TrimEnd(Path.DirectorySeparatorChar) : "default";
                                isDelTemplateDirectory = true;
                            }
                            else if (arg.StartsWith("--tempo=", StringComparison.OrdinalIgnoreCase))
                            {
                                outputTemplateDirectoryName = arg[8..].TrimEnd(Path.DirectorySeparatorChar);
                            }
                            else if (arg.StartsWith("--profilea=", StringComparison.OrdinalIgnoreCase))
                            {
                                isAddProfile = true;
                                addProfileName = arg[11..];
                            }
                            else if (arg.StartsWith("--profiled=", StringComparison.OrdinalIgnoreCase))
                            {
                                isDeleteProfile = true;
                                deleteProfileName = arg[11..];
                            }
                            else if (arg.StartsWith("--refname=", StringComparison.OrdinalIgnoreCase))
                            {
#if !LINUX
                                refName = arg[10..];
                                isNoCompile = false;
#else
                                Console.WriteLine(Properties.Resources.ErrorOS, "--refname");
#endif
                            }
                            else if (arg.StartsWith("-", StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine(Properties.Resources.InvalidOption, arg);
                            }
                            else
                            {
                                sourceFilePath = arg;
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
                count++;
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
            // seeオプション
            if (isOutputOptions)
            {
                Console.WriteLine(string.Join(' ', args));
                return;
            }
            // configの読み込み
            ConfigRead();
            SettingSystemPath();
            // --help オプション
            if (isOutputHelpTexts)
            {
                Console.WriteLine();
                var argsList = args.ToList();
                argsList.RemoveRange(0, count);
                if (argsList.Count == 0)
                    argsList.Add("help");
                var help = new Help(sysDirectoryPath, Properties.Resources.Culture.Name, isCatchErrors);
                Console.WriteLine(help.GetHelpTests(argsList.ToArray()));
                return;
            }
            // --profileaオプション
            if (isAddProfile)
            {
                ProfileaOption(args);
                return;
            }
            SettingCommonPath();
            SettingRuntimePath();
            SettingTemplatesPath();
            // -temp オプション
            if (isCopyTemplateDirectory)
            {
                TryTempOption();
            }
            if (sourceFilePath == string.Empty && !isNoCompile)
            {
                OutputError(Properties.Resources.DidNotSpecifySrcFile, isCatchErrors);
            }
            if (!File.Exists(sourceFilePath) && !isNoCompile)
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
            // ドキュメントブラウザを開く
            if (!string.IsNullOrEmpty(searchWord) && !Help.OpenHspDocument(searchWord, sysDirectoryPath, false))
            {
                Console.WriteLine(Properties.Resources.CouldNotOpenDoc);
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
                    // -r オプション
                    if (res)
                    {
                        // 実行
                        if (isExecute)
                        {
                            // 自動実行ファイル名取得
                            if (parser.ExeName != "")
                            {
                                // 自動実行ファイル実行
                                compile.ExecuteExe(Path.Combine(Path.GetDirectoryName(sourceFilePath), parser.ExeName), options);
                                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage) + Environment.NewLine);
                            }
                            else
                            {
                                var name = compile.CompileMessage.Where((s) => s.Contains("Make custom execute file")).FirstOrDefault() ?? "";
                                if (name != "")
                                {
                                    name = name[(name.IndexOf("[") + 1)..name.IndexOf("]")];
                                    // 自動実行ファイル実行
                                    compile.ExecuteExe(Path.Combine(Path.GetDirectoryName(sourceFilePath), name), options);
                                    Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage));
                                }
                            }
                        }
                        // ランタイム表示
                        else if (isOutputRuntimeName)
                        {
                            string runtimeName = Compile.GetRuntime(!string.IsNullOrEmpty(outputFileName) ? outputFileName : sourceFilePath);
                            Console.WriteLine($"Runtime[{runtimeName}]");
                        }
                    }
                }
#endif
            }
            // -m オプションなし
            else if (!isNoCompile)
            {
                CreateAx(compile, out res, out CompileTextParser parser);
                // -r オプション
                if (res || parser.Errors.Count == 0)
                {
                    // 実行
                    if (isExecute)
                    {
                        compile.ExcuteAx(outputFileName, options);
                        Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage));
                    }
                    // ランタイム表示
                    else if (isOutputRuntimeName)
                    {
                        string runtimeName = Compile.GetRuntime(!string.IsNullOrEmpty(outputFileName) ? outputFileName : sourceFilePath);
                        Console.WriteLine($"Runtime[{runtimeName}]");
                    }
                }
            }
            // -r オプション & コンパイルなし
            else if (isExecute)
            {
                compile.ExcuteAx(sourceFilePath, options);
                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage));
            }
            // -r0 オプション
            else if (isOutputRuntimeName)
            {
                string runtimeName = Compile.GetRuntime(!string.IsNullOrEmpty(outputFileName) ? outputFileName : sourceFilePath);
                Console.WriteLine($"Runtime[{runtimeName}]");
            }
            // -D オプション
            if (isDeleteFiles)
            {
                DeleteFiles();
            }
            // --tempd オプション
            if (isDelTemplateDirectory)
            {
                TempdOption();
            }
            // --tempa オプション
            if (isAddTemplateDirectory)
            {
                TryTempaOption();
            }
            // --profiled オプション
            if (isDeleteProfile)
            {
                ProfiledOption();
            }
        }

        /// <summary>
        /// Dオプションの実行
        /// </summary>
        private static void DeleteFiles()
        {
            if (isMakeExe)
            {
                if (File.Exists(Path.Combine(Path.GetDirectoryName(sourceFilePath), "start.ax")))
                {
                    File.Delete(Path.Combine(Path.GetDirectoryName(sourceFilePath), "start.ax"));
                    Console.WriteLine(Properties.Resources.DeleteA, Path.Combine(Path.GetDirectoryName(sourceFilePath), "start.ax"));
                }
                if (File.Exists(Path.Combine(Path.GetDirectoryName(sourceFilePath), "packfile")) && ppout.HasFlag(PreprocessOptions.MakePackfile))
                {
                    File.Delete(Path.Combine(Path.GetDirectoryName(sourceFilePath), "packfile"));
                    Console.WriteLine(Properties.Resources.DeleteA, Path.Combine(Path.GetDirectoryName(sourceFilePath), "packfile"));
                }
            }
            else if (!isNoCompile)
            {
                if (File.Exists(outputFileName))
                {
                    File.Delete(outputFileName);
                    Console.WriteLine(Properties.Resources.DeleteA, outputFileName);
                }
            }
        }

#if !LINUX
        private static bool CreateExe(Compile compile, CompileTextParser parser)
        {
            bool res = !compile.CreateExe();
            if (isViewNewCmpMessage)
            {
                parser.ParsDPM(compile.CompileMessage, Properties.Resources.Culture.Name);
                if (isViewLogo) Console.WriteLine(string.Join(Environment.NewLine, parser.Logos));
                Console.WriteLine(string.Join(Environment.NewLine, parser.Errors.Concat(parser.Infomation)) + Environment.NewLine);
            }
            else
                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage) + Environment.NewLine);
            return res;
        }
#endif

        private static void CreateAx(Compile compile, out bool res, out CompileTextParser parser)
        {
            res = !compile.CreateObj(ref outputFileName, refName, mode, ppout, isDebugWindow);
            parser = new CompileTextParser();
            if (isViewNewCmpMessage)
            {
                parser.Pars(compile.CompileMessage, Properties.Resources.Culture.Name);
                if (isViewLogo) Console.WriteLine(string.Join(Environment.NewLine, parser.Logos));
                Console.WriteLine(string.Join(Environment.NewLine, parser.Errors.Concat(parser.Warns).Concat(parser.Infomation)) + Environment.NewLine);
            }
            else
                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage) + Environment.NewLine);
        }

        /// <summary>
        /// AssemblyInfo の作成
        /// </summary>
        /// <param name="compile"></param>
        /// <returns>作成成功したかどうか。成功したら true 失敗したら false</returns>
        private static bool CreateAsmInfo(Compile compile)
        {
            bool res;
            var asmName = Path.GetFileName(outputFileName) ?? outputFileName;
            if (asmName.Length != 0 && asmName.IndexOf('.') != -1)
            {
                asmName = asmName[..asmName.IndexOf('.')];
            }
            res = compile.CreateAssemblyInfo(platform, mode, ppout, asmName);
            if (compile.CompileMessage.Count != 0)
            {
                Console.WriteLine(string.Join(Environment.NewLine, compile.CompileMessage));
            }
            if (res)
            {
                Console.WriteLine(Properties.Resources.CreateAssemblyInfo);
                res = false;
            }
            return res;
        }

        /// <summary>
        /// u オプションの実行
        /// </summary>
        private static void EncodeSourceFile()
        {
            int charset = HspcuiBase.Utils.Util.CheckUTF8(sourceFilePath);
            if (charset == 0)
            {
                Console.WriteLine(Properties.Resources.CompileCharset, "utf-8");
                if (!ppout.HasFlag(PreprocessOptions.InputUTF8))
                    ppout |= PreprocessOptions.InputUTF8;
            }
            else if (charset == 1)
            {
                if (HspcuiBase.Utils.Util.ConvertFileToUtf8(sourceFilePath, isCatchErrors))
                {
                    Console.WriteLine(Properties.Resources.EncodeShitjisToUtf8);
                    if (!ppout.HasFlag(PreprocessOptions.InputUTF8))
                        ppout |= PreprocessOptions.InputUTF8;
                }
                else
                {
                    Console.WriteLine(Properties.Resources.FailedEncodeShitjisToUtf8);
                }
            }
            else if (charset == -1)
            {
                OutputError(Properties.Resources.InvalidCharset, isCatchErrors);
            }
        }

        /// <summary>
        /// a オプションの実行
        /// </summary>
        private static void CharsetCheck()
        {
            int charset = HspcuiBase.Utils.Util.CheckUTF8(sourceFilePath);
            if (charset == 0 && !ppout.HasFlag(PreprocessOptions.InputUTF8))
            {
                ppout |= PreprocessOptions.InputUTF8;
                Console.WriteLine(Properties.Resources.CompileCharset, "utf-8");
            }
            else if (charset == 1 && ppout.HasFlag(PreprocessOptions.InputUTF8))
            {
                ppout &= ~PreprocessOptions.InputUTF8;
                Console.WriteLine(Properties.Resources.CompileCharset, "shift-jis");
            }
            else if (charset == -1)
            {
                OutputError(Properties.Resources.InvalidCharset, isCatchErrors);
            }
        }

        /// <summary>
        /// C オプションの実行
        /// </summary>
        private static void SetCurrentDirectory()
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(sourceFilePath));
            if (!Path.IsPathFullyQualified(sourceFilePath))
            {
                sourceFilePath = Path.GetFileName(sourceFilePath);
            }
            Console.WriteLine(Properties.Resources.CurrentDirectorySetTo, Environment.CurrentDirectory);
        }

        /// <summary>
        /// profiled オプションの実行
        /// </summary>
        private static void ProfiledOption()
        {
            if (Config is null)
                Config = new Config(isCatchErrors);
            if (Config.DeleteProfile(deleteProfileName))
                Console.WriteLine(Properties.Resources.DeleteA, deleteProfileName);
        }

        /// <summary>
        /// profilea オプションの実行
        /// </summary>
        /// <param name="args"></param>
        private static void ProfileaOption(string[] args)
        {
            if (Config is null)
                Config = new Config(isCatchErrors);
            if (Config.AddProfileCommand(args.ToList(), addProfileName))
                Console.WriteLine(Properties.Resources.SpecifiedProfileSet, addProfileName);
        }

        /// <summary>
        /// tempa オプションの実行
        /// </summary>
        private static void TryTempaOption()
        {
            try
            {
                if (!Directory.Exists(addTemplateDirectoryPath)) throw new Exception(string.Format(Properties.Resources.NotFoundSpecifySrcDirectory, addTemplateDirectoryPath));
                addTemplateDirectoryPath += Path.DirectorySeparatorChar;
                string addTemplateDirectoryName = Path.GetDirectoryName(addTemplateDirectoryPath);
                if (!string.IsNullOrEmpty(outputTemplateDirectoryName))
                    addTemplateDirectoryName = outputTemplateDirectoryName;
                int index = addTemplateDirectoryName.LastIndexOf(Path.DirectorySeparatorChar);
                if (index == -1)
                {
                    addTemplateDirectoryName = Path.Combine(templateDirectoryPath, addTemplateDirectoryName);
                }
                else
                {
                    addTemplateDirectoryName = Path.Combine(templateDirectoryPath, addTemplateDirectoryName[(index + 1)..]);
                }
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(addTemplateDirectoryPath, addTemplateDirectoryName, true);
                Console.WriteLine(Properties.Resources.CopyAtoB, addTemplateDirectoryPath, addTemplateDirectoryName);
            }
            catch (Exception ex)
            {
                OutputError(ex, isCatchErrors);
            }
        }

        /// <summary>
        /// tempd オプションの実行
        /// </summary>
        private static void TempdOption()
        {
            templateDirectoryPath = CheckDirectory(delTemplateDirectoryName, templateDirectoryPath, delTemplateDirectoryName, true, false).TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(templateDirectoryPath))
            {
                Console.WriteLine(Properties.Resources.DeleteTemplateCaution, delTemplateDirectoryName);
                var str = Console.ReadLine();
                if (str.Equals("Y", StringComparison.InvariantCulture))
                {
                    Directory.Delete(templateDirectoryPath, true);
                    Console.WriteLine(Properties.Resources.DeleteATemplate, delTemplateDirectoryName);
                }
            }
        }

        /// <summary>
        /// temp オプションの実行
        /// </summary>
        private static void TryTempOption()
        {
            try
            {
                templateDirectoryPath = CheckDirectory(templateDirectoryName, templateDirectoryPath, templateDirectoryName, true, isCatchErrors).TrimEnd(Path.DirectorySeparatorChar);
                if (!string.IsNullOrEmpty(outputTemplateDirectoryName))
                    templateDirectoryName = outputTemplateDirectoryName;
                if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, templateDirectoryName)))
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(templateDirectoryPath, Path.Combine(Environment.CurrentDirectory, templateDirectoryName), true);
                    Console.WriteLine(Properties.Resources.CopyAtoB, templateDirectoryPath, Path.Combine(Environment.CurrentDirectory, templateDirectoryName));
                }
                else
                    Console.WriteLine(Properties.Resources.AlreadyExistsInCurrentDirectory, templateDirectoryName);
            }
            catch (Exception ex)
            {
                OutputError(ex, isCatchErrors);
            }
        }

        /// <summary>
        /// HSP システムフォルダの設定 (--syspath オプションの処理)
        /// 指定されなかったり指定されたフォルダが存在しなかったりすれば、このアプリの存在するフォルダが割り当てられる。
        /// </summary>
        private static void SettingSystemPath()
        {
            if (!string.IsNullOrEmpty(sysDirectoryPath))
            {
                if (!Directory.Exists(sysDirectoryPath.TrimEnd(Path.DirectorySeparatorChar)))
                {
                    Console.WriteLine(Properties.Resources.NotFoundSpecifySysDirectory, sysDirectoryPath);
                    sysDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    Console.WriteLine(Properties.Resources.SysDirectorySetTo, sysDirectoryPath);
                }
            }
            else
            {
                sysDirectoryPath = System.AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine(Properties.Resources.SysDirectorySetTo, sysDirectoryPath);
            }
        }

        /// <summary>
        /// Templates ディレクトリパスの設定
        /// </summary>
        private static void SettingTemplatesPath()
        {
            // --tmppath= オプションなし
            if (string.IsNullOrEmpty(templateDirectoryPath))
            {
                templateDirectoryPath = Path.Combine(sysDirectoryPath, $"templates{Path.DirectorySeparatorChar}");
                Console.WriteLine(Properties.Resources.TmpDirectorySetTo, templateDirectoryPath);
            }
            // templates ディレクトリの存在の確認
            templateDirectoryPath = CheckDirectory(templateDirectoryPath, sysDirectoryPath, "templates", catchError: isCatchErrors).TrimEnd(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// runtime ディレクトリパスの設定
        /// </summary>
        private static void SettingRuntimePath()
        {
            // --rtmpath= オプションなし
            if (string.IsNullOrEmpty(runtimeDirectoryPath))
            {
                runtimeDirectoryPath = Path.Combine(sysDirectoryPath, $"runtime{Path.DirectorySeparatorChar}");
                Console.WriteLine(Properties.Resources.RtmDirectorySetTo, runtimeDirectoryPath);
            }
            // runtime ディレクトリの存在の確認
            runtimeDirectoryPath = CheckDirectory(runtimeDirectoryPath, sysDirectoryPath, "runtime", catchError: isCatchErrors);
        }

        /// <summary>
        /// common ディレクトリパスの設定
        /// </summary>
        private static void SettingCommonPath()
        {
            // --compath= オプションなし
            if (string.IsNullOrEmpty(commonDirectoryPath))
            {
                commonDirectoryPath = Path.Combine(sysDirectoryPath, $"common{Path.DirectorySeparatorChar}");
                Console.WriteLine(Properties.Resources.ComDirectorySetTo, commonDirectoryPath);
            }
            // common ディレクトリの存在の確認
            commonDirectoryPath = CheckDirectory(commonDirectoryPath, sysDirectoryPath, "common", catchError: isCatchErrors);
        }

        /// <summary>
        /// config のデータ取得
        /// </summary>
        private static void ConfigRead()
        {
            // config からディレクトリパス取得
            if (Config.FileExist)
            {
                if (Config is null)
                    Config = new Config(isCatchErrors);
                if (!string.IsNullOrEmpty(Config.SystemDirectory) && string.IsNullOrEmpty(sysDirectoryPath))
                {
                    sysDirectoryPath = Config.SystemDirectory;
                }
                if (!string.IsNullOrEmpty(Config.CommonDirectory) && string.IsNullOrEmpty(commonDirectoryPath))
                {
                    commonDirectoryPath = Config.CommonDirectory;
                }
                if (!string.IsNullOrEmpty(Config.TemplatesDirectory) && string.IsNullOrEmpty(templateDirectoryPath))
                {
                    templateDirectoryPath = Config.TemplatesDirectory;
                }
                if (!string.IsNullOrEmpty(Config.RuntimeDirectory) && string.IsNullOrEmpty(runtimeDirectoryPath))
                {
                    runtimeDirectoryPath = Config.RuntimeDirectory;
                }
            }
        }
    }
}