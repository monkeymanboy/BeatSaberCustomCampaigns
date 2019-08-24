using Harmony;
using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "StartLevel",
        new Type[] { typeof(Action)})]
    class CampaignFlowCoordinatorStartLevel
    {
        static bool Prefix(Action beforeSceneSwitchCallback, CampaignFlowCoordinator __instance, MissionLevelDetailViewController ____missionLevelDetailViewController, MenuTransitionsHelperSO ____menuTransitionsHelper, PlayerDataModelSO ____playerDataModel)
        {

            if (____missionLevelDetailViewController.missionNode.missionData is CustomMissionDataSO)
            {
                CustomMissionDataSO missionData = ____missionLevelDetailViewController.missionNode.missionData as CustomMissionDataSO;
                
                IDifficultyBeatmap difficultyBeatmap = BeatmapLevelDataExtensions.GetDifficultyBeatmap(Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(missionData.customLevel.levelID).beatmapLevelData, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                GameplayModifiers gameplayModifiers = missionData.gameplayModifiers;
                MissionObjective[] missionObjectives = missionData.missionObjectives;
                PlayerSpecificSettings playerSpecificSettings = ____playerDataModel.playerData.playerSpecificSettings;
                ____menuTransitionsHelper.StartMissionLevel(difficultyBeatmap, gameplayModifiers, missionObjectives, playerSpecificSettings, beforeSceneSwitchCallback, __instance.HandleMissionLevelSceneDidFinish);
                return false;
            }
            return true;
        }
    }
}
