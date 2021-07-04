using static HSPCommandLine.Utils.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HSPCommandLine
{
    internal class Help
    {
#pragma warning disable IDE0044 // 読み取り専用修飾子を追加します
        private HelpTexts helpTexts;
#pragma warning restore IDE0044 // 読み取り専用修飾子を追加します
        private readonly bool isCatchErrors;

        internal Help(string systemDirectorypath, string lang = "ja", bool error = true)
        {
            if (lang.Length > 2)
                lang = lang[0..lang.IndexOf('-')];
            isCatchErrors = error;
            helpTexts = JsonSerializer.Deserialize<HelpTexts>(Encoding.UTF8.GetBytes(Properties.Resources.hspcui));
            foreach (var option in helpTexts.Options)
            {
                if (Regex.IsMatch(option.Desctription, "^*.txt$"))
                    option.Desctription = Properties.Resources.ResourceManager.GetString(option.Desctription.Replace('-', '_')[0..^4], Properties.Resources.Culture);
                if (Regex.IsMatch(option.Note, "^*.txt$"))
                    option.Note = Properties.Resources.ResourceManager.GetString(option.Note.Replace('-', '_')[0..^4], Properties.Resources.Culture);
                if (Regex.IsMatch(option.Example, "^*.txt$"))
                    option.Example = Properties.Resources.ResourceManager.GetString(option.Example.Replace('-', '_')[0..^4], Properties.Resources.Culture);
            }
        }

        internal string GetHelpTests(string[] args)
        {
            string text = "";
            foreach (var arg in args)
            {
                if (arg.Equals("--ls", StringComparison.OrdinalIgnoreCase))
                {
                    text = GetAllHelpText();
                }
                else
                {
                    text += GetHelpText(arg);
                }
            }
            return text;
        }

        private string GetAllHelpText()
        {
            var text = "";
            foreach (var option in helpTexts.Options)
            {
                if (option.Linux)
                {
                    if (option.Command)
                        continue;
                    string bars = option.Name.Length > 2 ? "--" : "-";
                    text += $"{bars}{option.Name} {Properties.Resources.Optiondetails}{Environment.NewLine}{Properties.Resources.Desctription}:{Environment.NewLine}{option.Desctription}{Environment.NewLine}";
                    text += Environment.NewLine;
                }
            }
            return text;
        }

        private string GetHelpText(string optionText)
        {
            var option = helpTexts.Options.Where((opt) => opt.Name == optionText).FirstOrDefault();
            string bars = optionText.Length > 2 ? "--" : "-";
            if (option is not null)
            {
                if (option.Linux)
                {
                    var text = $"{bars}{optionText} {Properties.Resources.Optiondetails}{Environment.NewLine}{Properties.Resources.Desctription}:{Environment.NewLine}{option.Desctription}{Environment.NewLine}";
                    if (option.Command)
                        text = $"{option.Desctription}{Environment.NewLine}";
                    text += string.IsNullOrEmpty(option.Note) ? "" : $"{Properties.Resources.Note}:{Environment.NewLine}{option.Note}{Environment.NewLine}";
                    text += string.IsNullOrEmpty(option.Example) ? "" : $"{Properties.Resources.Example}:{Environment.NewLine}{option.Example}{Environment.NewLine}";
                    return text + Environment.NewLine;
                }
                else
                    return string.Format(Properties.Resources.ErrorOS, $"{bars}{optionText}");
            }
            else
                return string.Format(Properties.Resources.NotFoundSpecifyOptionHelp, optionText);
        }

#pragma warning disable IDE0060 // 未使用のパラメーターを削除します
        internal static bool OpenHspDocument(string word, string systemDirectoryPath, bool online)
#pragma warning restore IDE0060 // 未使用のパラメーターを削除します
        {
            if (File.Exists(Path.Combine(systemDirectoryPath, "hdl.exe")))
            {
                using var process = new Process();
                process.StartInfo.FileName = Path.Combine(systemDirectoryPath, "hdl.exe");
                process.StartInfo.Arguments = word;
                return process.Start();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessStartInfo startInfo = new()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    FileName = "hspcmp",
                    Arguments = $"-h{word.Trim()}"
                };
                using var process = new Process() { StartInfo = startInfo };
                bool ret = process.Start();
                if (ret)
                {
                    Console.WriteLine(process.StandardOutput.ReadToEnd());
                }
                return ret;
            }
            else
            {
                throw new Exception(Properties.Resources.NotFoundHDL);
            }
        }
    }

    internal class HelpTexts
    {
        [JsonPropertyName("ver")]
        public string Version { get; set; }
        [JsonPropertyName("lang")]
        public string Lang { get; set; }
        [JsonPropertyName("options")]
        public Option[] Options { get; set; }
    }

    internal class Option
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("desctription")]
        public string Desctription { get; set; }
        [JsonPropertyName("example")]
        public string Example { get; set; }
        [JsonPropertyName("note")]
        public string Note { get; set; }
        [JsonPropertyName("linux")]
        public bool Linux { get; set; }
        [JsonPropertyName("command")]
        public bool Command { get; set; } = false;
    }

}
