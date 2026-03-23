using System;
using System.Text.RegularExpressions;

namespace BoomBoxOverhaul
{
    internal static class UrlHelpers
    {
        private static readonly Regex WatchRegex = new Regex(@"(?:youtube\.com\/watch\?[^#\s]*\bv=)([A-Za-z0-9_-]{6,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ShortRegex = new Regex(@"(?:youtu\.be\/)([A-Za-z0-9_-]{6,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex PlaylistRegex = new Regex(@"(?:[?&]list=)([A-Za-z0-9_-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static bool IsLikelyYoutubeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            return url.IndexOf("youtube.com/", StringComparison.OrdinalIgnoreCase) >= 0
                || url.IndexOf("youtu.be/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string TryExtractVideoId(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            Match watch = WatchRegex.Match(url);
            if (watch.Success)
            {
                return watch.Groups[1].Value;
            }

            Match shortUrl = ShortRegex.Match(url);
            if (shortUrl.Success)
            {
                return shortUrl.Groups[1].Value;
            }

            return null;
        }

        public static string TryExtractPlaylistId(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            Match playlist = PlaylistRegex.Match(url);
            if (playlist.Success)
            {
                return playlist.Groups[1].Value;
            }

            return null;
        }

        public static bool IsPlaylistOnlyUrl(string url)
        {
            string playlist = TryExtractPlaylistId(url);
            string video = TryExtractVideoId(url);
            return !string.IsNullOrEmpty(playlist) && string.IsNullOrEmpty(video);
        }
    }
}