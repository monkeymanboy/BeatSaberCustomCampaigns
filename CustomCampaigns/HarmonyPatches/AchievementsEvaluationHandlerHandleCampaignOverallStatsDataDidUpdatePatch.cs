using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using SongCore;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(AchievementsEvaluationHandler), "HandleCampaignOverallStatsDataDidUpdate")]
    public class AchievementsEvaluationHandlerHandleCampaignOverallStatsDataDidUpdatePatch
    {
        public static bool Prefix(MissionCompletionResults missionCompletionResults, MissionNode missionNode, AchievementsEvaluationHandler __instance)
        {
            var missionData = missionNode.missionData as CustomMissionDataSO;
            if (missionData != null)
            {
                __instance.ProcessMissionFinishData(missionNode, missionCompletionResults);
                var level = missionData.customLevel.levelID;
                IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                __instance.ProcessLevelFinishData(difficultyBeatmap, missionCompletionResults.levelCompletionResults);
                return false;
            }
            return true;
        }
    }
}
