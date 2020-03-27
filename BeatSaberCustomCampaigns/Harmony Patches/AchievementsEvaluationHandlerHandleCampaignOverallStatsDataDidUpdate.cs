using HarmonyLib;
using SongCore;
using System;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(AchievementsEvaluationHandler), "HandleCampaignOverallStatsDataDidUpdate",
        new Type[] { typeof(MissionCompletionResults), typeof(MissionNode) })]
    class AchievementsEvaluationHandlerHandleCampaignOverallStatsDataDidUpdate
    {
        static bool Prefix(MissionCompletionResults missionCompletionResults, MissionNode missionNode, AchievementsEvaluationHandler __instance)
        {
            if (missionNode.missionData is CustomMissionDataSO)
            {
                __instance.ProcessMissionFinishData(missionNode, missionCompletionResults);
                IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded((missionNode.missionData as CustomMissionDataSO).customLevel.levelID).beatmapLevelData, missionNode.missionData.beatmapCharacteristic, missionNode.missionData.beatmapDifficulty);
                __instance.ProcessLevelFinishData(difficultyBeatmap, missionCompletionResults.levelCompletionResults);
                return false;
            }
            return true;
        }
    }
}
