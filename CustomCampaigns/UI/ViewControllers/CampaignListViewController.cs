using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using UnityEngine;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.campaign-list.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\campaign-list.bsml")]
    public class CampaignListViewController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        const string CustomCampaignsPathName = "/CustomCampaigns";

        public new event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("campaign-list")]
        internal CustomListTableData customListTableData;

        [UIValue("is-loaded")]
        protected bool isLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isLoaded)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(isLoading)));
            }
        }

        [UIValue("loading")]
        protected bool isLoading { get => !isLoaded; }

        private bool _isLoaded;

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
                rectTransform.anchorMin = new Vector3(0.5f, 0, 0);
                rectTransform.anchorMax = new Vector3(0.5f, 1, 0);
                rectTransform.sizeDelta = new Vector3(70, 0, 0);
            }
            if (addedToHierarchy)
            {
                isLoaded = false;
                StartCoroutine(LoadCampaigns());
            }
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
            customListTableData.tableView.ClearSelection();
            isLoaded = true;
        }

        private string ConvertPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}
