using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using IPA.Utilities;
using System;
using System.Threading;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionSelectionMapViewController), "HandleMissionNodeSelectionManagerDidSelectMissionNode")]
    class MissionSelectionMapViewControllerHandleMissionNodeSelectionManagerDidSelectMissionNodePatch
    {
        static bool Prefix(MissionNodeVisualController missionNodeVisualController, MissionSelectionMapViewController __instance, SongPreviewPlayer ____songPreviewPlayer)
        {
            if (missionNodeVisualController.missionNode.missionData is CustomMissionDataSO)
            {
                __instance.SetField("_selectedMissionNode", missionNodeVisualController.missionNode);
                CustomPreviewBeatmapLevel level = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).customLevel;
                if (level != null)
                {
                    __instance.SongPlayerCrossfadeToLevelAsync(level);
                }
                __instance.GetField<Action<MissionSelectionMapViewController, MissionNode>, MissionSelectionMapViewController>("didSelectMissionLevelEvent")?.Invoke(__instance, missionNodeVisualController.missionNode);
                return false;
            }
            return true;
        }
    }
}
