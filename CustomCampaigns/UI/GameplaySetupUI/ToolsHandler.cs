using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.FlowCoordinators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using UnityEngine.UI;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class ToolsHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MenuTransitionsHelper _menuTransitionsHelper;
        private CreditsManager _creditsManager;
        private DownloadManager _downloadManager;
        private Config _config;

        private Campaign.Campaign _campaign;

        private int songsDownloaded;
        private int downloadsFailed;

        [UIValue("credits-visible-unviewed")]
        public bool CreditsVisibleUnViewed { get; private set; }

        [UIValue("credits-visible-viewed")]
        public bool CreditsVisibleViewed { get; private set; }

        [UIValue("playlists-visible")]
        public bool PlaylistsVisible { get; private set; }

        [UIComponent("download-button")]
        Button downloadButton;

        [UIComponent("playlist-button")]
        Button playlistButton;

        public ToolsHandler(MenuTransitionsHelper menuTransitionsHelper, CreditsManager creditsManager, DownloadManager downloadManager, Config config)
        {
            _menuTransitionsHelper = menuTransitionsHelper;
            _creditsManager = creditsManager;
            _downloadManager = downloadManager;
            _config = config;

            PlaylistsVisible = Plugin.isPlaylistLibInstalled;
        }

        internal void SetCampaign(Campaign.Campaign campaign)
        {
            _campaign = campaign;

            var creditsVisible = File.Exists(campaign.campaignPath + "/" + "credits.json");
            CreditsVisibleUnViewed = creditsVisible && !_config.creditsViewed.Contains(_campaign.info.name);
            CreditsVisibleViewed = creditsVisible && _config.creditsViewed.Contains(_campaign.info.name);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisibleUnViewed)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisibleViewed)));

            downloadButton.interactable = true;
            downloadButton.SetButtonText("Download Missing Songs");

            playlistButton.interactable = true;

            playlistButton.SetButtonText(GetDoesPlaylistExist(_campaign) ? "Update Playlist" : "Create Playlist");
        }

        #region Download Buton
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
        #endregion

        [UIAction("playlist-click")]
        public void ExportPlaylist()
        {
            playlistButton.interactable = false;
            playlistButton.SetButtonText("Creating Playlist");

            BeatSaberPlaylistsLib.PlaylistManager playlistManager = BeatSaberPlaylistsLib.PlaylistManager.DefaultManager.CreateChildManager("Custom Campaigns");
            BeatSaberPlaylistsLib.Types.IPlaylist playlist;

            if (!playlistManager.TryGetPlaylist(GetPlaylistFileName(_campaign.info.name), out playlist))
            {
                playlist = playlistManager.CreatePlaylist("", _campaign.info.name, _campaign.info.name, "");
            }

            playlist.RemoveAll((x) => true);

            foreach (var mission in _campaign.missions)
            {
                var song = new BeatSaberPlaylistsLib.Legacy.LegacyPlaylistSong();

                if (mission.customDownloadURL != "")
                {
                    var customPreviewBeatmapLevel = mission.FindSong();
                    if (customPreviewBeatmapLevel == null)
                    {
                        continue;
                    }

                    song.LevelId = customPreviewBeatmapLevel.levelID;
                    song.Key = mission.songid;
                }

                else if (mission.hash != "")
                {
                    song.Hash = mission.hash;
                    song.Key = mission.songid;
                }

                else
                {
                    song.Key = mission.songid;
                }


                var diff = new BeatSaberPlaylistsLib.Types.Difficulty
                {
                    Name = mission.difficulty.ToString(),
                    Characteristic = mission.characteristic
                };

                bool songInPlaylist = false;
                foreach (var song2 in playlist)
                {
                    if (song2.Hash == song.Hash)
                    {
                        songInPlaylist = true;
                        (song2 as BeatSaberPlaylistsLib.Legacy.LegacyPlaylistSong).AddDifficulty(diff);
                        break;
                    }
                }

                if (!songInPlaylist)
                {
                    song.AddDifficulty(diff);
                    playlist.Add(song);
                }
            }

            string fileLocation = _campaign.campaignPath + "/cover.png";
            try
            {
                playlist.SetCover(new FileStream(fileLocation, FileMode.Open));
            }
            catch (Exception e)
            {
                Plugin.logger.Error($"Error setting cover image: {e}");
                playlist.SetCover(_campaign.info.name);
            }


            playlistManager.StorePlaylist(playlist);


            playlistButton.SetButtonText("Created Playlist");
        }

        [UIAction("credits-click")]
        public void ShowCredits()
        {
            Plugin.logger.Debug("credits :D");
            _creditsManager.StartingCustomCampaignCredits(CustomCampaignFlowCoordinator.CustomCampaignManager.Campaign);
            _menuTransitionsHelper.ShowCredits();

            if (_config.creditsViewed.Contains(_campaign.info.name))
            {
                return;
            }
            _config.creditsViewed.Add(_campaign.info.name);
            _config.Changed();

            CreditsVisibleViewed = true;
            CreditsVisibleUnViewed = false;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisibleUnViewed)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisibleViewed)));
        }

        private bool GetDoesPlaylistExist(Campaign.Campaign campaign)
        {
            string fileName = GetPlaylistFileName(campaign.info.name);
            return File.Exists(Path.Combine(Environment.CurrentDirectory, "Playlists", "Custom Campaigns", fileName + ".bplist"));
        }

        private string GetPlaylistFileName(string name)
        {
            return string.Join("_", string.Join("", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).Split()); ;
        }
    }
}
