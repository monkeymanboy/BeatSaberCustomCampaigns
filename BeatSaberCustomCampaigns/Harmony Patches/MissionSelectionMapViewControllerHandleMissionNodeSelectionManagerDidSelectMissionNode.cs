using BS_Utils.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionSelectionMapViewController), "HandleMissionNodeSelectionManagerDidSelectMissionNode",
        new Type[] { typeof(MissionNodeVisualController) })]
    class MissionSelectionMapViewControllerHandleMissionNodeSelectionManagerDidSelectMissionNode
    {
        static bool Prefix(MissionNodeVisualController missionNodeVisualController, MissionSelectionMapViewController __instance, SongPreviewPlayer ____songPreviewPlayer)
        {
            if (missionNodeVisualController.missionNode.missionData is CustomMissionDataSO)
            {
                __instance.SetPrivateField("_selectedMissionNode", missionNodeVisualController.missionNode);
                CustomPreviewBeatmapLevel level = (missionNodeVisualController.missionNode.missionData as CustomMissionDataSO).customLevel;
                if(level!=null)LoadAudio(missionNodeVisualController, level, __instance, ____songPreviewPlayer);
                else __instance.GetPrivateField<Action<MissionSelectionMapViewController, MissionNode>>("didSelectMissionLevelEvent")(__instance, missionNodeVisualController.missionNode);
                return false;
            }
            return true;
        }

        async static void LoadAudio(MissionNodeVisualController missionNodeVisualController, CustomPreviewBeatmapLevel level, MissionSelectionMapViewController __instance, SongPreviewPlayer ____songPreviewPlayer)
        {
            ____songPreviewPlayer.CrossfadeTo(await level.GetPreviewAudioClipAsync(new System.Threading.CancellationToken()), level.previewStartTime, level.previewDuration);
            __instance.GetPrivateField<Action<MissionSelectionMapViewController, MissionNode>>("didSelectMissionLevelEvent")(__instance, missionNodeVisualController.missionNode);
        }
    }
}
