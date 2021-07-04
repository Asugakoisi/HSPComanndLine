using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HspcuiBase.Utils
{
    public static class Util
    {
        public const int MAX_PATH = 260;

        internal static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IEnumerable<KeyValuePair<TKey, TValue>> addPairs)
        {
            foreach (var kv in addPairs)
            {
                //if (source.ContainsKey(kv.Key))
                //    source[kv.Key] = kv.Value;
                //else
                    source.Add(kv);
            }
        }

        internal static void AddRange<TKey, TValue>(this IDictionary<TKey, List<TValue>> source, IEnumerable<KeyValuePair<TKey, TValue>> addPairs)
        {
            foreach (var kv in addPairs)
            {
                if (source.ContainsKey(kv.Key))
                    source[kv.Key].Add(kv.Value);
                else
                    source.Add(kv.Key, (new TValue[1] { kv.Value}).ToList());
            }
        }

        /// <summary>
        /// 指定されたディレクトリが存在するかどうか。使う際は try - catch すること。
        /// 
        /// </summary>
        /// <param name="checkDirectoryPath">指定するディレクトリ</param>
        /// <param name="sysDirectoryPath">HSPシステムフォルダパス</param>
        /// <param name="directoryName">ディレクトリ名</param>
        /// <returns>ディレクトリパス</returns>
        public static string CheckDirectory(string checkDirectoryPath, string sysDirectoryPath, string directoryName, bool skipcurrentdirectory = false)
        {
            if (!Directory.Exists(checkDirectoryPath.TrimEnd(Path.DirectorySeparatorChar)) || skipcurrentdirectory)
            {
#if net50
                var buffer = new Span<Char>(new string(' ', MAX_PATH).ToCharArray());
                if (!string.IsNullOrEmpty(sysDirectoryPath) && Path.TryJoin(sysDirectoryPath.AsSpan(), checkDirectoryPath.AsSpan(), buffer, out int nchars)
                    && Directory.Exists(buffer.Slice(0, nchars).ToString()))
                {
                    checkDirectoryPath = buffer.Slice(0, nchars).ToString();
#else
                var path = Path.Combine(sysDirectoryPath, checkDirectoryPath);
                if (!string.IsNullOrEmpty(sysDirectoryPath) && path.Length != 0 && Directory.Exists(path))
                {
                    checkDirectoryPath = path;
#endif
                    if (!checkDirectoryPath.EndsWith($"{Path.DirectorySeparatorChar}"))
                        checkDirectoryPath += Path.DirectorySeparatorChar;
                }
                else
                {
                    throw new Exception(string.Format(Properties.Resources.NotFoundSpecifyADirectory, directoryName, checkDirectoryPath));
                }
            }
            else if (!checkDirectoryPath.EndsWith($"{Path.DirectorySeparatorChar}"))
                checkDirectoryPath += Path.DirectorySeparatorChar;
            return checkDirectoryPath;
        }

        /// <summary>
        /// 指定されたファイルの文字コードがUtf8かshift_jisかどうかを判断します。
        /// </summary>
        /// <param name="filePath">調べるファイルのパス</param>
        /// <returns>utf-8(であれば 0、shift_jisなら 1、それ以外なら -1</returns>
        public static int CheckUTF8(string filePath)
        {
            int isUtf8 = 0;
            var stream = new StreamReader(filePath, new UTF8Encoding(true, true));
            try
            {
                _ = stream.ReadToEnd();
            }
            catch (DecoderFallbackException)
            {
                isUtf8 = 1;
            }
            finally
            {
                stream.Close();
            }
            if (isUtf8 == 1)
            {
                stream = new StreamReader(filePath, Encoding.GetEncoding("Shift_JIS", new EncoderExceptionFallback(), new DecoderExceptionFallback()));
                try
                {
                    _ = stream.ReadToEnd();
                }
                catch (DecoderFallbackException)
                {
                    isUtf8 = -1;
                }
                finally
                {
                    stream.Close();
                }
            }
            return isUtf8;
        }

        public static bool ConvertFileToUtf8(string filePath, bool catchErrors)
        {
            var reader = new StreamReader(filePath, Encoding.GetEncoding("Shift_JIS"));
            string fileContent = "";
            try
            {
                fileContent = reader.ReadToEnd();
            }
            catch
            {
                if (catchErrors)
                    throw;
                else
                    return false;
            }
            finally
            {
                reader.Close();
            }
            var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
            try
            {
                writer.Write(fileContent);
                writer.Flush();
            }
            catch
            {
                if (catchErrors)
                    throw;
                else
                    return false;
            }
            finally
            {
                writer.Close();
            }
            return true;
        }
    }
}
