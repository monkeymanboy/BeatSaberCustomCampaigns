using BeatSaberCustomCampaigns;
using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Campaign.Missions;
using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionLevelSceneDidFinish")]
    public class CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch
    {
        public static void Prefix(CampaignFlowCoordinator __instance, MissionLevelScenesTransitionSetupDataSO missionLevelScenesTransitionSetupData,
                                    MissionCompletionResults missionCompletionResults, MissionSelectionNavigationController ____missionSelectionNavigationController)
        {
            Plugin.logger.Debug("HandleMissionLevelSceneDidFinish prefix");
            if (missionCompletionResults.levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared && missionCompletionResults.IsMissionComplete)
            {
                Plugin.logger.Debug("cleared mission");
                CustomMissionDataSO customMissionData = ____missionSelectionNavigationController.selectedMissionNode.missionData as CustomMissionDataSO;
                Campaign.Campaign campaign = customMissionData.campaign;
                Mission mission = customMissionData.mission;
                //foreach (UnlockableItem item in challenge.unlockableItems)
                //{
                //    try
                //    {
                //        item.UnlockItem(campaign.path);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("Failed to unlock item: " + item.fileName + " - Exception: " + ex.Message);
                //    }
                //}
                //UnlockedItemsViewController unlockedItemsViewController = Resources.FindObjectsOfTypeAll<UnlockedItemsViewController>().First();
                //unlockedItemsViewController.items = challenge.unlockableItems;
                //unlockedItemsViewController.index = 0;
                //if (unlockedItemsViewController.items.Count > 0) __instance.InvokeMethod("SetBottomScreenViewController", new object[] { unlockedItemsViewController, HMUI.ViewController.AnimationType.None });
                //if (challenge.unlockMap)
                //{
                //    UnlockedMaps.CompletedChallenge(challenge.name);
                //}

                //if (!string.IsNullOrWhiteSpace(campaign.completionPost))
                //{
                //    CompleteSubmission submission = new CompleteSubmission();
                //    Challenge challenge = new Challenge(mission);
                //    submission.challengeHash = challenge.GetHash();
                //    submission.score = missionCompletionResults.levelCompletionResults.rawScore;
                //    submission.userID = APITools.UserID;
                //    foreach (MissionObjectiveResult objective in missionCompletionResults.missionObjectiveResults)
                //    {
                //        Requirement requirement = new Requirement();
                //        requirement.name = objective.missionObjective.type.objectiveName;
                //        requirement.value = objective.value;
                //        submission.requirements.Add(requirement);
                //    }
                //    __instance.StartCoroutine(submission.Submit(campaign.completionPost));
                //}
                Challenge challenge = new Challenge(mission);
                Plugin.logger.Debug("submitting score...");
                __instance.StartCoroutine(CustomCampaignLeaderboard.SubmitScore(challenge, missionCompletionResults));
            }
        }
    }
}
