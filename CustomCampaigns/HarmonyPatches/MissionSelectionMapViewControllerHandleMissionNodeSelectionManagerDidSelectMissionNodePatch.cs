using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using IPA.Utilities;
using System;

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
                BeatmapLevel beatmapLevel = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).beatmapLevel;
                if (beatmapLevel != null)
                {
                    __instance.InvokeMethod<object, MissionSelectionMapViewController>("SongPlayerCrossfadeToLevelAsync", beatmapLevel);
                }
                __instance.GetField<Action<MissionSelectionMapViewController, MissionNode>, MissionSelectionMapViewController>("didSelectMissionLevelEvent")?.Invoke(__instance, missionNodeVisualController.missionNode);
                return false;
            }
            return true;
        }
    }
}
