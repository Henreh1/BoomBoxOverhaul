using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.ComponentModel.Design;
using System.IO;
using Unity.Netcode;
using UnityEngine;

namespace BoomBoxOverhaul
{
    public enum AudioModeType
    {
        Realistic,
        Balanced,
        PureMusic,
    }

    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "henreh.boomboxoverhaul";
        public const string ModName = "BoomBoxOverhaulV2";
        public const string ModVersion = "2.1.0";

        internal static Plugin Instance;
        internal static Harmony Harmony;

        internal static ConfigEntry<bool> InfiniteBattery;
        internal static ConfigEntry<bool> WeightlessBoombox;
        internal static ConfigEntry<bool> KeepPlayingPocketed;
        internal static ConfigEntry<float> VolumeStep;
        internal static ConfigEntry<float> DefaultVolume;
        internal static ConfigEntry<AudioModeType> AudioMode;
        internal static ConfigEntry<KeyCode> OpenUiKey;
        internal static ConfigEntry<KeyCode> VolumeUpKey;
        internal static ConfigEntry<KeyCode> VolumeDownKey;
        internal static ConfigEntry<int> MaxCacheFiles;
        internal static ConfigEntry<float> ReadyTimeoutSeconds;
        internal static ConfigEntry<bool> AutoplayPlaylist;
        internal static ConfigEntry<bool> ShufflePlaylist;
        internal static ConfigEntry<int> MaxTrackSeconds;
        internal static ConfigEntry<bool> DeleteCacheOnBoot;
        internal static ConfigEntry<bool> LocalVolumeOnly;

        internal static ConfigEntry<bool> AutoDownloadYtDlp;
        internal static ConfigEntry<bool> AutoDownloadFfmpeg;
        internal static ConfigEntry<string> FfmpegZipUrl;
        internal static ConfigEntry<bool> SearchPathForTools;
        internal static ConfigEntry<bool> DbgLogs;

        internal static bool SyncedLocalVolumeOnly = true;
        internal static bool HasSyncedVolumeMode = false;
        internal static bool SyncedWeightlessBoombox = true;
        internal static bool HasSyncedWeightlessBoombox = false;

        internal static bool HostHasBoomBoxOverhaul = false;
        internal static bool HasRecievedHostHandshake = false;

        internal static string PluginFolder = "";
        internal static string CacheFolder = "";
        internal static string ToolsFolder = "";

        internal static AudioClip SilentSFXClip;

        private void Awake()
        {
            Instance = this;

            InfiniteBattery = Config.Bind("Gameplay", "InfiniteBattery", true, "Boombox does not require battery.");
            WeightlessBoombox = Config.Bind("Gameplay", "WeightlessBoombox", true, "When set to true the boombox is as light as a feather!");
            KeepPlayingPocketed = Config.Bind("Gameplay", "KeepPlayingPocketed", true, "Boombox keeps playing when pocketed.");
            VolumeStep = Config.Bind("Audio", "VolumeStep", 0.1f, "Volume increment/decrement amount.");
            DefaultVolume = Config.Bind("Audio", "DefaultVolume", 1.0f, "Default boombox volume.");
            OpenUiKey = Config.Bind("Input", "OpenUiKey", KeyCode.B, "Open URL input UI.");
            VolumeUpKey = Config.Bind("Input", "VolumeUpKey", KeyCode.Equals, "Increase boombox volume.");
            VolumeDownKey = Config.Bind("Input", "VolumeDownKey", KeyCode.Minus, "Decrease boombox volume.");
            AudioMode = Config.Bind(
                "Audio",
                "AudioMode",
                AudioModeType.Balanced,
                "Boombox audio mixer:\nRealistic = Full 3D\nBalanced =Mixed\nPure Music! = Crystal clear audio"
            );
            MaxCacheFiles = Config.Bind("Cache", "MaxCacheFiles", 15, "Maximum amount of downloaded tracks to keep.");
            ReadyTimeoutSeconds = Config.Bind("Networking", "ReadyTimeoutSeconds", 20f, "How long the server waits for clients to prepare before starting anyway.");
            AutoplayPlaylist = Config.Bind("Playlist", "AutoplayPlaylist", true, "Automatically continue to next playlist track.");
            ShufflePlaylist = Config.Bind("Playlist", "ShufflePlaylist", false, "Shuffle playlist order after resolving entries.");
            MaxTrackSeconds = Config.Bind("Downloads", "MaxTrackSeconds", 1800, "Maximum allowed track duration in seconds.");
            DeleteCacheOnBoot = Config.Bind("Cache", "DeleteCacheOnBoot", true, "Clear cache when the plugin loads.");
            LocalVolumeOnly = Config.Bind("Audio", "LocalVolumeOnly", false, "If true, volume changes are local only. If false, boombox volume is shared server-wide.");

            AutoDownloadYtDlp = Config.Bind("Dependencies", "AutoDownloadYtDlp", true, "Automatically download yt-dlp if it is missing.");
            AutoDownloadFfmpeg = Config.Bind("Dependencies", "AutoDownloadFfmpeg", true, "Automatically download ffmpeg if it is missing.");
            FfmpegZipUrl = Config.Bind("Dependencies", "FfmpegZipUrl", "https://github.com/yt-dlp/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip", "FFmpeg ZIP URL.");
            SearchPathForTools = Config.Bind("Dependencies", "SearchPathForTools", true, "Search PATH for yt-dlp and ffmpeg.");
            DbgLogs = Config.Bind("Debug", "DbgLogs", false, "Enable debug logs.");

            PluginFolder = Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath;
            ToolsFolder = Path.Combine(PluginFolder, "tools");
            CacheFolder = Path.Combine(PluginFolder, "cache");

            Directory.CreateDirectory(ToolsFolder);
            Directory.CreateDirectory(CacheFolder);

            if (DeleteCacheOnBoot.Value)
            {
                FileSystemHelpers.TryDeleteDirectoryContents(CacheFolder);
            }

            if (SilentSFXClip == null)
            {
                SilentSFXClip = AudioClip.Create("BoomBoxOverhaul_SilentSFX", 1, 1, 44100, false);
                SilentSFXClip.SetData(new float[] { 0f }, 0); //Silent sfx for boombox equip events etc a very very caveman way to prevcnt playoneshot audio clip null
            }

            Harmony = new Harmony(ModGuid);
            Harmony.PatchAll();

            Logger.LogInfo(ModName + " " + ModVersion + " loaded.");

            DependencyBootstrapper.EnsureStarted(this);
            Logger.LogInfo("Dependency bootstrap started.");

            GameObject netBoot = new GameObject("BoomBoxOverhaulNetBoot");
            netBoot.hideFlags = HideFlags.HideAndDontSave;
            netBoot.AddComponent<BoomBoxOverhaulNetBoot>();
            DontDestroyOnLoad(netBoot);

            Logger.LogInfo("BoomBoxOverhaul network boot started.");
        }

        //Network checkers or something

        internal static bool CanUseHostForcedGameplayFeatures()
        {
            if (Unity.Netcode.NetworkManager.Singleton == null)
            {
                return true;
            }

            if (Unity.Netcode.NetworkManager.Singleton.IsServer)
            {
                return true;
            }

            return HasRecievedHostHandshake && HostHasBoomBoxOverhaul;
        }



        internal static bool UseInfiniteBattery()
        {
            if (!CanUseHostForcedGameplayFeatures())
            {
                return false;
            }

            return InfiniteBattery.Value;
        }

        internal static bool UseKeepPlayingPocketed()
        {
            if (!CanUseHostForcedGameplayFeatures())
            {
                return false;
            }
            return KeepPlayingPocketed.Value;
        }


        internal static bool UseWeightlessBoombox()
        {
            if (!CanUseHostForcedGameplayFeatures())
            {
                return false;
            }
            
        if (Unity.Netcode.NetworkManager.Singleton != null
        && Unity.Netcode.NetworkManager.Singleton.IsClient
        && HasSyncedWeightlessBoombox)
        {
        return SyncedWeightlessBoombox;
            }
            return WeightlessBoombox.Value;
        }

        internal static bool UseLocalVolumeOnly()
        {
            if (Unity.Netcode.NetworkManager.Singleton != null
                && Unity.Netcode.NetworkManager.Singleton.IsClient
                && HasSyncedVolumeMode)
            {
                return SyncedLocalVolumeOnly;
            }

            return LocalVolumeOnly.Value;
        }


        internal static AudioModeType UseAudioMode()
        {
            return AudioMode.Value;
        }

        //Debug logs option for my own sanity

        internal static void DbgLog(string msg)
        {
            if (DbgLogs != null && DbgLogs.Value)
            {
                Log("[DBG] " + msg);
            }
        }
        internal static void Log(string msg)
        {
            if (Instance != null)
            {
                Instance.Logger.LogInfo(msg);
            }
            else
            {
                BepInEx.Logging.Logger.CreateLogSource("BoomBoxOverhaul")
                    .LogInfo(msg);
            }
        }

        internal static void Warn(string msg)
        {
            if (Instance != null)
            {
                Instance.Logger.LogWarning(msg);
            }
            else
            {
                BepInEx.Logging.Logger.CreateLogSource("BoomBoxOverhaul")
                    .LogWarning(msg);
            }
        }

        internal static void Error(string msg)
        {
            if (Instance != null)
            {
                Instance.Logger.LogError(msg);
            }
            else
            {
                BepInEx.Logging.Logger.CreateLogSource("BoomBoxOverhaul")
                    .LogError(msg);
            }
        }
    }
}