using System;

namespace CustomCampaigns.HarmonyPatches.ScoreSaber
{
    class PanelViewsIsLoadedSetterPatch
    {
        public static event Action<bool> ViewLoaded;
        public static void Postfix(bool ____isLoaded)
        {
            ViewLoaded?.Invoke(____isLoaded);
        }
    }
}
