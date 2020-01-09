using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections;
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

        public void DownloadSong(string songid, string url, bool isBeatSaver, Action onFinish = null, Action onFail = null)
        {
            if (IsDownloading)
            {
                onFail?.Invoke();
                return;
            }
            this.onFail = onFail;
            this.onFinish = onFinish;
            IsDownloading = true;
            StartCoroutine(DownloadSongCoroutine(songid, url, isBeatSaver));
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
                    var www2 = UnityWebRequest.Get("https://beatsaver.com/api/maps/detail/" + songid);
                    www2.SetRequestHeader("User-Agent", $"CustomCampaigns/v{Plugin.version}");

                    var timeout2 = false;
                    var time2 = 0f;

                    var asyncRequest2 = www2.SendWebRequest();

                    while (!asyncRequest2.isDone || asyncRequest2.progress < 1f)
                    {
                        yield return null;

                        time2 += Time.deltaTime;

                        if (!(time2 >= 15f) || asyncRequest2.progress != 0f) continue;
                        www2.Abort();
                        timeout2 = true;
                    }

                    if (www2.isNetworkError || www2.isHttpError || timeout2)
                    {
                        www2.Abort();
                    }
                    else
                    {
                        DetailResponse response = JsonConvert.DeserializeObject<DetailResponse>(www2.downloadHandler.text);
                        extra = " (" + response.metadata.songName + " - " + response.metadata.levelAuthorName + ")";
                    }
                }

                Stream zipStream = null;
                try
                {
                    docPath = Application.dataPath;
                    docPath = docPath.Substring(0, docPath.Length - 5);
                    docPath = docPath.Substring(0, docPath.LastIndexOf("/"));
                    customSongsPath = docPath + "/Beat Saber_Data/CustomLevels/" + songid + extra + "/";
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
        private void OnSongsLoaded(Loader songLoader, Dictionary<string, CustomPreviewBeatmapLevel> list)
        {
            Loader.SongsLoadedEvent -= OnSongsLoaded;
            IsDownloading = false;
            onFinish?.Invoke();
        }

        class DetailResponse
        {
            public DetailMetadata metadata;
        }
        class DetailMetadata
        {
            public string songName;
            public string levelAuthorName;
        }
    }
}
