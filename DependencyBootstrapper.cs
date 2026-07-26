using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Diagnostics;
using YoutubeDLSharp;

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

       //Following 753 advice and using dependancy :D

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
                if ( !string.IsNullOrEmpty(onPath))
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

                //Wrapper things (I think)
                YoutubeDLSharp.Utils.DownloadYtDlp(Plugin.ToolsFolder).GetAwaiter().GetResult();
                if (File.Exists(local))
                {
                    state.YtDlpPath = local;
                    status = "yt-dlp downloaded successfully. Hopefully :D";
                    Plugin.Log("yt-dlp ready at: " + local);
                }
                else
                {
                    status = "yt-dlp download completed but executable not found.";
                    Plugin.Warn(status);
                }
            }
            catch (Exception ex)
            { status = "Failed downloading yt-dlp: " + ex.Message;
                Plugin.Error(status);
            }
        }

        //FFmpeg stuff, also following 753 advice and using dependancy :D

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
                    status = "Usaing PATH ffmpeg.";
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

            try
            {
                status = "Downloading ffmpeg...";
                Plugin.Log(status);
                //Wrapper things
                YoutubeDLSharp.Utils.DownloadFFmpeg(Plugin.ToolsFolder).GetAwaiter().GetResult();

                if (File.Exists(localExe))
                {
                    state.FfmpegPath = localExe;
                    status = "ffmpeg downloaded successfully. Hopefully :D";
                    Plugin.Log("ffmpeg ready at: " + localExe);
                }
                else
                {
                    status = "ffmpeg download completed but executable not found.";
                    Plugin.Warn(status);
                }
            }
            catch (Exception ex)
            {
                status = "Failed downloading ffmpeg: " + ex.Message;
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