﻿using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Campaign.Missions;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.mission-secondary-leaderboard.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\mission-secondary-leaderboard.bsml")]
    public class CampaignMissionSecondaryLeaderboardViewController : CampaignMissionLeaderboardViewController
    {
        private Mission _lastLoadedMission = null;
        internal void Shown()
        {
            if (_lastLoadedMission != mission)
            {
                UpdateLeaderboards();
                _lastLoadedMission = mission;
            }
        }
    }
}
