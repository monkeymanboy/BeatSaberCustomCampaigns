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
    public class CampaignTotalLeaderboardViewController : VRUIViewController
    {
        LeaderboardTableView table;
        TextMeshProUGUI text;
        public string lastClicked = "";
        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                table = Instantiate(Resources.FindObjectsOfTypeAll<LeaderboardTableView>().First(), rectTransform, false);
                table.SetPrivateField("_rowHeight", 5.58f);
                (table.transform as RectTransform).anchoredPosition = new Vector3(0, (table.transform as RectTransform).anchoredPosition.y + 5);
                text = BeatSaberUI.CreateText(rectTransform, "Total Leaderboard", new Vector2(0, 35));
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 6;
            }
        }
        public void UpdateLeaderboards()
        {
            if (lastClicked != "") StartCoroutine(CustomCampaignLeaderboard.LoadTotalLeaderboards(table, "Anniversary-Expert"));
        }
    }
}
