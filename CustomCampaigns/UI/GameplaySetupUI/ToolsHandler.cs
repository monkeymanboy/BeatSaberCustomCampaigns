using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.FlowCoordinators;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using Zenject;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class ToolsHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MenuTransitionsHelper _menuTransitionsHelper;
        private CreditsManager _creditsManager;
        private DownloadManager _downloadManager;

        private Campaign.Campaign _campaign;

        private int songsDownloaded;
        private int downloadsFailed;

        [UIValue("credits-visible")]
        public bool CreditsVisible { get; private set; }

        [UIComponent("download-button")]
        Button downloadButton;

        public ToolsHandler(MenuTransitionsHelper menuTransitionsHelper, CreditsManager creditsManager, DownloadManager downloadManager)
        {
            _menuTransitionsHelper = menuTransitionsHelper;
            _creditsManager = creditsManager;
            _downloadManager = downloadManager;
        }

        internal void SetCampaign(Campaign.Campaign campaign)
        {
            _campaign = campaign;
            CreditsVisible = File.Exists(campaign.campaignPath + "/" + "credits.json");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisible)));

            downloadButton.interactable = true;
            downloadButton.SetButtonText("Download Missing Songs");
        }

        [UIAction("download-click")]
        public void DownloadMissingSongs()
        {
            downloadButton.interactable = false;
            downloadButton.SetButtonText("Downloading...");

            List<DownloadManager.DownloadEntry> downloadEntries = new List<DownloadManager.DownloadEntry>();

            foreach (Mission mission in _campaign.missions)
            {
                if (mission.FindSong() == null)
                {
                    downloadEntries.Add(new DownloadManager.DownloadEntry(mission.songid, mission.hash, mission.customDownloadURL));
                }
            }

            if (downloadEntries.Count == 0)
            {
                OnDownloadFinish();
            }

            if (_downloadManager.AddSongsToQueue(downloadEntries))
            {

                _downloadManager.OnDownloadSuccess -= OnDownloadSuccess;
                _downloadManager.OnDownloadSuccess += OnDownloadSuccess;

                _downloadManager.OnDownloadFail -= OnDownloadFail;
                _downloadManager.OnDownloadFail += OnDownloadFail;

                _downloadManager.OnQueueComplete -= OnDownloadFinish;
                _downloadManager.OnQueueComplete += OnDownloadFinish;

                downloadButton.SetButtonText($"Downloading 1 / {_downloadManager.GetQueueSize()}...");
                _downloadManager.InitiateDownloads();
            }
        }

        private void OnDownloadFinish()
        {
            if (downloadsFailed > 0)
            {
                downloadButton.SetButtonText($"{downloadsFailed} downloads failed");
                downloadButton.interactable = true;
            }
            else
            {
                downloadButton.SetButtonText("Downloaded");
            }

            if (downloadsFailed < songsDownloaded)
            {
                SongCore.Loader.Instance.RefreshSongs();
            }

            songsDownloaded = 0;
            downloadsFailed = 0;

            _downloadManager.OnDownloadSuccess -= OnDownloadSuccess;
            _downloadManager.OnDownloadFail -= OnDownloadFail;
            _downloadManager.OnQueueComplete -= OnDownloadFinish;
        }

        private void OnDownloadSuccess()
        {
            songsDownloaded += 1;

            downloadButton.SetButtonText($"Downloading {songsDownloaded + 1} / {_downloadManager.GetQueueSize()}...");
        }

        private void OnDownloadFail()
        {
            songsDownloaded += 1;
            downloadsFailed += 1;

            downloadButton.SetButtonText($"Downloading {songsDownloaded + 1} / {_downloadManager.GetQueueSize()}...");
        }

        [UIAction("credits-click")]
        public void ShowCredits()
        {
            Plugin.logger.Debug("credits :D");
            _creditsManager.StartingCustomCampaignCredits(CustomCampaignFlowCoordinator.CustomCampaignManager.Campaign);
            _menuTransitionsHelper.ShowCredits();
        }
    }
}
