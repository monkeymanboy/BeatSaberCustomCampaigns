using Harmony;
using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(AchievementsEvaluationHandler), "HandleCampaignFlowCoordinatorDidUpdateOverallStatsDataWithLevelCompletionResults",
        new Type[] { typeof(CampaignFlowCoordinator), typeof(MissionNode), typeof(MissionCompletionResults) })]
    class AchievementsEvaluationHandlerHandleCampaignFlowCoordinatorDidUpdateOverallStatsDataWithLevelCompletionResults
    {
        static bool Prefix(CampaignFlowCoordinator campaignFlowCoordinator, MissionNode missionNode, MissionCompletionResults missionCompletionResults, AchievementsEvaluationHandler __instance)
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
