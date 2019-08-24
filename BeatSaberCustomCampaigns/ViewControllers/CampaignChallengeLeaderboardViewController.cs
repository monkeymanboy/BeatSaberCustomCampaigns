using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using VRUI;
using static LeaderboardTableView;

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