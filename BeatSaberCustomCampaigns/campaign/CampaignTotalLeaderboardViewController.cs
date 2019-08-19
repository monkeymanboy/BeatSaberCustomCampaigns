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
                table.GetPrivateField<LeaderboardTableCell>("_cellPrefab").GetPrivateField<TextMeshProUGUI>("_scoreText").enableWordWrapping = false;
                foreach (Transform child in table.transform.GetChild(0).GetChild(0)) //This is to ensure if a leaderboard with scores already on it gets cloned that old scores are cleared off
                {
                    GameObject.Destroy(child.gameObject);
                }

                table.SetPrivateField("_rowHeight", 5.58f);
                (table.transform as RectTransform).anchoredPosition = new Vector3(0, 0);
                text = BeatSaberUI.CreateText(rectTransform, "Total Leaderboard", new Vector2(0, 35));
                text.alignment = TextAlignmentOptions.Center;
                text.fontSize = 6;
            }
        }
        public void UpdateLeaderboards()
        {
            StartCoroutine(CustomCampaignLeaderboard.LoadTotalLeaderboards(table, lastClicked));
        }
    }
}
