using System.IO;

namespace de.sebastianrutofski.d2mm
{
    internal static class Helpers
    {
        public static long GetDirectorySize(string dir)
        {
            long size = 0;

            foreach (string file in Directory.GetFiles(dir))
            {
                size += new FileInfo(file).Length;
            }

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                size += GetDirectorySize(subDir);
            }

            return size;
        }
    }
}