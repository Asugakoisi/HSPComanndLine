using HspcuiBase.Options;
using HspcuiBase.Utils;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.IO;
using static System.IO.Path;
using System.Linq;
using System.Resources;
using System.Text;
using HspcuiBase.Compile;

namespace HSPC
{
    public class Hspc : Task
    {
        public bool AddDegugInfo { get; set; }
        public bool AsHSP26 { get; set; }
        public bool Nologo { get; set; }
        [Required]
        public string SystemDirectory { get; set; }
        public string CommonDirectory { get; set; } = string.Empty;
        public string RuntimeDirectory { get; set; } = string.Empty;
        public string RefName { get; set; } = string.Empty;
        public bool MakeExe { get; set; }
        public bool DeleteFiles { get; set; }
        public bool OnDebugWindow { get; set; }
        public bool CreateAssmblyInfo { get; set; } = true;
        public string Output { get; set; } = string.Empty;
        public string Platform { get; set; } = "x86";
        [Required]
        public string SourceFile { get; set; }

        public Hspc() => Initialize();

        public Hspc(ResourceManager taskResources) : base(taskResources) => Initialize();

        public Hspc(ResourceManager taskResources, string helpKeywordPrefix) : base(taskResources, helpKeywordPrefix) => Initialize();

        public override bool Execute()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if (SourceFile is null || string.IsNullOrEmpty(SourceFile))
            {
                Log.LogError(Properties.Resources.DidNotSpecifySrcFile);
                return false;
            }
            if(SystemDirectory is null || string.IsNullOrEmpty(SystemDirectory))
            {
                Log.LogError(Properties.Resources.DidNotSpecifySysDirectory);
                return false;
            }
            string outputFileName = string.Empty;
            Platform platform = HspcuiBase.Options.Platform.x86;
            ModeOptions mode = ModeOptions.None;
            PreprocessOptions ppout = PreprocessOptions.None;
            if (AsHSP26) ppout |= PreprocessOptions.Ver26;
            if (AddDegugInfo) mode |= ModeOptions.Debug;
            if (MakeExe) ppout |= PreprocessOptions.MakePackfile;

            Console.WriteLine("\n");

            // HSP システムフォルダの設定
            if (!string.IsNullOrEmpty(SystemDirectory))
            {
                if (!Directory.Exists(SystemDirectory.TrimEnd('\\')))
                {
                    Console.WriteLine(Properties.Resources.NotFoundSpecifySysDirectory, SystemDirectory);
                    SystemDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    Console.WriteLine(Properties.Resources.SysDirectorySetTo, SystemDirectory);
                }
            }
            else
            {
                SystemDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine(Properties.Resources.SysDirectorySetTo, SystemDirectory);
            }
            // common ディレクトリの設定
            if (string.IsNullOrEmpty(CommonDirectory))
            {
                CommonDirectory = Path.Combine(SystemDirectory, "common");
                Console.WriteLine(Properties.Resources.ComDirectorySetTo, CommonDirectory);
                CommonDirectory += "\\";
            }
            // runtime ディレクトリの設定
            if (string.IsNullOrEmpty(RuntimeDirectory))
            {
                RuntimeDirectory = Path.Combine(SystemDirectory, "runtime");
                Console.WriteLine(Properties.Resources.RtmDirectorySetTo, RuntimeDirectory);
                RuntimeDirectory += "\\";
            }
            // common, runtime ディレクトリの存在の確認
            CommonDirectory = CheckDirectory(CommonDirectory, SystemDirectory, "common");
            RuntimeDirectory = CheckDirectory(RuntimeDirectory, SystemDirectory, "runtime");
            // ソースファイルの確認
            if (!File.Exists(SourceFile))
            {
                Log.LogError(Properties.Resources.NotFoundSpecifySrcFile, SourceFile);
                return false;
            }
            // 文字コードの確認
            int charset = Util.CheckUTF8(SourceFile);
            if (charset == 0)
            {
                ppout |= PreprocessOptions.InputUTF8;
                Console.WriteLine(Properties.Resources.CompileCharset, "utf-8");
            }
            else if (charset == 1)
            {
                Console.WriteLine(Properties.Resources.CompileCharset, "shift-jis");
            }
            else if (charset == -1)
            {
                throw new Exception(Properties.Resources.InvalidCharset);
            }
            // プラットフォームの確認
            if (string.IsNullOrEmpty(Platform))
            {
                if (Platform.Equals("x64", StringComparison.OrdinalIgnoreCase))
                    platform = HspcuiBase.Options.Platform.x64;
                else
                    platform = HspcuiBase.Options.Platform.x86;
            }

            Console.WriteLine("\r\n");

            // コンパイル
            var compile = new Compile(SourceFile, CommonDirectory, SystemDirectory);
            bool res;
            // --platform -i -a(utf8) オプション
            if (CreateAssmblyInfo)
            {
                var asmName = GetFileName(outputFileName) ?? outputFileName;
                if (asmName.Length != 0 && asmName.IndexOf('.') != -1)
                {
                    asmName = asmName.Remove(asmName.IndexOf('.'));
                }
                // 改造すること。
                res = compile.CreateAssemblyInfo(platform, mode, ppout, asmName);
                if (compile.CompileMessage.Count != 0)
                {
                    foreach (var error in compile.CompileMessage)
                    {
                        Log.LogError(error);
                    }
                    return false;
                }
                if (res)
                {
                    Console.WriteLine(Properties.Resources.CreateAssemblyInfo);
                }
            }
            // -m オプション
            if (MakeExe)
            {
                // start.ax 生成 (-P オプションでコードから packfile 生成)
                res = !compile.CreateObj(Combine(GetDirectoryName(SourceFile), "start.ax"), RefName, mode, ppout, OnDebugWindow);
                var parser = new CompileTextParser();
                parser.Pars(compile.CompileMessage, Properties.Resources.Culture.Name);
                if (!Nologo) Console.WriteLine(string.Join(Environment.NewLine, parser.Logos));
                Console.WriteLine(string.Join(Environment.NewLine, parser.Infomation) + Environment.NewLine);
                if (parser.Warns.Count != 0)
                {
                    foreach (var warn in parser.Warns)
                    {
                        Log.LogWarning(warn);
                    }
                }
                if (parser.Errors.Count != 0)
                {
                    foreach (var error in parser.Errors)
                    {
                        Log.LogError(error);
                    }
                    return false;
                }
                if (res)
                {
                    // packfile に基づいて自動実行ファイル作成
                    compile.CreateExe();
                    parser.ParsDPM(compile.CompileMessage, Properties.Resources.Culture.Name);
                    if (!Nologo) Console.WriteLine(string.Join(Environment.NewLine, parser.Logos));
                    Console.WriteLine(string.Join(Environment.NewLine, parser.Infomation) + Environment.NewLine);
                    if (parser.Errors.Count != 0)
                    {
                        foreach (var error in parser.Errors)
                        {
                            Log.LogError(error);
                        }
                        return false;
                    }
                }
            }
            // -m オプションなし
            else
            {
                compile.CreateObj(ref outputFileName, RefName, mode, ppout, OnDebugWindow);
                var parser = new CompileTextParser();
                parser.Pars(compile.CompileMessage, Properties.Resources.Culture?.Name ?? "ja");
                if (!Nologo) Console.WriteLine(string.Join(Environment.NewLine, parser.Logos));
                Console.WriteLine(string.Join(Environment.NewLine, parser.Infomation) + Environment.NewLine);
                if(parser.Warns.Count != 0)
                {
                    foreach (var warn in parser.Warns)
                    {
                        Log.LogWarning(warn);
                    }
                }
                if (parser.Errors.Count != 0)
                {
                    foreach (var error in parser.Errors)
                    {
                        Log.LogError(error);
                    }
                    return false;
                }
            }
            // -D オプション
            if (DeleteFiles)
            {
                if (MakeExe)
                {
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(SourceFile), "start.ax")))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(SourceFile), "start.ax"));
                        Console.WriteLine(Properties.Resources.DeleteA, Path.Combine(Path.GetDirectoryName(SourceFile), "start.ax"));
                    }
                    if (File.Exists(Path.Combine(Path.GetDirectoryName(SourceFile), "packfile")) && ppout.HasFlag(PreprocessOptions.MakePackfile))
                    {
                        File.Delete(Path.Combine(Path.GetDirectoryName(SourceFile), "packfile"));
                        Console.WriteLine(Properties.Resources.DeleteA, Path.Combine(Path.GetDirectoryName(SourceFile), "packfile"));
                    }
                }
            }

            return !Log.HasLoggedErrors;
        }

        private void Initialize()
        {
            this.HelpKeywordPrefix = "HSP";
            this.TaskResources = Properties.Resources.ResourceManager;
        }

        private string CheckDirectory(string checkDirectoryPath, string sysDirectoryPath, string directoryName, bool skipcurrentdirectory = false)
        {
            try
            {
                return Util.CheckDirectory(checkDirectoryPath, sysDirectoryPath, directoryName, skipcurrentdirectory);
            }
            catch (Exception ex)
            {
                Log.LogErrorFromException(ex);
                throw;
            }
        }
    }
}
