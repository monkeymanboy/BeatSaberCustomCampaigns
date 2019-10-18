using BeatSaberCustomCampaigns.campaign;
using BS_Utils.Utilities;
using CustomCampaignLeaderboardLibrary;
using Harmony;
using SongCore;
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
            ChallengeExternalModifiers.onChallengeEnd?.Invoke();
            if (missionCompletionResults.levelCompletionResults.levelEndAction == LevelCompletionResults.LevelEndAction.Restart)
            {
                ____missionLevelDetailViewController.GetPrivateField<Action<MissionLevelDetailViewController>>("didPressPlayButtonEvent")(____missionLevelDetailViewController);
                return false;
            }
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
                unlockedItemsViewController.index = 0;
                if(unlockedItemsViewController.items.Count>0) __instance.InvokeMethod("SetBottomScreenViewController", new object[] { unlockedItemsViewController, false });
                if (challenge.unlockMap)
                {
                    UnlockedMaps.CompletedChallenge(challenge.name);
                }
                //Score submission
                if (customMissionData.gameplayModifiers.songSpeedMul==1f && customMissionData.gameplayModifiers.fastNotes == false && customMissionData.gameplayModifiers.failOnSaberClash == false) {
                    SoloFreePlayFlowCoordinator freePlayCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();

                    PlayerDataModelSO dataModel = freePlayCoordinator.GetPrivateField<PlayerDataModelSO>("_playerDataModel");

                    PlayerData currentLocalPlayer = dataModel.playerData;
                    IDifficultyBeatmap difficultyBeatmap = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(customMissionData.customLevel.levelID).beatmapLevelData.GetDifficultyBeatmap(customMissionData.beatmapCharacteristic, customMissionData.beatmapDifficulty);
                    PlayerLevelStatsData playerLevelStatsData = currentLocalPlayer.GetPlayerLevelStatsData(difficultyBeatmap.level.levelID, difficultyBeatmap.difficulty, difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic);
                    LevelCompletionResults levelCompletionResults = missionCompletionResults.levelCompletionResults;
                    playerLevelStatsData.UpdateScoreData(levelCompletionResults.modifiedScore, levelCompletionResults.maxCombo, levelCompletionResults.fullCombo, levelCompletionResults.rank);
                    freePlayCoordinator.GetPrivateField<PlatformLeaderboardsModel>("_platformLeaderboardsModel").AddScoreFromComletionResults(difficultyBeatmap, levelCompletionResults);
                }

                __instance.StartCoroutine(CustomCampaignLeaderboard.SubmitScore(challenge, missionCompletionResults));
            }
            return true;
        }
    }
}
