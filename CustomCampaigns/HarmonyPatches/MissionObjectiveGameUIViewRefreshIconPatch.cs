using HarmonyLib;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionObjectiveGameUIView), "RefreshIcon")]
    public class MissionObjectiveGameUIViewRefreshIconPatch
    {
        private static int _numberOfParticles = 0;
        public static void Prefix(ref int ____numberOfParticles)
        {
            _numberOfParticles = ____numberOfParticles;
            if (Plugin.config.disableObjectiveParticles)
            {
                ____numberOfParticles = 0;
            }
        }

        public static void Postfix(ref int ____numberOfParticles)
        {
            ____numberOfParticles = _numberOfParticles;
        }
    }
}
