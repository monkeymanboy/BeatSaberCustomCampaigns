using BS_Utils.Utilities;
using HarmonyLib;
using HMUI;
using Polyglot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionLevelDetailViewController), "RefreshContent",
        new Type[] { })]
    class MissionLevelDetailViewControllerRefreshContent
    {
        static bool Prefix(MissionLevelDetailViewController __instance, MissionNode ____missionNode, LevelBar ____levelBar, ObjectiveListItemsList ____objectiveListItems, GameplayModifiersModelSO ____gameplayModifiersModel, GameObject ____modifiersPanelGO, GameplayModifierInfoListItemsList ____gameplayModifierInfoListItemsList)
        {
            if (____missionNode.missionData is CustomMissionDataSO)
            {
                CustomMissionDataSO missionData = (____missionNode.missionData as CustomMissionDataSO);
                CustomPreviewBeatmapLevel level = missionData.customLevel;
                if (level == null)
                {
                    // TODO: Localization?
                    ____levelBar.GetPrivateField<TextMeshProUGUI>("_songNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetPrivateField<TextMeshProUGUI>("_difficultyText").text = "SONG NOT FOUND";
                    ____levelBar.GetPrivateField<TextMeshProUGUI>("_authorNameText").text = "SONG NOT FOUND";
                    ____levelBar.GetPrivateField<ImageView>("_songArtworkImageView").sprite = SongCore.Loader.defaultCoverImage;
                }
                else
                {
                    ____levelBar.Setup(level, missionData.beatmapCharacteristic, missionData.beatmapDifficulty);
                }
                MissionObjective[] missionObjectives = missionData.missionObjectives;
                ____objectiveListItems.SetData((missionObjectives.Length == 0) ? 1 : missionObjectives.Length, delegate (int idx, ObjectiveListItem objectiveListItem)
                {
                    if (idx == 0 && missionObjectives.Length == 0)
                    {
                        objectiveListItem.title = Localization.Get("CAMPAIGN_FINISH_LEVEL");
                        objectiveListItem.conditionText = "";
                        objectiveListItem.hideCondition = true;
                    }
                    else
                    {
                        MissionObjective missionObjective = missionObjectives[idx];
                        if (missionObjective.type.noConditionValue)
                        {
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
                });
                List<GameplayModifierParamsSO> modifierParamsList = ____gameplayModifiersModel.CreateModifierParamsList(missionData.gameplayModifiers);
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
    }
}
