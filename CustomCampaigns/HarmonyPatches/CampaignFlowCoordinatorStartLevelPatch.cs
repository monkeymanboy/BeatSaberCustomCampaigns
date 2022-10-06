using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using HarmonyLib;
using SongCore;
using System;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "StartLevel")]
    class CampaignFlowCoordinatorStartLevelPatch
    {
        static bool Prefix(Action beforeSceneSwitchCallback, CampaignFlowCoordinator __instance, MissionSelectionNavigationController ____missionSelectionNavigationController, MenuTransitionsHelper ____menuTransitionsHelper, PlayerDataModel ____playerDataModel)
        {
            if (CustomCampaignManager.isCampaignLevel)
            {
                return false;
            }
            CustomMissionDataSO missionData = ____missionSelectionNavigationController.selectedMissionNode.missionData as CustomMissionDataSO;
            if (missionData != null)
            {
                var level = missionData.customLevel.levelID;
                var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(level);
                IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(beatmapLevel.beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                GameplayModifiers gameplayModifiers = missionData.gameplayModifiers;
                MissionObjective[] missionObjectives = missionData.missionObjectives;
                PlayerSpecificSettings playerSpecificSettings = ____playerDataModel.playerData.playerSpecificSettings;
                ColorSchemesSettings colorSchemesSettings = ____playerDataModel.playerData.colorSchemesSettings;
                ColorScheme overrideColorScheme = colorSchemesSettings.overrideDefaultColors ? colorSchemesSettings.GetSelectedColorScheme() : null;

                ____menuTransitionsHelper.StartMissionLevel("", difficultyBeatmap, missionData.customLevel, overrideColorScheme, gameplayModifiers, missionObjectives, playerSpecificSettings, beforeSceneSwitchCallback, __instance.HandleMissionLevelSceneDidFinish, __instance.HandleMissionLevelSceneRestarted);

                return false;
            }
            return true;
        }
    }
}
