using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace BoomBoxOverhaul
{
    internal static class YtDlpBridge
    {

        //Set wrapper paths or somn

        private static YoutubeDL CreateClient(DependencyState dep)
        {
            YoutubeDL ytdl = new YoutubeDL();
            ytdl.YoutubeDLPath = dep.YtDlpPath;
            ytdl.FFmpegPath = dep.FfmpegPath;
            ytdl.OutputFolder = Plugin.CacheFolder;
            return ytdl;
        }


        public static bool ResolvePlaylistIds(string url, out List<string> ids)
        {
            ids = new List<string>();
            try
            {
                DependencyState dep = DependencyBootstrapper.GetState();
                if (!dep.IsReady() || string.IsNullOrEmpty(dep.YtDlpPath))
                {
                    return false;
                }

                YoutubeDL ytdl = CreateClient(dep);
                var result = ytdl.RunVideoDataFetch(url).GetAwaiter().GetResult();

                if (!result.Success || result.Data == null)
                {
                    Plugin.Warn("yt-dlp playlist feth failed");
                    return false;
                }

                VideoData data = result.Data;

                if (data.Entries != null)
                {
                    int i;
                    for (i = 0; i < data.Entries.Length; i++)
                    {
                        VideoData entry = data.Entries[i];
                        if (entry != null && !string.IsNullOrEmpty(entry.ID))
                        {
                            ids.Add(entry.ID);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(data.ID))
                {
                    ids.Add(data.ID);
                }

                if (Plugin.ShufflePlaylist.Value && ids.Count > 1)
                {
                    Random rng = new Random();
                    int i;
                    for (i = ids.Count - 1; i > 0; i--)
                    {
                        int j = rng.Next(i + 1);
                        string temp = ids[i];
                        ids[i] = ids[j];
                        ids[j] = temp;
                    }
                }
                return ids.Count > 0;
            }
            catch (Exception ex)
            {
                Plugin.Error("ResolvePlaylistIds exception: " + ex);
                return false;
            }
        }

        //Feth track using wrapper because why not

        public static bool FetchTrack(string sourceUrl, string videoId, out TrackInfo info)
        {
            info = new TrackInfo();
            info.videoId = videoId;
            info.sourceUrl = sourceUrl;
            info.cachePath = CacheManager.BuildAudioPath(videoId);

            try
            {
                DependencyState dep = DependencyBootstrapper.GetState();
                if (!dep.IsReady() || string.IsNullOrEmpty(dep.YtDlpPath) || string.IsNullOrEmpty(dep.FfmpegPath))
                {
                    return false;
                }

                if (File.Exists(info.cachePath))
                {
                    CacheManager.Touch(info.cachePath);
                    info.title = CacheManager.ReadMeta(videoId);
                    return true;
                }

                YoutubeDL ytdl = CreateClient(dep);

                var metaResult = ytdl.RunVideoDataFetch(sourceUrl).GetAwaiter().GetResult();
                if (metaResult.Success && metaResult.Data != null)
                {
                    VideoData data = metaResult.Data;
                    info.title = data.Title;

                    if (data.Duration.HasValue)
                    {
                        info.durationSeconds = (float)data.Duration.Value;
                    }
                }

                if (Plugin.MaxTrackSeconds.Value > 0 && info.durationSeconds > Plugin.MaxTrackSeconds.Value)
                {
                    Plugin.Warn("Track is too long, skipping.");
                    return false;
                }

                var audioResult = ytdl.RunAudioDownload(sourceUrl, AudioConversionFormat.Mp3).GetAwaiter().GetResult();
                if (!audioResult.Success || string.IsNullOrEmpty(audioResult.Data) || !File.Exists(audioResult.Data))
                {
                    Plugin.Warn("Download Failed, sorry :(");
                    return false;
                }

                if (File.Exists(info.cachePath))
                {
                    File.Delete(info.cachePath);
                }

                File.Move(audioResult.Data, info.cachePath);
                CacheManager.Touch(info.cachePath);
                CacheManager.WriteMeta(videoId, info.title);
                CacheManager.Prune();
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Error("FetchTrack exception: " + ex);
                return false;
            }
        }
    }
}
        //Old method for running yt-dlp directly
    //    private static bool RunProcess(string exePath, string args, out string stdout, out string stderr, int timeoutMs)
    //    {
    //        stdout = "";
    //        stderr = "";

    //        try
    //        {
    //            using (Process proc = new Process())
    //            {
    //                proc.StartInfo = new ProcessStartInfo
    //                {
    //                    FileName = exePath,
    //                    Arguments = args,
    //                    WorkingDirectory = Plugin.ToolsFolder,
    //                    UseShellExecute = false,
    //                    RedirectStandardOutput = true,
    //                    RedirectStandardError = true,
    //                    CreateNoWindow = true,
    //                    StandardOutputEncoding = Encoding.UTF8,
    //                    StandardErrorEncoding = Encoding.UTF8
    //                };

    //                proc.Start();
    //                stdout = proc.StandardOutput.ReadToEnd();
    //                stderr = proc.StandardError.ReadToEnd();

    //                if (!proc.WaitForExit(timeoutMs))
    //                {
    //                    try
    //                    {
    //                        proc.Kill();
    //                    }
    //                    catch
    //                    {
    //                    }

    //                    stderr += "\nProcess timed out.";
    //                    return false;
    //                }

    //                return proc.ExitCode == 0;
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            stderr = ex.ToString();
    //            return false;
    //        }
    //    }

    //    private static string EscapeArg(string value)
    //    {
    //        return value.Replace("\"", "\\\"");
    //    }
    //}
