using System;

namespace CustomCampaigns.HarmonyPatches.ScoreSaber
{
    class PanelViewsIsLoadedSetterPatch
    {
        public static event Action<bool> ViewLoaded;
        public static void Postfix(bool ____isLoaded)
        {
            Plugin.logger.Debug("view loaded");
            ViewLoaded?.Invoke(____isLoaded);
        }
    }
}
