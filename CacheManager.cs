using System;
using System.Collections.Generic;
using System.IO;

namespace BoomBoxOverhaul
{
    internal static class CacheManager
    {
        public static string BuildTrackBasePath(string videoId)
        {
            return Path.Combine(Plugin.CacheFolder, videoId);
        }

        public static string BuildAudioPath(string videoId)
        {
            return BuildTrackBasePath(videoId) + ".mp3";
        }

        public static string BuildMetaPath(string videoId)
        {
            return BuildTrackBasePath(videoId) + ".txt";
        }

        public static bool HasTrack(string videoId)
        {
            return File.Exists(BuildAudioPath(videoId));
        }

        public static void Touch(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
                }
            }
            catch
            {
            }
        }

        public static void WriteMeta(string videoId, string title)
        {
            try
            {
                File.WriteAllText(BuildMetaPath(videoId), title ?? "");
            }
            catch
            {
            }
        }

        public static string ReadMeta(string videoId)
        {
            try
            {
                string p = BuildMetaPath(videoId);
                if (File.Exists(p))
                {
                    return File.ReadAllText(p);
                }
            }
            catch
            {
            }

            return "";
        }

        public static void Prune()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Plugin.CacheFolder);
                FileInfo[] rawFiles = dir.GetFiles("*.mp3");
                List<FileInfo> files = new List<FileInfo>(rawFiles);
                files.Sort(delegate (FileInfo a, FileInfo b)
                {
                    return b.LastWriteTimeUtc.CompareTo(a.LastWriteTimeUtc);
                });

                int i;
                for (i = Plugin.MaxCacheFiles.Value; i < files.Count; i++)
                {
                    try
                    {
                        string audioPath = files[i].FullName;
                        string basePath = Path.Combine(files[i].DirectoryName ?? Plugin.CacheFolder, Path.GetFileNameWithoutExtension(audioPath));
                        File.Delete(audioPath);

                        string metaPath = basePath + ".txt";
                        if (File.Exists(metaPath))
                        {
                            File.Delete(metaPath);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Warn("Cache prune failed: " + ex);
            }
        }
    }
}