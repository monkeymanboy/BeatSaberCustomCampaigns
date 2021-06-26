using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(GameplayModifiersModelSO), "CreateModifierParamsList")]
    public class GameplayModifiersModelSOCreateModifierParamsListPatch
    {
        public static void Postfix(GameplayModifiers gameplayModifiers, GameplayModifierParamsSO ____fasterSong, GameplayModifierParamsSO ____slowerSong, ref List<GameplayModifierParamsSO> __result)
        {
            if (gameplayModifiers is CustomGameplayModifiers)
            {
                MissionModifiers mission = (gameplayModifiers as CustomGameplayModifiers).missionModifiers;
                // Custom song speed
                if (mission.songSpeed == GameplayModifiers.SongSpeed.Normal && mission.speedMul != 1)
                {
                    GameplayModifierParamsSO speedParamsSo = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                    speedParamsSo.SetField("_modifierNameLocalizationKey", "Song Speed - " + (int)(mission.speedMul * 100) + "%");
                    if (mission.speedMul < 1)
                    {
                        speedParamsSo.SetField("_descriptionLocalizationKey", "Song will play slower.");
                        speedParamsSo.SetField("_icon", ____slowerSong.icon);
                    }
                    else
                    {
                        speedParamsSo.SetField("_descriptionLocalizationKey", "Song will play faster.");
                        speedParamsSo.SetField("_icon", ____fasterSong.icon);
                    }
                    __result.Add(speedParamsSo);
                }
                // TODO: Other modifiers
            }
        }
    }
}
