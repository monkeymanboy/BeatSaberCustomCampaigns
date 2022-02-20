using BeatSaberMarkupLanguage.Attributes;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.mission-secondary-leaderboard.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\mission-secondary-leaderboard.bsml")]
    public class CampaignMissionLeaderboardCoreViewController : CampaignMissionLeaderboardViewController
    {
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            UpdateLeaderboards();
        }
    }
}
