﻿using SongCore;
using System.Linq;

namespace CustomCampaigns.Utils
{
    public static class BeatmapUtils
    {
        public static IDifficultyBeatmap GetMatchingBeatmapDifficulty(string songId, BeatmapCharacteristicSO beatmapCharacteristic, BeatmapDifficulty beatmapDifficulty)
        {
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(songId);
            if (beatmapLevel == null)
            {
                Plugin.logger.Debug($"null beatmaplevel - {songId}");
                return null;
            }

            var levelDifficultyBeatmapSets = beatmapLevel.beatmapLevelData.difficultyBeatmapSets;

            foreach (var difficultyBeatmap in levelDifficultyBeatmapSets)
            {
                if (difficultyBeatmap.beatmapCharacteristic.Equals(beatmapCharacteristic))
                {
                    var levelDifficultyBeatmaps = difficultyBeatmap.difficultyBeatmaps;
                    foreach (var diff in levelDifficultyBeatmaps)
                    {
                        if (diff.difficulty.Equals(beatmapDifficulty))
                        {
                            return diff;
                        }
                    }
                }
            }
            
            return null;
        }
    }
}
