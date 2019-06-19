using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionNode), "missionId", MethodType.Getter)]
    class MissionNodeGetMissionId
    {
        static bool Prefix(ref string __result, MissionDataSO ____missionDataSO, string ____letterPartName, int ____numberPartName)
        {
            if(____missionDataSO is CustomMissionDataSO)
            {
                Challenge challenge = ((CustomMissionDataSO)____missionDataSO).challenge;
                __result = challenge.name + challenge.songid + challenge.difficulty + ____numberPartName.ToString() + ____letterPartName;
                return false;
            }
            return true;
        }
    }
}
