using System;
using System.IO;
using System.Linq;

namespace Serpent_Theater_Server.Utils
{
    public enum Verbosity
    {
        Information,
        Warning,
        Error
    }
    public static class BasicLogger
    {
        private static bool _inUse;
        private static int _logFileNumber = 1;
        public static void Log(string message)
        {
            if (!_inUse)
            {
                var directory = AppDomain.CurrentDomain.BaseDirectory + @"\Logs";
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                else
                {
                    var files = Directory.GetFileSystemEntries(directory).ToList();
                    if (files.Count != 0)
                    {
                        var obtainedfile = files.LastOrDefault(
                        x => x.Contains("Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_"));
                        if (!(String.IsNullOrEmpty(obtainedfile) || String.IsNullOrWhiteSpace(obtainedfile)))
                        {
                            _logFileNumber =
                                Convert.ToInt32(obtainedfile.Substring(obtainedfile.LastIndexOf("_", StringComparison.Ordinal) + 1,
                                    obtainedfile.IndexOf(".txt", StringComparison.Ordinal) - (obtainedfile.LastIndexOf("_", StringComparison.Ordinal) + 1)));
                            _logFileNumber++;
                        }
                    }
                }
                _inUse = true;
                using (var file = new StreamWriter(@".\Logs\Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + _logFileNumber + ".txt"))
                {
                    Console.WriteLine(DateTime.Now + @": " + message);
                    file.WriteLine(DateTime.Now + ": " + message);
                }
            }
            else
            {
                using (var file = File.AppendText(@".\Logs\Log_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + _logFileNumber + ".txt"))
                {
                    Console.WriteLine(DateTime.Now + @": " + message);
                    file.WriteLine(DateTime.Now + ": " + message);
                }
            }
        }
    }
}
