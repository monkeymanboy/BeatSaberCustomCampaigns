using BeatSaberCustomCampaigns.campaign;
using BS_Utils.Utilities;
using CustomCampaignLeaderboardLibrary;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionLevelSceneDidFinish",
        new Type[] { typeof(MissionLevelScenesTransitionSetupDataSO), typeof(MissionCompletionResults) })]
    class CampaignFlowCoordinatorHandleMissionLevelSceneDidFinish
    {
        static bool Prefix(MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupData, MissionCompletionResults missionCompletionResults, CampaignFlowCoordinator __instance, MissionSelectionNavigationController ____missionSelectionNavigationController)
        {
            if (!(____missionSelectionNavigationController.selectedMissionNode.missionData is CustomMissionDataSO)) return true;
            ChallengeExternalModifiers.onChallengeEnd?.Invoke();
            if (missionCompletionResults.levelCompletionResults.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
            {
                ____missionSelectionNavigationController.GetPrivateField<Action<MissionSelectionNavigationController>>("didPressPlayButtonEvent")(____missionSelectionNavigationController);
                Resources.FindObjectsOfTypeAll<CustomCampaignFlowCoordinator>().First().LoadExternalModifiers((____missionSelectionNavigationController.selectedMissionNode.missionData as CustomMissionDataSO).challenge);
                return false;
            }
            if (missionCompletionResults.levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared && missionCompletionResults.IsMissionComplete)
            {
                CustomMissionDataSO customMissionData = ____missionSelectionNavigationController.selectedMissionNode.missionData as CustomMissionDataSO;
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
                unlockedItemsViewController.index = 0;
                if(unlockedItemsViewController.items.Count>0) __instance.InvokeMethod("SetBottomScreenViewController", new object[] { unlockedItemsViewController, false });
                if (challenge.unlockMap)
                {
                    UnlockedMaps.CompletedChallenge(challenge.name);
                }
                /*
                //Score submission
                if (customMissionData.gameplayModifiers.songSpeedMul==1f && customMissionData.gameplayModifiers.fastNotes == false && customMissionData.gameplayModifiers.failOnSaberClash == false) {
                    SoloFreePlayFlowCoordinator freePlayCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();

                    PlayerDataModel dataModel = freePlayCoordinator.GetPrivateField<PlayerDataModel>("_playerDataModel");

                    PlayerData currentLocalPlayer = dataModel.playerData;
                    IDifficultyBeatmap difficultyBeatmap = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(customMissionData.customLevel.levelID).beatmapLevelData.GetDifficultyBeatmap(customMissionData.beatmapCharacteristic, customMissionData.beatmapDifficulty);
                    PlayerLevelStatsData playerLevelStatsData = currentLocalPlayer.GetPlayerLevelStatsData(difficultyBeatmap.level.levelID, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
                    LevelCompletionResults levelCompletionResults = missionCompletionResults.levelCompletionResults;
                    playerLevelStatsData.UpdateScoreData(levelCompletionResults.modifiedScore, levelCompletionResults.maxCombo, levelCompletionResults.fullCombo, levelCompletionResults.rank);
                    //todo Need change???
                    //freePlayCoordinator.GetPrivateField<PlatformLeaderboardsModel>("_platformLeaderboardsModel").AddScoreFromComletionResults(difficultyBeatmap, levelCompletionResults);
                }
                */
                if (!string.IsNullOrWhiteSpace(campaign.completionPost))
                {
                    CompleteSubmission submission = new CompleteSubmission();
                    submission.challengeHash = challenge.GetHash();
                    submission.score = missionCompletionResults.levelCompletionResults.rawScore;
                    submission.userID = APITools.UserID;
                    foreach(MissionObjectiveResult objective in missionCompletionResults.missionObjectiveResults)
                    {
                        Requirement requirement = new Requirement();
                        requirement.name = objective.missionObjective.type.objectiveName;
                        requirement.value = objective.value;
                        submission.requirements.Add(requirement);
                    }
                    __instance.StartCoroutine(submission.Submit(campaign.completionPost));
                }
                __instance.StartCoroutine(CustomCampaignLeaderboard.SubmitScore(challenge, missionCompletionResults));
            }
            return true;
        }
    }
}
