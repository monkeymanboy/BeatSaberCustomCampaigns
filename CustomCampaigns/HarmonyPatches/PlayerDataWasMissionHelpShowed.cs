using CustomCampaigns.Campaign.Missions;
using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(PlayerData), "WasMissionHelpShowed")]
    class PlayerDataWasMissionHelpShowed
    {
        public static bool Prefix(MissionHelpSO missionHelp, ref bool __result)
        {
            CustomMissionHelpSO customMissionHelp = missionHelp as CustomMissionHelpSO;
            if (customMissionHelp != null && customMissionHelp.missionInfo.showEverytime)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
