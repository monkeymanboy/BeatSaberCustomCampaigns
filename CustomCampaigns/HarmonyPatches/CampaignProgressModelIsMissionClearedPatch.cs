using CustomCampaigns.Managers;
using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(CampaignProgressModel), "IsMissionCleared")]
    class CampaignProgressModelIsMissionClearedPatch
    {
        static bool Prefix(string missionId, ref bool __result)
        {
            if (CustomCampaignManager.unlockAllMissions)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
