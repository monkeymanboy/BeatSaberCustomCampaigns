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
    public class CampaignChallengeLeaderboardViewController : VRUIViewController
    {
        LeaderboardTableView table;
        TextMeshProUGUI text;
        public Challenge lastClicked;
        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                table = Instantiate(Resources.FindObjectsOfTypeAll<LeaderboardTableView>().First(), rectTransform, false);
                table.SetPrivateField("_rowHeight", 5.58f);
                (table.transform as RectTransform).anchoredPosition = new Vector3(0, 0);
                text = BeatSaberUI.CreateText(rectTransform, "Leaderboard", new Vector2(0, 35));
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 6;
            }
        }
        public void UpdateLeaderboards()
        {
            if (lastClicked != null) StartCoroutine(CustomCampaignLeaderboard.LoadLeaderboards(table, lastClicked));
        }
    }
}