using CustomCampaigns.Managers;
using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionToggle), "ChangeSelection")]
    class MissionToggleChangeSelectionPatch
    {
        public static bool Prefix()
        {
            return CustomCampaignManager.downloadingNode == null;
        }
    }
}
