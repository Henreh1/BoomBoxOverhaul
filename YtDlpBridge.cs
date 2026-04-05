using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BoomBoxOverhaul //Tried to use wrapper to handle ytdlp stuff but could not solve log spam, so reverting to manual method until I look into it more, sorry 753 :(
{
    internal static class YtDlpBridge
    {
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

                string args = "--flat-playlist --print \"%(id)s\" \"" + EscapeArg(url) + "\"";
                string stdout;
                string stderr;

                if (!RunProcess(dep.YtDlpPath, args, out stdout, out stderr, 120000))
                {
                    Plugin.Warn("yt-dlp playlist resolve failed: " + stderr);
                    return false;
                }

                using (StringReader reader = new StringReader(stdout))
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        line = line.Trim();
                        if (line.Length > 0)
                        {
                            ids.Add(line);
                        }
                    }
                }

                if (Plugin.ShufflePlaylist.Value && ids.Count > 1)
                {
                    Random rng = new Random();
                    int i;
                    for (i = ids.Count - 1; i > 0; i--)
                    {
                        int j = rng.Next(i + 1);
                        string tmp = ids[i];
                        ids[i] = ids[j];
                        ids[j] = tmp;
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

                string tempBase = CacheManager.BuildTrackBasePath(videoId) + "_tmp";
                string tempAudio = tempBase + ".mp3";

                if (File.Exists(tempAudio))
                {
                    try
                    {
                        File.Delete(tempAudio);
                    }
                    catch
                    {
                    }
                }

                string metadataArgs = "--no-playlist --print \"%(title)s|||%(duration)s|||%(id)s\" \"" + EscapeArg(sourceUrl) + "\"";
                string metaStdout;
                string metaStderr;

                if (RunProcess(dep.YtDlpPath, metadataArgs, out metaStdout, out metaStderr, 120000))
                {
                    string[] parts = metaStdout.Trim().Split(new string[] { "|||" }, StringSplitOptions.None);
                    if (parts.Length >= 3)
                    {
                        info.title = parts[0];
                        float duration;
                        if (float.TryParse(parts[1], out duration))
                        {
                            info.durationSeconds = duration;
                        }
                    }
                }

                if (Plugin.MaxTrackSeconds.Value > 0 && info.durationSeconds > Plugin.MaxTrackSeconds.Value)
                {
                    return false;
                }

                string ffmpegFolder = Path.GetDirectoryName(dep.FfmpegPath) ?? Plugin.ToolsFolder;
                string downloadArgs =
                    "--no-playlist " +
                    "--extract-audio --audio-format mp3 --audio-quality 0 " +
                    "--ffmpeg-location \"" + EscapeArg(ffmpegFolder) + "\" " +
                    "--output \"" + EscapeArg(tempBase) + ".%(ext)s\" " +
                    "\"" + EscapeArg(sourceUrl) + "\"";

                string dlStdout;
                string dlStderr;

                if (!RunProcess(dep.YtDlpPath, downloadArgs, out dlStdout, out dlStderr, 900000))
                {
                    Plugin.Warn("Download failed: " + dlStderr);
                    return false;
                }

                if (!File.Exists(tempAudio))
                {
                    return false;
                }

                if (File.Exists(info.cachePath))
                {
                    File.Delete(info.cachePath);
                }

                File.Move(tempAudio, info.cachePath);
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

        private static bool RunProcess(string exePath, string args, out string stdout, out string stderr, int timeoutMs)
        {
            stdout = "";
            stderr = "";

            try
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = args,
                        WorkingDirectory = Plugin.ToolsFolder,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };

                    proc.Start();
                    stdout = proc.StandardOutput.ReadToEnd();
                    stderr = proc.StandardError.ReadToEnd();

                    if (!proc.WaitForExit(timeoutMs))
                    {
                        try
                        {
                            proc.Kill();
                        }
                        catch
                        {
                        }

                        stderr += "\nProcess timed out.";
                        return false;
                    }

                    return proc.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                stderr = ex.ToString();
                return false;
            }
        }

        private static string EscapeArg(string value)
        {
            return value.Replace("\"", "\\\"");
        }
    }
}