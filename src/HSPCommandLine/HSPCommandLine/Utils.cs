using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HSPCommandLine.Utils
{
    internal static class Util
    {
        internal const int MAX_PATH = 260;

        /// <summary>
        /// 指定されたディレクトリが存在するかどうか。
        /// </summary>
        /// <param name="checkDirectoryPath">指定するディレクトリ</param>
        /// <param name="sysDirectoryPath">HSPシステムフォルダパス</param>
        /// <param name="directoryName">ディレクトリ名</param>
        /// <returns>ディレクトリパス</returns>
        internal static string CheckDirectory(string checkDirectoryPath, string sysDirectoryPath, string directoryName, bool skipcurrentdirectory = false, bool catchError = true)
        {
            try
            {
                return HspcuiBase.Utils.Util.CheckDirectory(checkDirectoryPath, sysDirectoryPath, directoryName, skipcurrentdirectory);
            }
            catch(Exception ex)
            {
                if (catchError) throw;
                else
                {
                    Console.WriteLine(ex.Message);
                    return string.Empty;
                }
            }
        }

        internal static void OutputError(string errorMessage, bool isCatchErrors)
        {
            if (isCatchErrors)
                throw new Exception(errorMessage);
            else
                Console.WriteLine(errorMessage);
        }

        internal static void OutputError(Exception exception, bool isCatchErrors)
        {
            if (isCatchErrors)
                throw exception;
            else
                Console.WriteLine(exception.Message);
        }

        internal static bool CheckTextVersion(Version asmVersion, string ver)
        {
            var matches = Regex.Matches(ver, "([0-9]+).([0-9]+).([0-9]+).([0-9]+)");
            if (matches.Count == 2)
            {
                var lowestVersion = new Version(matches[0].ToString());
                var latestVersion = new Version(matches[1].ToString());
                if (asmVersion >= lowestVersion && asmVersion <= latestVersion)
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }
}
