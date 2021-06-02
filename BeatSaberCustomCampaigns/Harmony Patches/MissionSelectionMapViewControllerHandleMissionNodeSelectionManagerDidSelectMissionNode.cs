using BS_Utils.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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
                if (level!= null)
                {
                    var audioTask = level.GetPreviewAudioClipAsync(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token);
                    if (audioTask.IsCompleted)
                    {
                        ____songPreviewPlayer.CrossfadeTo(audioTask.Result, -4f, level.previewStartTime, level.previewDuration);
                    }
                }
                __instance.GetPrivateField<Action<MissionSelectionMapViewController, MissionNode>>("didSelectMissionLevelEvent")(__instance, missionNodeVisualController.missionNode);
                return false;
            }
            return true;
        }
    }
}
