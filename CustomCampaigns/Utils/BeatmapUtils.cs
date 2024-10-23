using SongCore;
using System.Linq;

namespace CustomCampaigns.Utils
{
    public static class BeatmapUtils
    {
        public static BeatmapKey GetMatchingBeatmapKey(string songId, BeatmapCharacteristicSO beatmapCharacteristic, BeatmapDifficulty beatmapDifficulty)
        {
            BeatmapKey beatmapKey = new BeatmapKey(songId, beatmapCharacteristic, beatmapDifficulty);
            return beatmapKey;
        }
    }
}
