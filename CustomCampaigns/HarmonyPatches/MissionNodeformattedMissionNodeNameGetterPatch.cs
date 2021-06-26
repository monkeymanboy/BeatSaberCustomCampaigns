using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionNode), "formattedMissionNodeName", MethodType.Getter)]
    class MissionNodeformattedMissionNodeNameGetterPatch
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
