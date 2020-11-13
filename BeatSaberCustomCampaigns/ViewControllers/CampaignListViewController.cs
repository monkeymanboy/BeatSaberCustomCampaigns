using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BeatSaberCustomCampaigns.campaign
{
    public class CampaignListViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.campaign-list.bsml";

        [UIComponent("list")]
        public CustomListTableData customListTableData;
        
        public Action<Campaign> clickedCampaign;
        
        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            if(addedToHierarchy) StartCoroutine(LoadCampaigns());
        }

        [UIAction("campaign-click")]
        private void ClickedRow(TableView table, int row)
        {
            clickedCampaign(customListTableData.data[row] as Campaign);
        }
        [UIAction("download-click")]
        private void DownloadClick()
        {
            System.Diagnostics.Process.Start("https://docs.google.com/spreadsheets/d/15e9M541X6XJrdRVgWnUK3pt5EX27h1cx_uTNeIog-10/edit?usp=sharing");
        }

        private IEnumerator LoadCampaigns()
        {
            customListTableData.data.Clear();
            string path = Environment.CurrentDirectory.Replace('\\', '/');
            Directory.CreateDirectory(path + "/CustomCampaigns");
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
                    yield return null;
                }
            }
            customListTableData.tableView.ReloadData();
        }
    }
}
