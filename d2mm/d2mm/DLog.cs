using System;
using System.IO;
using System.Text;

namespace de.sebastianrutofski.d2mm
{
    internal static class DLog
    {
        private static StringBuilder stb = new StringBuilder();

        public static void Log(string message, LogType type = LogType.Verbose)
        {
            stb.AppendFormat("[{0}]\t\t{1}{2}", type.ToString().ToUpperInvariant(), message, Environment.NewLine);
        }

        public enum LogType
        {
            Verbose,
            Error,
            Debug
        }

        public static void Log(Exception exception)
        {
            Log(exception.Message, LogType.Error);
        }

        public static void WriteLog(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
                using (StreamWriter writer = new StreamWriter(File.Open(file, FileMode.CreateNew, FileAccess.Write))
                    )
                {
                    writer.Write(stb.ToString());
                    writer.Flush();
                }
            }
            catch
            {
                    
            }
        }
    }
}