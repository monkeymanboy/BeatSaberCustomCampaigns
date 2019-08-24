using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerDataModelSO;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(PlayerData), "WasMissionHelpShowed",
        new Type[] {typeof(MissionHelpSO)})]
    class PlayerDataWasMissionHelpShowed
    {
        static bool Prefix(MissionHelpSO missionHelp, ref bool __result)
        {
            if (missionHelp is CustomMissionHelpSO && (missionHelp as CustomMissionHelpSO).challengeInfo.showEverytime)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}
