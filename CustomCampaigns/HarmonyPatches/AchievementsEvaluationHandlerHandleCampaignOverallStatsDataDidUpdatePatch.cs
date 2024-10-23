using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using IPA.Utilities;
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
                __instance.InvokeMethod<object, AchievementsEvaluationHandler>("ProcessMissionFinishData", missionNode, missionCompletionResults);
                __instance.InvokeMethod<object, AchievementsEvaluationHandler>("ProcessLevelFinishData", missionNode.missionData.beatmapDifficulty, missionCompletionResults.levelCompletionResults);
                return false;
            }
            return true;
        }
    }
}
