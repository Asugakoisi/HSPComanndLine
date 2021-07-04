using static HSPCommandLine.Utils.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HSPCommandLine
{
    internal class Config
    {
#pragma warning disable IDE0044 // 読み取り専用修飾子を追加します
        private ConfigText configText;
#pragma warning restore IDE0044 // 読み取り専用修飾子を追加します
        private readonly bool isCatchErrors;
        private readonly List<string> removeOptions = new()
        {
            "--help",
            "--profile=",
            "--lang="
        };
#if LINUX
        private readonly List<string> removeOptionsLinux = new()
        {
            "-a",
            "-j",
            "-P",
            "-m",
            "-w",
            "--notasminfo",
            "--platform=",
            "--refname="
        };
#endif

        public string CommonDirectory
        {
            get => configText.CommonDirectory;
        }
        public string TemplatesDirectory
        {
            get => configText.TemplatesDirectory;
        }
        public string SystemDirectory
        {
            get => configText.SystemDirectory;
        }
        public string RuntimeDirectory
        {
            get => configText.RuntimeDirectory;
        }

        public static bool FileExist
        {
            get => File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hspcuiconfig.json"));
        }

        internal Config(bool error = true)
        {
            isCatchErrors = error;
            string configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); 
            var version = typeof(Program).Assembly.GetName().Version;
            if (File.Exists(Path.Combine(configFilePath, ".hspcuiconfig.json")))
            {
                configText = JsonSerializer.Deserialize<ConfigText>(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(File.ReadAllText(Path.Combine(configFilePath, ".hspcuiconfig.json")))));
                if (!CheckTextVersion(version, configText.Version))
                {
                    OutputError(string.Format(Properties.Resources.NotCompatibleWithCurrentVersion, ".hspcuiconfig.json", version), isCatchErrors);
                }
            }
            else
            {
                OutputError(string.Format(Properties.Resources.NotFoundA, ".hspcuiconfig.json"), false);
                configText = new ConfigText
                {
                    Version = $"1.0.0.0-{version}"
                };
                TryConfigSave();
            }
        }

        internal List<string> GetProfileCommand(string arg)
        {
            if (int.TryParse(arg, out int id))
            {
                foreach (var profile in configText.Profiles)
                {
                    if (profile.Id == id)
                    {
                        return profile.Options.ToList();
                    }
                }
            }
            else
            {
                foreach (var profile in configText.Profiles)
                {
                    if (profile.Name.Equals(arg, StringComparison.CurrentCulture))
                    {
                        return profile.Options.ToList();
                    }
                }
            }
            return null;
        }

        internal bool AddProfileCommand(List<string> options, string name)
        {
            foreach (var item in removeOptions)
            {
                options.Remove(item);
            }
#if LINUX
            foreach (var item in removeOptionsLinux)
            {
                options.Remove(item);
            }
#endif
            options.Remove(options.Find(option => option.StartsWith("--profilea")));
            Profile profile = null;
            if (int.TryParse(name, out int id))
            {
                profile = configText.Profiles.Where(options => options.Id == id).First();
            }
            else if (configText.Profiles.Any(options => options.Name == name))
            {
                profile = configText.Profiles.Where(options => options.Name == name).First();
            }
            if (profile != null && configText.Profiles.Contains(profile))
            {
                foreach (var item in configText.Profiles)
                {
                    if (item.Equals(profile))
                        item.Options = options.ToArray();
                }
            }
            else
            {
                var tmp = configText.Profiles.ToList();
                profile = new Profile()
                {
                    Id = (configText.Profiles.OrderByDescending(a => a.Id).FirstOrDefault()?.Id ?? -1) + 1,
                    Name = name,
                    Options = options.ToArray()
                };
                tmp.Add(profile);
                configText.Profiles = tmp.ToArray();
            }
            return TryConfigSave();
        }

        internal bool DeleteProfile(string name)
        {
            Profile profile = null;
            if (int.TryParse(name, out int id))
            {
                profile = configText.Profiles.Where(options => options.Id == id).First();
            }
            else if (configText.Profiles.Any(options => options.Name == name))
            {
                profile = configText.Profiles.Where(options => options.Name == name)?.First();
            }
            if (profile is not null)
            {
                var tmp = configText.Profiles.ToList();
                tmp.Remove(profile);
                configText.Profiles = tmp.ToArray();
                return TryConfigSave();
            }
            else
            {
                OutputError(string.Format(Properties.Resources.NotFoundSpecifyProfile, name), isCatchErrors);
                return false;
            }
        }

        internal bool SystemPathSave(string path)
        {
            if (Directory.Exists(path.TrimEnd(Path.DirectorySeparatorChar)) || path == "")
            {
                configText.SystemDirectory = path.TrimEnd(Path.DirectorySeparatorChar);
                return TryConfigSave();
            }
            else
                return false;
        }

        internal bool CommonPathSave(string path)
        {
            if (Directory.Exists(path.TrimEnd(Path.DirectorySeparatorChar)) || path == "")
            {
                configText.CommonDirectory = path.TrimEnd(Path.DirectorySeparatorChar);
                return TryConfigSave();
            }
            else
                return false;
        }

        internal bool TemplatesPathSave(string path)
        {
            if (Directory.Exists(path.TrimEnd(Path.DirectorySeparatorChar)) || path == "")
            {
                configText.TemplatesDirectory = path.TrimEnd(Path.DirectorySeparatorChar);
                return TryConfigSave();
            }
            else
                return false;
        }

        internal bool RuntimePathSave(string path)
        {
            if (Directory.Exists(path.TrimEnd(Path.DirectorySeparatorChar)) || path == "")
            {
                configText.RuntimeDirectory = path.TrimEnd(Path.DirectorySeparatorChar);
                return TryConfigSave();
            }
            else
                return false;
        }

        private bool TryConfigSave()
        {
            var jOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            var text = JsonSerializer.SerializeToUtf8Bytes(configText, jOptions);
            try
            {
                using var fs = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".hspcuiconfig.json"), FileMode.Create);
                fs.Write(text);
                fs.Flush();
                return true;
            }
            catch (Exception e)
            {
                OutputError(e.Message, isCatchErrors);
                return false;
            }
        }
    }


    internal class ConfigText
    {
        [JsonPropertyName("$schema")]
        public string Schema { get; set; } = "https://raw.githubusercontent.com/Asugakoisi/HSPComanndLine/main/src/HSPCommandLine/HSPCommandLine/json/hspcuiconfig-schema.json";
        [JsonPropertyName("ver")]
        public string Version { get; set; }
        [JsonPropertyName("common")]
        public string CommonDirectory { get; set; } = string.Empty;
        [JsonPropertyName("runtime")]
        public string RuntimeDirectory { get; set; } = string.Empty;
        [JsonPropertyName("system")]
        public string SystemDirectory { get; set; } = string.Empty;
        [JsonPropertyName("templates")]
        public string TemplatesDirectory { get; set; } = string.Empty;
        [JsonPropertyName("profiles")]
        public Profile[] Profiles { get; set; } = Array.Empty<Profile>();
    }

    internal class Profile
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("options")]
        public string[] Options { get; set; }
    }
}
