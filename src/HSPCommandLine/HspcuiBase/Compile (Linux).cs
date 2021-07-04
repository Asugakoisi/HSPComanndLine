using HspcuiBase.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using HspcuiBase.Utils;

namespace HspcuiBase.Compile
{
    public partial class Compile
    {
#if LINUX
        private ProcessStartInfo startInfo = new()
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            FileName = "hspcmp"
        };

        private static void MakeArgument(ref string arg, ModeOptions mode, PreprocessOptions ppout, bool debug, string CompPath)
        {
            if (debug)
                arg += "-w ";
            if (!mode.HasFlag(ModeOptions.None))
            {
                if (mode.HasFlag(ModeOptions.Debug))
                    arg += "-d ";
                if (mode.HasFlag(ModeOptions.OnlyPreprocess))
                    arg += "-p ";
                if (mode.HasFlag(ModeOptions.OutputUTF8))
                    arg += "-u ";
                if (mode.HasFlag(ModeOptions.OutputStrmap))
                    arg += "-s ";
            }
            if (!ppout.HasFlag(PreprocessOptions.None))
            {
                if (ppout.HasFlag(PreprocessOptions.Ver26))
                    arg += "-c ";
                if (ppout.HasFlag(PreprocessOptions.InputUTF8))
                    arg += "-i ";
            }
            if (!string.IsNullOrEmpty(CompPath))
                arg += $"--compath={CompPath} ";
        }

        public bool CreateObj(ref string axName, string refName, ModeOptions mode = 0, PreprocessOptions ppout = 0, bool debug = false)
        {
            CompileMessage.Clear();
            string cmpMessage = "";
            bool res;
            string arg = string.Empty;
            if (mode.HasFlag(ModeOptions.OutputStrmap) && !mode.HasFlag(ModeOptions.OnlyPreprocess))
            {
                string beforeAxName = axName;
                bool onDebugOption = false;
                if (mode.HasFlag(ModeOptions.Debug))
                    mode &= ~ModeOptions.Debug;
                MakeArgument(ref arg, mode, ppout, debug, CommonDirectoryPath);
                if (!string.IsNullOrEmpty(axName))
                    arg += $"-o{Path.ChangeExtension(axName, "strmap")} ";
                else
                    axName = Path.ChangeExtension(SourceFilePath, "strmap");
                using var process1 = new Process();
                startInfo.Arguments = arg + SourceFilePath;
                process1.StartInfo = startInfo;
                res = !process1.Start();
                //Hsc_getmes(errbuf, 0, 0, 0);   // 結果を取得
                cmpMessage += process1.StandardOutput?.ReadToEnd() ?? "";
                if (onDebugOption) mode |= ModeOptions.Debug;
                if (res) return true;
                else
                {
                    File.Move(Path.ChangeExtension(axName, "ax"), axName, true);
                    if (!ppout.HasFlag(PreprocessOptions.MakePackfile) && !ppout.HasFlag(PreprocessOptions.Ver26) && !ppout.HasFlag(PreprocessOptions.MakeAHT) && !mode.HasFlag(ModeOptions.Debug)) return false;
                    cmpMessage += Environment.NewLine;
                    mode &= ~ModeOptions.OutputStrmap;
                    axName = beforeAxName;
                }
            }
            arg = string.Empty;
            MakeArgument(ref arg, mode, ppout, debug, CommonDirectoryPath);
            if (!string.IsNullOrEmpty(axName))
            {
                axName = Path.ChangeExtension(axName, "ax");
                arg += $"-o{axName} ";
            }
            else
                axName = Path.ChangeExtension(SourceFilePath, "ax");
            using var process = new Process();
            startInfo.Arguments = arg + SourceFilePath;
            process.StartInfo = startInfo;
            res = !process.Start();
            //Hsc_getmes(errbuf, 0, 0, 0);   // 結果を取得
            cmpMessage += process.StandardOutput?.ReadToEnd() ?? "";
            CompileMessage.AddRange(cmpMessage.TrimEnd('\n').TrimEnd('\r').Replace("\r\n", "\n").Split('\n'));
            return res;
        }

        public bool CreateObj(string axName, string refName, ModeOptions mode = 0, PreprocessOptions ppout = 0, bool debug = false)
        {
            return CreateObj(ref axName, refName, mode, ppout, debug);
        }

        public int ExcuteAx(string axname, string option = "")
        {
            CompileMessage.Clear();
            try
            {
                if (!File.Exists(axname)) throw new Exception(string.Format(Properties.Resources.NotFoundA, axname));
                var startInfo = new ProcessStartInfo
                {
                    FileName = GetRuntime(axname),
                    Arguments = $"{Path.GetFullPath(axname)} {option}",
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(axname)
                };
                if (!File.Exists(startInfo.FileName))
                {
                    if (!File.Exists(Path.Combine(SystemDirectoryPath, startInfo.FileName)))
                    {
                        Console.WriteLine(Properties.Resources.NotFoundA, Path.Combine(SystemDirectoryPath, startInfo.FileName));
                        if (!File.Exists(Path.Combine(RuntimeDirectoryPath, startInfo.FileName)))
                            throw new Exception(string.Format(Properties.Resources.NotFoundA, Path.Combine(RuntimeDirectoryPath, startInfo.FileName)));
                        else
                            startInfo.FileName = Path.Combine(RuntimeDirectoryPath, startInfo.FileName);
                    }
                    else
                        startInfo.FileName = Path.Combine(SystemDirectoryPath, startInfo.FileName);
                }
                using var process = new Process
                {
                    StartInfo = startInfo
                };
                if (!process.Start())
                {
                    throw new Exception(string.Format(Properties.Resources.CouldNotRunRuntime, process.StartInfo.FileName));
                }
                process.WaitForExit();
                CompileMessage.Add($"ReturnCode[{axname}] " + process.ExitCode.ToString());
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                if (CatchErrors) throw;
                else CompileMessage.AddRange(ex.Message.TrimEnd('\n').TrimEnd('\r').Replace("\r\n", "\n").Split('\n'));
                return -2;
            }
        }

        public static string GetRuntime(string axName)
        {
            using var process = new Process();
            ProcessStartInfo startInfo1 = new()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                FileName = "hspcmp",
                Arguments = axName + " -r0"
            };
            process.StartInfo = startInfo1;
            if (process.Start())
            {
                string runtimeName = process.StandardOutput.ReadToEnd();
                if(runtimeName.Length > 8)
                {
                    if (runtimeName[8..^3] == "hsp3")
                        return "hsp3cl";
                    return runtimeName[8..^3];
                }
            }
            return "";
        }
#endif
    }
}
