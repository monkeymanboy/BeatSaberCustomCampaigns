using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionLevelDetailViewController), "RefreshContent")]
    class MissionLevelDetailViewControllerRefreshContentPatch
    {
        private static CustomMissionDataSO _missionData;
        static bool Prefix(MissionLevelDetailViewController __instance, MissionNode ____missionNode, LevelBar ____levelBar, ObjectiveListItemsList ____objectiveListItems,
                            GameplayModifiersModelSO ____gameplayModifiersModel, GameObject ____modifiersPanelGO, GameplayModifierInfoListItemsList ____gameplayModifierInfoListItemsList)
        {
            if (____missionNode.missionData is CustomMissionDataSO)
            {
                _missionData = ____missionNode.missionData as CustomMissionDataSO;
                CustomPreviewBeatmapLevel level = _missionData.customLevel;
                if (level == null)
                {
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_songNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_difficultyText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<TextMeshProUGUI, LevelBar>("_authorNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetField<ImageView, LevelBar>("_songArtworkImageView").sprite = SongCore.Loader.defaultCoverImage;
                }
                else
                {
                    ____levelBar.Setup(level, _missionData.beatmapCharacteristic, _missionData.beatmapDifficulty);
                }

                MissionObjective[] missionObjectives = _missionData.missionObjectives;
                ____objectiveListItems.SetData((missionObjectives.Length == 0) ? 1 : missionObjectives.Length, OnItemFinish);

                List<GameplayModifierParamsSO> modifierParamsList = ____gameplayModifiersModel.CreateModifierParamsList(_missionData.gameplayModifiers);
                ____modifiersPanelGO.SetActive(modifierParamsList.Count > 0);

                ____gameplayModifierInfoListItemsList.SetData(modifierParamsList.Count, delegate (int idx, GameplayModifierInfoListItem gameplayModifierInfoListItem)
                {
                    GameplayModifierParamsSO gameplayModifierParamsSO = modifierParamsList[idx];
                    gameplayModifierInfoListItem.SetModifier(gameplayModifierParamsSO, true);
                });
                return false;
            }
            return true;
        }

        static void OnItemFinish(int idx, ObjectiveListItem objectiveListItem)
        {
            if (idx == 0 && _missionData.missionObjectives.Length == 0)
            {;
                objectiveListItem.title = Localization.Get("CAMPAIGN_FINISH_LEVEL");
                objectiveListItem.conditionText = "";
                objectiveListItem.hideCondition = true;
            }
            else
            {
                MissionObjective missionObjective = _missionData.missionObjectives[idx];
                if (missionObjective.type.noConditionValue)
                {;
                    objectiveListItem.title = missionObjective.type.objectiveNameLocalized.Replace(" ", "\n");
                    objectiveListItem.hideCondition = true;
                }
                else
                {
                    objectiveListItem.title = missionObjective.type.objectiveNameLocalized;
                    objectiveListItem.hideCondition = false;
                    ObjectiveValueFormatterSO objectiveValueFormater = missionObjective.type.objectiveValueFormater;
                    objectiveListItem.conditionText = $"{MissionDataExtensions.Name(missionObjective.referenceValueComparisonType)} {objectiveValueFormater.FormatValue(missionObjective.referenceValue)}";
                }
            }
        }
    }
}
