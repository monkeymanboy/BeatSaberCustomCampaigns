using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using static MenuLightsPresetSO;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignFlowCoordinator), "HandleMissionSelectionNavigationControllerDidPressPlayButton")]
    class CampaignFlowCoordinatorHandleMissionSelectionNavigationControllerDidPressPlayButtonPatch
    {
        private static MenuLightsPresetSO baseObjectiveLightsPreset = null;
        private static MenuLightsPresetSO customObjectiveLights = null;

        public static bool Prefix(MissionSelectionNavigationController viewController, CampaignFlowCoordinator __instance)
        {
            MissionDataSO missionData = viewController.selectedMissionNode.missionData;
            MissionHelpSO missionHelp = missionData.missionHelp;
            if (baseObjectiveLightsPreset == null)
            {
                baseObjectiveLightsPreset = __instance.GetField<MenuLightsPresetSO, CampaignFlowCoordinator>("_newObjectiveLightsPreset");
            }
            customObjectiveLights = baseObjectiveLightsPreset;

            CustomMissionHelpSO customMissionHelp = missionHelp as CustomMissionHelpSO;
            if (customMissionHelp != null)
            {
                MissionInfo missionInfo = (missionHelp as CustomMissionHelpSO).missionInfo;
                if (missionInfo.lightColor != null)
                {
                    customObjectiveLights = UnityEngine.Object.Instantiate(baseObjectiveLightsPreset);

                    SimpleColorSO color = ScriptableObject.CreateInstance<SimpleColorSO>();
                    color.SetColor(new Color(missionInfo.lightColor.r, missionInfo.lightColor.g, missionInfo.lightColor.b));
                    foreach (LightIdColorPair pair in customObjectiveLights.lightIdColorPairs)
                    {
                        pair.baseColor = color;
                    }
                    __instance.SetField("_newObjectiveLightsPreset", customObjectiveLights);
                }
            }

            if (CustomCampaignManager.isCampaignLevel)
            {
                __instance.GetField<MenuLightsManager, CampaignFlowCoordinator>("_menuLightsManager").SetColorPreset(customObjectiveLights, true);
                MissionHelpViewController missionHelpViewController = __instance.GetField<MissionHelpViewController, CampaignFlowCoordinator>("_missionHelpViewController");
                missionHelpViewController.Setup(missionHelp);
                __instance.InvokeMethod<object, CampaignFlowCoordinator>("PresentViewController", missionHelpViewController, null, ViewController.AnimationDirection.Horizontal, false);
                return false;
            }

            return true;
        }

        public static void Postfix(CampaignFlowCoordinator __instance)
        {
            __instance.SetField("_newObjectiveLightsPreset", baseObjectiveLightsPreset);
        }
    }
}
