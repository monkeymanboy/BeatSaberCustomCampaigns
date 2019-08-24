using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using CustomCampaignLeaderboardLibrary;
using CustomUI.BeatSaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRUI;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignTotalLeaderboardViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.campaign-leaderboard.bsml";

        [UIComponent("leaderboard")]
        LeaderboardTableView table;
        public string lastClicked = "";

        public void UpdateLeaderboards()
        {
            StartCoroutine(CustomCampaignLeaderboard.LoadTotalLeaderboards(table, lastClicked));
        }
    }
}
