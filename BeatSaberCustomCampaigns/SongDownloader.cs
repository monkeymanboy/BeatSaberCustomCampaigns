using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatSaberCustomCampaigns
{
    class SongDownloader : PersistentSingleton<SongDownloader>
    {
        public bool IsDownloading { get; private set; } = false;

        private Action onFinish;
        private Action onFail;
        private Action onGetDownloadURL;
        private Action onGetDownloadURLFail;

        private string beatSaverURL;
        private DetailResponse beatSaverResponse;

        public void DownloadSong(string songid, string hash, string customDownloadURL, Action onFinish = null, Action onFail = null)
        {
            bool isBeatSaver = customDownloadURL == "";
            if (IsDownloading)
            {
                onFail?.Invoke();
                return;
            }
            this.onFail = onFail;
            this.onFinish = onFinish;
            IsDownloading = true;

            if (isBeatSaver)
            {
                if (hash == "")
                {
                    this.onGetDownloadURL = delegate { StartCoroutine(DownloadSongCoroutine(songid, beatSaverURL, isBeatSaver)); };
                    this.onGetDownloadURLFail = delegate { onFail?.Invoke(); };
                    StartCoroutine(GetDownloadUrl(songid));
                }

                else
                {
                    string url = "https://cdn.beatsaver.com/ " + hash.ToLower() + ".zip";
                    StartCoroutine(DownloadSongCoroutine(songid, url, isBeatSaver));
                }

                }
            else
            {
                StartCoroutine(DownloadSongCoroutine(songid, customDownloadURL, isBeatSaver));
            }
            
        }

        private IEnumerator DownloadSongCoroutine(string songid, string url, bool isBeatSaver)
        {
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", $"CustomCampaigns/v{Plugin.version}");

            var timeout = false;
            var time = 0f;

            var asyncRequest = www.SendWebRequest();

            while (!asyncRequest.isDone || asyncRequest.progress < 1f)
            {
                yield return null;

                time += Time.deltaTime;

                if (!(time >= 15f) || asyncRequest.progress != 0f) continue;
                www.Abort();
                timeout = true;
            }

            if (www.isNetworkError || www.isHttpError || timeout)
            {
                www.Abort();
                IsDownloading = false;
                onFail?.Invoke();
            }
            else
            {
                string docPath = "";
                string customSongsPath = "";

                byte[] data = www.downloadHandler.data;

                string extra = "";
                if (isBeatSaver)
                {
                    extra = " (" + beatSaverResponse.metadata.songName + " - " + beatSaverResponse.metadata.levelAuthorName + ")";
                }

                Stream zipStream = null;
                try
                {
                    docPath = Application.dataPath;
                    docPath = docPath.Substring(0, docPath.Length - 5);
                    docPath = docPath.Substring(0, docPath.LastIndexOf("/"));
                    customSongsPath = docPath + "/Beat Saber_Data/CustomLevels/" + songid + string.Concat(extra.Split(Path.GetInvalidFileNameChars())) + "/";
                    if (!Directory.Exists(customSongsPath))
                    {
                        Directory.CreateDirectory(customSongsPath);
                    }
                    zipStream = new MemoryStream(data);
                }
                catch (Exception e)
                {
                    yield break;
                }
                Task extract = ExtractZipAsync(songid, zipStream, customSongsPath);
                yield return new WaitWhile(() => !extract.IsCompleted);
                Loader.SongsLoadedEvent += OnSongsLoaded;
                Loader.Instance.RefreshSongs();
            }
        }

        private IEnumerator GetDownloadUrl(string songid)
        {
            string url = "https://api.beatsaver.com/maps/id/" + songid;
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", $"CustomCampaigns/v{Plugin.version}");

            var timeout = false;
            var time = 0f;

            var asyncRequest = www.SendWebRequest();

            while (!asyncRequest.isDone || asyncRequest.progress < 1f)
            {
                yield return null;

                time += Time.deltaTime;

                if (!(time >= 15f) || asyncRequest.progress != 0f) continue;
                www.Abort();
                timeout = true;
            }

            if (www.isNetworkError || www.isHttpError || timeout)
            {
                www.Abort();
                IsDownloading = false;
                onGetDownloadURLFail?.Invoke();
            }
            else
            {
                DetailResponse response = JsonConvert.DeserializeObject<DetailResponse>(www.downloadHandler.text);
                if (response != null && response.versions.Count > 0)
                {
                    this.beatSaverURL = response.versions[0].downloadURL;
                    this.beatSaverResponse = response;
                    onGetDownloadURL?.Invoke();
                }
                else
                {
                    onGetDownloadURLFail?.Invoke();
                }
                
            }
        }

        private async Task ExtractZipAsync(string songid, Stream zipStream, string customSongsPath)
        {
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                await Task.Run(() => archive.ExtractToDirectory(customSongsPath)).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
            }
            catch (Exception e)
            {
                return;
            }
        }
        private void OnSongsLoaded(Loader songLoader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> list)
        {
            Loader.SongsLoadedEvent -= OnSongsLoaded;
            IsDownloading = false;
            onFinish?.Invoke();
        }

        class DetailResponse
        {
            public DetailMetadata metadata;
            public List<MapVersion> versions;
        }

        class DetailMetadata
        {
            public string songName;
            public string levelAuthorName;
        }

        class MapVersion
        {
            public string downloadURL;
            public string coverURL;
        }
    }
}
