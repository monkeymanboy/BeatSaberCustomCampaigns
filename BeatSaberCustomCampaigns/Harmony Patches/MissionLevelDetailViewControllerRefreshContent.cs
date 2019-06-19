using Harmony;
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
        static bool Prefix(MissionLevelDetailViewController __instance, MissionNode ____missionNode, TextMeshProUGUI ____missionText, TextMeshProUGUI ____songNameText, TextMeshProUGUI ____difficultyText, TextMeshProUGUI ____characteristicsText, ObjectiveListItemsList ____objectiveListItems, GameplayModifiersModelSO ____gameplayModifiersModel, GameObject ____modifiersPanelGO, GameplayModifierInfoListItemsList ____gameplayModifierInfoListItemsList)
        {
            if (____missionNode.missionData is CustomMissionDataSO)
            {
                CustomMissionDataSO missionData = (____missionNode.missionData as CustomMissionDataSO);
                CustomPreviewBeatmapLevel level = missionData.customLevel;
                ____missionText.text = Localization.Get("CAMPAIGN_MISSION") + " " + ____missionNode.formattedMissionNodeName;
                if (level == null)
                {
                    ____songNameText.text = string.Format(Localization.Get("CAMPAIGN_SONG"), "SONG NOT FOUND");
                    ____difficultyText.text = string.Format(Localization.Get("CAMPAIGN_DIFFICULTY"), "NOT FOUND");
                    ____characteristicsText.text = string.Format(Localization.Get("CAMPAIGN_TYPE"), "NOT FOUND");
                } else
                {
                    ____songNameText.text = string.Format(Localization.Get("CAMPAIGN_SONG"), level.songName);
                    ____difficultyText.text = string.Format(Localization.Get("CAMPAIGN_DIFFICULTY"), BeatmapDifficultyMethods.Name(missionData.beatmapDifficulty));
                    ____characteristicsText.text = string.Format(Localization.Get("CAMPAIGN_TYPE"), missionData.beatmapCharacteristic.characteristicNameLocalized);
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
                List<GameplayModifierParamsSO> modifierParamsList = ____gameplayModifiersModel.GetModifierParams(missionData.gameplayModifiers);
                ____modifiersPanelGO.SetActive(modifierParamsList.Count > 0);
                ____gameplayModifierInfoListItemsList.SetData(modifierParamsList.Count, delegate (int idx, GameplayModifierInfoListItem gameplayModifierInfoListItem)
                {
                    GameplayModifierParamsSO gameplayModifierParamsSO = modifierParamsList[idx];
                    gameplayModifierInfoListItem.modifierIcon = gameplayModifierParamsSO.icon;
                    gameplayModifierInfoListItem.modifierName = gameplayModifierParamsSO.localizedModifierName;
                    gameplayModifierInfoListItem.modifierDescription = gameplayModifierParamsSO.localizedHintText;
                    gameplayModifierInfoListItem.showSeparator = (idx != modifierParamsList.Count - 1);
                });
                return false;
            }
            return true;
        }
    }
}
