using HarmonyLib;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MenuLightsPresetSO;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionSelectionNavigationControllerDidPressPlayButton")]
    class CampaignFlowCoordinatorHandleMissionSelectionNavigationControllerDidPressPlayButton
    {
        private static MenuLightsPresetSO baseObjectiveLightsPreset = null;
        static void Prefix(MissionSelectionNavigationController viewController, CampaignFlowCoordinator __instance)
        {
            MissionHelpSO missionHelp = viewController.selectedMissionNode.missionData.missionHelp;
            if (baseObjectiveLightsPreset == null)
            {
                baseObjectiveLightsPreset = __instance.GetField<MenuLightsPresetSO, CampaignFlowCoordinator>("_newObjectiveLightsPreset");
            }
            if (missionHelp is CustomMissionHelpSO)
            {
                ChallengeInfo challengeInfo = (missionHelp as CustomMissionHelpSO).challengeInfo;
                if (challengeInfo.lightColor != null)
                {
                    MenuLightsPresetSO customObjectiveLights = UnityEngine.Object.Instantiate(baseObjectiveLightsPreset);

                    SimpleColorSO color = ScriptableObject.CreateInstance<SimpleColorSO>();
                    color.SetColor(new Color(challengeInfo.lightColor.r, challengeInfo.lightColor.g, challengeInfo.lightColor.b));
                    foreach (LightIdColorPair pair in customObjectiveLights.lightIdColorPairs)
                    {
                        pair.baseColor = color;
                    }
                    __instance.SetField("_newObjectiveLightsPreset", customObjectiveLights);
                }
                
            }
        }

        static void Postfix(CampaignFlowCoordinator __instance)
        {
            __instance.SetField("_newObjectiveLightsPreset", baseObjectiveLightsPreset);
        }
    }
}
