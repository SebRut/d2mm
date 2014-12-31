using System;
using System.IO;
using System.Linq;

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
        public static void CopyDirectoryToDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    try
                    {
                        DLog.Log(
                            string.Format("Copying file {0} to {1}", file, Path.Combine(destDir, Path.GetFileName(file))),
                            DLog.LogType.Debug);
                        File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), true);
                    }
                    catch (Exception ex)
                    {
                        DLog.Log(ex);
                    }
                }
            }

            foreach (string directory in Directory.GetDirectories(sourceDir))
            {
                CopyDirectoryToDirectory(directory, Path.Combine(destDir, Path.GetFileName(Path.GetDirectoryName(directory + "\\"))));
            }
        }

        public static void DeleteDirectoryFromDirectory(string removableDir, string cleanableDir)
        {
            DLog.Log("Deleting dir...");
            try
            {
                foreach (string file in Directory.GetFiles(removableDir))
                {
                    if (!Path.GetInvalidPathChars().Any(invalidFileNameChar => file.Contains(invalidFileNameChar)) &
                        File.Exists(Path.Combine(cleanableDir, Path.GetFileName(file))))
                        DLog.Log("Deleting file: " + Path.Combine(cleanableDir, Path.GetFileName(file)), DLog.LogType.Debug);
                    File.Delete(Path.Combine(cleanableDir, Path.GetFileName(file)));
                }

                if (!Directory.GetDirectories(cleanableDir).Any() && !Directory.GetFiles(cleanableDir).Any())
                {
                    DLog.Log("Deleting dir: " + cleanableDir, DLog.LogType.Debug);
                    Directory.Delete(cleanableDir);
                }

            }
            catch (Exception ex)
            {
                DLog.Log(ex);
            }

            if (!Directory.Exists(removableDir))
                return;

            foreach (string directory in Directory.GetDirectories(removableDir))
            {
                if (!Path.GetInvalidPathChars().Any(c => directory.Contains(c)))
                {
                    if (directory != null)
                        DeleteDirectoryFromDirectory(directory,
                            Path.Combine(cleanableDir,
                                Path.GetFileName(Path.GetDirectoryName(string.Format("{0}{1}", directory, Path.DirectorySeparatorChar)))));
                }
            }
        }

        public static void MoveDirectoryToDirectory(string src, string dest)
        {
            CopyDirectoryToDirectory(src, dest);
            Directory.Delete(src, true);
        }
    }

}