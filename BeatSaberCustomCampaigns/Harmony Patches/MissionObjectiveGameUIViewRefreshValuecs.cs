using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{

    [HarmonyPatch(typeof(MissionObjectiveGameUIView), "RefreshValue")]
    class MissionObjectiveGameUIViewRefreshValuecs
    {
        static bool Prefix(TextMeshProUGUI ____valueText, MissionObjectiveChecker ____missionObjectiveChecker)
        {
            string text = ____missionObjectiveChecker.missionObjective.type.objectiveValueFormater.FormatValue(____missionObjectiveChecker.checkedValue);
            ____valueText.text = text;
            return false;
        }
    }
}
