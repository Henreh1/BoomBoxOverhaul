using System;
using System.IO;

namespace BoomBoxOverhaul
{
    internal static class FileSystemHelpers
    {
        public static void TryDeleteDirectoryContents(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    return;
                }

                string[] files = Directory.GetFiles(path);
                int i;
                for (i = 0; i < files.Length; i++)
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Error("Failed clearing directory contents for '" + path + "': " + ex);
            }
        }

        public static string TryFindOnPath(string exeName)
        {
            try
            {
                string path = Environment.GetEnvironmentVariable("PATH");
                if (string.IsNullOrEmpty(path))
                {
                    return null;
                }

                string[] folders = path.Split(Path.PathSeparator);
                int i;
                for (i = 0; i < folders.Length; i++)
                {
                    try
                    {
                        string folder = folders[i];
                        if (string.IsNullOrEmpty(folder))
                        {
                            continue;
                        }

                        string candidate = Path.Combine(folder.Trim(), exeName);
                        if (File.Exists(candidate))
                        {
                            return candidate;
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }

            return null;
        }
    }
}