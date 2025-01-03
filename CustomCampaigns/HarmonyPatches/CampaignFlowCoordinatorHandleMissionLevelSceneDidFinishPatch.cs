﻿using HarmonyLib;
using System;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionLevelSceneDidFinish")]
    public class CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch
    {
        public static Action<MissionLevelScenesTransitionSetupDataSO, MissionCompletionResults> onMissionSceneFinish;

        public static void Postfix(CampaignFlowCoordinator __instance, MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupData,
                                    MissionCompletionResults missionCompletionResults, MissionSelectionNavigationController ____missionSelectionNavigationController)
        {
            onMissionSceneFinish?.Invoke(missionLevelScenesTransitionSetupData, missionCompletionResults);
        }
    }
}
