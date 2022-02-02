using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomCampaigns.Managers
{
    public class DownloadManager
    {
        private Downloader _downloader;
        private HashSet<DownloadEntry> _queue;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isDownloading = false;

        public Action OnDownloadSuccess;
        public Action OnDownloadFail;
        public Action<float> DownloadProgress;
        public Action<string> DownloadStatus;

        public Action OnQueueComplete;

        public DownloadManager(Downloader downloader)
        {
            _downloader = downloader;
            _queue = new HashSet<DownloadEntry>();
        }

        internal void InitiateDownloads()
        {
            DownloadQueueAsync();
        }

        private async void DownloadQueueAsync()
        {
            _isDownloading = true;
            _cancellationTokenSource = new CancellationTokenSource();

            foreach (DownloadEntry song in _queue)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                if (song.customURL != null && song.customURL != "")
                {
                    var path = Path.Combine(CustomLevelPathHelper.customLevelsDirectoryPath, song.id);
                    path = _downloader.DeterminePathNumber(path, _cancellationTokenSource.Token);
                    await _downloader.DownloadMapFromUrlAsync(song.customURL, path, _cancellationTokenSource.Token, OnDownloadSuccess, OnDownloadFail, DownloadProgress, DownloadStatus);
                }
                else if (song.hash != null && song.hash != "")
                {
                    await _downloader.DownloadMapByHashAsync(song.hash, song.id, _cancellationTokenSource.Token, OnDownloadSuccess, OnDownloadFail, DownloadProgress, DownloadStatus);
                }
                else
                {
                    await _downloader.DownloadMapByIDAsync(song.id, _cancellationTokenSource.Token, OnDownloadSuccess, OnDownloadFail, DownloadProgress, DownloadStatus);
                }
            }

            _queue.Clear();

            OnQueueComplete?.Invoke();
            _isDownloading = false;
        }

        internal bool AddSongToQueue(DownloadEntry song)
        {
            if (_isDownloading)
            {
                return false;
            }

            _queue.Add(song);
            return true;
            
        }

        internal bool AddSongsToQueue(List<DownloadEntry> songs)
        {
            if (_isDownloading)
            {
                return false;
            }

            foreach (DownloadEntry song in songs)
            {
                _queue.Add(song);
            }
            return true;
        }

        public int GetQueueSize()
        {
            return _queue.Count;
        }

        public void CancelDownloads()
        {
            _cancellationTokenSource.Cancel();
        }

        internal class DownloadEntry
        {
            public string id;
            public string hash;
            public string customURL;

            public DownloadEntry(string id, string hash = null, string customURL = null)
            {
                this.id = id;
                this.hash = hash;
                this.customURL = customURL;
            }
        }
        
    }
}
