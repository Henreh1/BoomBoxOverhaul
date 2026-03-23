using System;
using System.Collections.Generic;

namespace BoomBoxOverhaul
{
    [Serializable]
    internal sealed class TrackInfo
    {
        public string videoId = "";
        public string title = "";
        public string sourceUrl = "";
        public string cachePath = "";
        public float durationSeconds = 0f;
    }

    internal sealed class PlaylistState
    {
        public readonly List<string> VideoIds = new List<string>();
        public int Index = 0;

        public bool HasCurrent()
        {
            return Index >= 0 && Index < VideoIds.Count;
        }

        public string GetCurrentId()
        {
            if (!HasCurrent())
            {
                return null;
            }

            return VideoIds[Index];
        }

        public string Advance()
        {
            Index++;
            return GetCurrentId();
        }

        public void Shuffle(Random rng)
        {
            int i;
            for (i = VideoIds.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                string tmp = VideoIds[i];
                VideoIds[i] = VideoIds[j];
                VideoIds[j] = tmp;
            }
        }
    }
}