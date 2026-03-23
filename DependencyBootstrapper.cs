using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using UnityEngine;

namespace BoomBoxOverhaul
{
    internal sealed class DependencyState
    {
        public string YtDlpPath;
        public string FfmpegPath;

        public bool IsReady()
        {
            return !string.IsNullOrEmpty(YtDlpPath)
                && !string.IsNullOrEmpty(FfmpegPath)
                && File.Exists(YtDlpPath)
                && File.Exists(FfmpegPath);
        }
    }

    internal static class DependencyBootstrapper
    {
        private static readonly object Gate = new object();
        private static bool started;
        private static string status = "Dependency check not started.";
        private static DependencyState state = new DependencyState();

        public static string GetStatus()
        {
            return status;
        }

        public static DependencyState GetState()
        {
            return state;
        }

        public static void EnsureStarted(MonoBehaviour host)
        {
            lock (Gate)
            {
                if (started)
                {
                    Plugin.Log("Dependency bootstrap already started.");
                    return;
                }

                started = true;
                Plugin.Log("Dependency bootstrap starting.");
                host.StartCoroutine(BootstrapCoroutine());
            }
        }

        private static System.Collections.IEnumerator BootstrapCoroutine()
        {
            status = "Checking dependencies...";
            Plugin.Log(status);

            yield return RunBlockingTask(delegate
            {
                try
                {
                    ResolveYtDlp();
                    ResolveFfmpeg();

                    if (state.IsReady())
                    {
                        status = "Dependencies ready.";
                        Plugin.Log(status);
                    }
                    else
                    {
                        Plugin.Warn("Dependencies not fully ready. Status: " + status);
                    }
                }
                catch (Exception ex)
                {
                    status = "Dependency bootstrap failed: " + ex.Message;
                    Plugin.Error(status);
                    Plugin.Error(ex.ToString());
                }
            });
        }

        private static void ResolveYtDlp()
        {
            Plugin.Log("Checking for yt-dlp...");

            string local = Path.Combine(Plugin.ToolsFolder, "yt-dlp.exe");
            if (File.Exists(local))
            {
                state.YtDlpPath = local;
                status = "Using local yt-dlp.";
                Plugin.Log("yt-dlp found in tools folder: " + local);
                return;
            }

            if (Plugin.SearchPathForTools.Value)
            {
                string onPath = FileSystemHelpers.TryFindOnPath("yt-dlp.exe");
                if (!string.IsNullOrEmpty(onPath))
                {
                    state.YtDlpPath = onPath;
                    status = "Using PATH yt-dlp.";
                    Plugin.Log("yt-dlp found on PATH: " + onPath);
                    return;
                }
            }

            if (!Plugin.AutoDownloadYtDlp.Value)
            {
                status = "yt-dlp missing.";
                Plugin.Warn("yt-dlp not found and auto-download is disabled.");
                return;
            }

            try
            {
                status = "Downloading yt-dlp...";
                Plugin.Log(status);

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "BoomBoxOverhaul/2.0.0");
                    client.DownloadFile("https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe", local);
                }

                state.YtDlpPath = local;
                status = "yt-dlp downloaded successfully.";
                Plugin.Log("yt-dlp ready at: " + local);
            }
            catch (Exception ex)
            {
                status = "Failed downloading yt-dlp: " + ex.Message;
                Plugin.Error(status);
            }
        }

        private static void ResolveFfmpeg()
        {
            Plugin.Log("Checking for ffmpeg...");

            string localExe = Path.Combine(Plugin.ToolsFolder, "ffmpeg.exe");
            if (File.Exists(localExe))
            {
                state.FfmpegPath = localExe;
                status = "Using local ffmpeg.";
                Plugin.Log("ffmpeg found in tools folder: " + localExe);
                return;
            }

            if (Plugin.SearchPathForTools.Value)
            {
                string onPath = FileSystemHelpers.TryFindOnPath("ffmpeg.exe");
                if (!string.IsNullOrEmpty(onPath))
                {
                    state.FfmpegPath = onPath;
                    status = "Using PATH ffmpeg.";
                    Plugin.Log("ffmpeg found on PATH: " + onPath);
                    return;
                }
            }

            if (!Plugin.AutoDownloadFfmpeg.Value)
            {
                status = "ffmpeg missing.";
                Plugin.Warn("ffmpeg not found and auto-download is disabled.");
                return;
            }

            string zipUrl = Plugin.FfmpegZipUrl.Value == null ? "" : Plugin.FfmpegZipUrl.Value.Trim();

            if (string.IsNullOrEmpty(zipUrl))
            {
                zipUrl = "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip";
                Plugin.Warn("ffmpeg ZIP URL was empty, falling back to built-in default.");
            }

            try
            {
                status = "Downloading FFmpeg...";
                Plugin.Log(status);

                string zipPath = Path.Combine(Plugin.ToolsFolder, "ffmpeg_package.zip");
                string extractPath = Path.Combine(Plugin.ToolsFolder, "ffmpeg_extract");

                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }

                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, true);
                }

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "BoomBoxOverhaul/2.0.0");
                    client.DownloadFile(zipUrl, zipPath);
                }

                Plugin.Log("FFmpeg archive downloaded: " + zipPath);

                ZipFile.ExtractToDirectory(zipPath, extractPath);
                Plugin.Log("FFmpeg archive extracted.");

                string[] ffmpegCandidates = Directory.GetFiles(extractPath, "ffmpeg.exe", SearchOption.AllDirectories);
                string[] ffprobeCandidates = Directory.GetFiles(extractPath, "ffprobe.exe", SearchOption.AllDirectories);

                if (ffmpegCandidates.Length > 0)
                {
                    File.Copy(ffmpegCandidates[0], localExe, true);
                    state.FfmpegPath = localExe;
                    Plugin.Log("ffmpeg ready at: " + localExe);
                }
                else
                {
                    status = "ffmpeg.exe not found in archive.";
                    Plugin.Warn(status);
                    return;
                }

                if (ffprobeCandidates.Length > 0)
                {
                    string localProbe = Path.Combine(Plugin.ToolsFolder, "ffprobe.exe");
                    File.Copy(ffprobeCandidates[0], localProbe, true);
                    Plugin.Log("ffprobe ready at: " + localProbe);
                }

                status = "ffmpeg downloaded successfully.";
            }
            catch (Exception ex)
            {
                status = "Failed resolving ffmpeg: " + ex.Message;
                Plugin.Error(status);
            }
        }

        private static System.Collections.IEnumerator RunBlockingTask(Action action)
        {
            bool done = false;
            Exception caught = null;

            Thread thread = new Thread(delegate ()
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    caught = ex;
                }
                finally
                {
                    done = true;
                }
            });

            thread.IsBackground = true;
            thread.Start();

            while (!done)
            {
                yield return null;
            }

            if (caught != null)
            {
                Plugin.Error("Dependency worker thread failed: " + caught);
            }
        }
    }
}