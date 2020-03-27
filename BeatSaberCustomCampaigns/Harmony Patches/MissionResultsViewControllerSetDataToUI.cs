using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionResultsViewController), "SetDataToUI",
        new Type[] { })]
    class MissionResultsViewControllerSetDataToUI
    {
        static void Postfix(MissionResultsViewController __instance, TextMeshProUGUI ____songNameText, MissionNode ____missionNode)
        {
            if(____missionNode.missionData is CustomMissionDataSO)
                ____songNameText.text = (____missionNode.missionData as CustomMissionDataSO).customLevel.songName;
        }
    }
}
