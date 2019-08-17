using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionNode), "formattedMissionNodeName", MethodType.Getter)]
    class MissionNodeGetFormattedMissionNodeName
    {
        static bool Prefix(ref string __result, string ____letterPartName, int ____numberPartName)
        {
            if (____numberPartName == -1)
            {
                __result = ____letterPartName;
                return false;
            }
            return true;
        }
    }
}
