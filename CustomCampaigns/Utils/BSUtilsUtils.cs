using IPA.Loader;
using System.Linq;

namespace CustomCampaigns.Utils
{
    public static class BSUtilsUtils
    {
        public static bool isBSUtilsInstalled = false;
        public static bool wasSubmissionDisabled = false;

        // songcore depends on bs utils so I would be quite surprised if this ever returns false
        public static void CheckForBSUtilsInstall()
        {
            PluginMetadata bsUtilsPlugin = PluginManager.EnabledPlugins.First(x => x.Id == "BS Utils");
            isBSUtilsInstalled = bsUtilsPlugin != null;
        }

        public static bool WasSubmissionDisabled()
        {
            return isBSUtilsInstalled && BS_Utils.Gameplay.ScoreSubmission.WasDisabled;
        }

        public static bool IsSubmissionDisabled()
        {
            return isBSUtilsInstalled && BS_Utils.Gameplay.ScoreSubmission.Disabled;
        }
    }
}
