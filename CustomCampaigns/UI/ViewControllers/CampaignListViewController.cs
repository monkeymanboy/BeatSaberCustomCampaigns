using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaigns.Campaign;
using HMUI;
using SiraUtil.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zenject;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.campaign-list.bsml")]
    [HotReload(RelativePathToLayout=@"..\Views\campaign-list.bsml")]
    public class CampaignListViewController : BSMLAutomaticViewController
    {
        const string CustomCampaignsPathName = "/CustomCampaigns";

        [UIComponent("campaign-list")]
        internal CustomListTableData customListTableData;

        public event Action<Campaign.Campaign> DidClickCampaignEvent;

        [UIAction("campaign-click")]
        protected void ClickedCampaign(TableView table, int row)
        {
            DidClickCampaignEvent?.Invoke(customListTableData.data[row] as Campaign.Campaign);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation)
            {

            }
            if (addedToHierarchy)
            {
                StartCoroutine(LoadCampaigns());
            }

            customListTableData.tableView.ClearSelection();
        }

        private IEnumerator LoadCampaigns()
        {
            customListTableData.data.Clear();
            var path = ConvertPath(Environment.CurrentDirectory);
            path += CustomCampaignsPathName;
            Directory.CreateDirectory(path);

            foreach (var campaign in Directory.GetDirectories(path))
            {
                var files = Directory.GetFiles(campaign, "info.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var campaignPath = ConvertPath(Path.GetDirectoryName(file));
                    try
                    {
                        Campaign.Campaign currentCampaign = new Campaign.Campaign(this, campaignPath);
                        customListTableData.data.Add(currentCampaign);
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.Error($"Failed to load custom campaign at {campaignPath}: {e.Message}");
                    }
                    yield return null;
                }
            }
            customListTableData.tableView.ReloadData();
        }

        private string ConvertPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
