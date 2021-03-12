using HarmonyLib;
using SongCore;
using System;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "StartLevel",
        new Type[] { typeof(Action)})]
    class CampaignFlowCoordinatorStartLevel
    {
        static bool Prefix(Action beforeSceneSwitchCallback, CampaignFlowCoordinator __instance, MissionSelectionNavigationController ____missionSelectionNavigationController, MenuTransitionsHelper ____menuTransitionsHelper, PlayerDataModel ____playerDataModel)
        {

            if (____missionSelectionNavigationController.selectedMissionNode.missionData is CustomMissionDataSO)
            {
                CustomMissionDataSO missionData = ____missionSelectionNavigationController.selectedMissionNode.missionData as CustomMissionDataSO;
                
                IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(missionData.customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                GameplayModifiers gameplayModifiers = missionData.gameplayModifiers;
                MissionObjective[] missionObjectives = missionData.missionObjectives;
                PlayerSpecificSettings playerSpecificSettings = ____playerDataModel.playerData.playerSpecificSettings;
                ColorSchemesSettings colorSchemesSettings = ____playerDataModel.playerData.colorSchemesSettings;
                ColorScheme overrideColorScheme = colorSchemesSettings.overrideDefaultColors ? colorSchemesSettings.GetSelectedColorScheme() : null;
                ____menuTransitionsHelper.StartMissionLevel("", difficultyBeatmap, missionData.level, overrideColorScheme, gameplayModifiers, missionObjectives, playerSpecificSettings, beforeSceneSwitchCallback, __instance.HandleMissionLevelSceneDidFinish);
                return false;
            }
            return true;
        }
    }
}
