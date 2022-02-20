using System;

namespace CustomCampaigns.HarmonyPatches.ScoreSaber
{
    class PanelViewShowPatch
    {
        public static event Action ViewShown;
        public static void Postfix()
        {
            Plugin.logger.Debug("view shown");
            ViewShown?.Invoke();
        }
    }
}
