using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
using TMPro;

namespace BeatSaberCustomCampaigns
{
    public class CampaignChallengeLeaderboardViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.challenge-leaderboard.bsml";

        [UIComponent("leaderboard")]
        LeaderboardTableView table;
        TextMeshProUGUI text;
        public Challenge lastClicked;
        
        public void UpdateLeaderboards()
        {
            if (lastClicked != null) StartCoroutine(CustomCampaignLeaderboard.LoadLeaderboards(table, lastClicked));
        }
    }
}