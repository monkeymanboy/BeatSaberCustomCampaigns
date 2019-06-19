using BeatSaberCustomCampaigns.campaign;
using CustomCampaignLeaderboardLibrary;
using CustomUI.Utilities;
using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionLevelSceneDidFinish",
        new Type[] { typeof(MissionLevelScenesTransitionSetupDataSO), typeof(MissionCompletionResults) })]
    class CampaignFlowCoordinatorHandleMissionLevelSceneDidFinish
    {
        static bool Prefix(MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupData, MissionCompletionResults missionCompletionResults, CampaignFlowCoordinator __instance, MissionLevelDetailViewController ____missionLevelDetailViewController)
        {
            if (!(____missionLevelDetailViewController.missionNode.missionData is CustomMissionDataSO)) return true;
            if (missionCompletionResults.levelCompletionResults.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
            {
                ____missionLevelDetailViewController.GetPrivateField<Action<MissionLevelDetailViewController>>("didPressPlayButtonEvent")(____missionLevelDetailViewController);
                return false;
            }
            ChallengeExternalModifiers.onChallengeEnd?.Invoke();
            if (missionCompletionResults.levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                CustomMissionDataSO customMissionData = ____missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO;
                Campaign campaign = customMissionData.campaign;
                Challenge challenge = customMissionData.challenge;
                foreach(UnlockableItem item in challenge.unlockableItems)
                {
                    try
                    {
                        item.UnlockItem(campaign.path);
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Failed to unlock item: " + item.fileName + " - Exception: " + ex.Message);
                    }
                }
                UnlockedItemsViewController unlockedItemsViewController = Resources.FindObjectsOfTypeAll<UnlockedItemsViewController>().First();
                unlockedItemsViewController.items = challenge.unlockableItems;
                if(unlockedItemsViewController.items.Count>0) __instance.InvokePrivateMethod("SetBottomScreenViewController", new object[] { unlockedItemsViewController, false });
                if (challenge.unlockMap)
                {
                    UnlockedMaps.CompletedChallenge(challenge.name);
                }


                __instance.StartCoroutine(CustomCampaignLeaderboard.SubmitScore(challenge, missionCompletionResults));
            }
            return true;
        }
    }
}
