using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
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
    public class CampaignListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.campaign-list.bsml";

        [UIComponent("list")]
        public CustomListTableData customListTableData;
        
        public Action<Campaign> clickedCampaign;
        
        protected override void DidActivate(bool firstActivation, ActivationType type)
        {
            base.DidActivate(firstActivation, type);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            if(type==ActivationType.AddedToHierarchy) LoadCampaigns();
            customListTableData.tableView.ReloadData();
        }

        [UIAction("campaign-click")]
        private void ClickedRow(TableView table, int row)
        {
            clickedCampaign(customListTableData.data[row] as Campaign);
        }

        private void LoadCampaigns()
        {
            customListTableData.data.Clear();
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
                        customListTableData.data.Add(curr);
                    } catch
                    {
                        Console.WriteLine("[Challenges] failed to load campaign at " + campaignPath);
                    }
                }
            }
        }
    }
}
