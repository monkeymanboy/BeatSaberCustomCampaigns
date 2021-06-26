using HarmonyLib;
using TMPro;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionObjectiveGameUIView), "RefreshValue")]
    public class MissionObjectiveGameUIViewRefreshValuePatch
    {
        public static bool Prefix(TextMeshProUGUI ____valueText, MissionObjectiveChecker ____missionObjectiveChecker)
        {
            ____valueText.text = ____missionObjectiveChecker.missionObjective.type.objectiveValueFormater.FormatValue(____missionObjectiveChecker.checkedValue);
            return false;
        }
    }
}
