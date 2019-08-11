using CustomUI.BeatSaber;
using CustomUI.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRUI;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignListViewController : CustomListViewController
    {
        List<Campaign> campaigns = new List<Campaign>();
        
        public Action<Campaign> clickedCampaign;
        
        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            if (firstActivation)
            {
                reuseIdentifier = "CampaignListCell";
                DidSelectRowEvent += ClickedRow;
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            base.DidActivate(firstActivation, type);
            if(type==ActivationType.AddedToHierarchy) LoadCampaigns();
            _customListTableView.ReloadData();
        }

        private void ClickedRow(TableView table, int row)
        {
            clickedCampaign(campaigns[row]);
        }

        private void LoadCampaigns()
        {
            campaigns.Clear();
            Data.Clear();
            string path = Environment.CurrentDirectory.Replace('\\', '/');
            var folders = Directory.GetDirectories(path + "/CustomCampaigns").ToList();

            foreach (string campaign in folders)
            {
                var results = Directory.GetFiles(campaign, "info.json", SearchOption.AllDirectories);
                foreach (string result in results)
                {
                    string campaignPath = Path.GetDirectoryName(result).Replace('\\', '/');
                    try
                    {
                        Campaign curr = new Campaign(this, campaignPath);
                        campaigns.Add(curr);
                        Data.Add(curr);
                    } catch
                    {
                        Console.WriteLine("[Challenges] failed to load campaign at " + campaignPath);
                    }
                }
            }
        }
        public override TableCell CellForIdx(int idx)
        {
            LevelListTableCell _tableCell = GetTableCell();
            
            _tableCell.GetPrivateField<TextMeshProUGUI>("_songNameText").rectTransform.anchorMax = new Vector3(2,1,0);
            _tableCell.SetText(Data[idx].text);
            _tableCell.SetSubText(Data[idx].subtext);
            _tableCell.SetIcon(Data[idx].icon == null ? UIUtilities.BlankSprite : Data[idx].icon);

            return _tableCell;
        }
    }
}
