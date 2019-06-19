using BeatSaberCustomCampaigns.campaign;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(CampaignProgressModelSO), "IsMissionCleared",
        new Type[] { typeof(string)})]
    class CampaignProgressModelSOIsMissionCleared
    {
        static bool Prefix(string missionId, ref bool __result)
        {
            if (CustomCampaignFlowCoordinator.unlockAllMissions)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
