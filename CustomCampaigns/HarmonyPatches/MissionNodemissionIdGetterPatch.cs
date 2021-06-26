using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionNode), "missionId", MethodType.Getter)]
    class MissionNodemissionIdGetterPatch
    {
        static bool Prefix(ref string __result, MissionDataSO ____missionDataSO, string ____letterPartName, int ____numberPartName)
        {
            if (____missionDataSO is CustomMissionDataSO)
            {
                Mission mission = (____missionDataSO as CustomMissionDataSO).mission;
                __result = $"{mission.name}{mission.songid}{mission.difficulty}{____numberPartName}{ ____letterPartName}";
                return false;
            }
            return true;
        }
    }
}
