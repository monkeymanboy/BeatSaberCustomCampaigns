using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace CustomCampaigns
{
    public class Downloader
    {
        private const string BEATSAVER_PREFIX = "https://api.beatsaver.com/maps/";
        private const string DOWNLOAD_URL_TEMPLATE = "https://cdn.beatsaver.com/<hash>.zip";
        private const string ID_ENDPOINT = "id/";

        private static string UserAgent = "";

        private static bool isDownloading = false;

        public Downloader()
        {
            UserAgent = $"CustomCampaigns/v{Plugin.version}";
        }

        public async Task DownloadMapByHashAsync(string hash, string id, CancellationToken cancellationToken, Action onDownloadSuccess, Action onDownloadFail, Action<float> progressCallback = null, Action<string> statusCallback = null)
        {
            if (isDownloading)
            {
                Plugin.logger.Debug("Already currently downloading a map");
                onDownloadFail?.Invoke();
                return;
            }

            string downloadUrl = DOWNLOAD_URL_TEMPLATE.Replace("<hash>", hash.ToLower());
            statusCallback?.Invoke("Getting Map Info...");
            DetailResponse detailResponse = await GetMapInformationAsync(id, cancellationToken, progressCallback);
            if (detailResponse == null || detailResponse.versions.Count == 0)
            {
                onDownloadFail?.Invoke();
                return;
            }

            string path = GetMapPath(id, detailResponse);
            statusCallback?.Invoke("Downloading map...");
            await DownloadMapFromUrlAsync(downloadUrl, path, cancellationToken, onDownloadSuccess, onDownloadFail, progressCallback, statusCallback);

        }

        public async Task DownloadMapByIDAsync(string id, CancellationToken cancellationToken, Action onDownloadSuccess, Action onDownloadFail, Action<float> progressCallback = null, Action<string> statusCallback = null)
        {
            if (isDownloading)
            {
                Plugin.logger.Debug("Already currently downloading a map");
                onDownloadFail?.Invoke();
                return;
            }

            statusCallback?.Invoke("Getting Map Info...");
            DetailResponse detailResponse = await GetMapInformationAsync(id, cancellationToken, progressCallback);
            if (detailResponse == null || detailResponse.versions.Count == 0)
            {
                onDownloadFail?.Invoke();
                return;
            }

            string downloadUrl = GetDownloadUrlFromDetailResponse(detailResponse);
            if (downloadUrl == null)
            {
                Plugin.logger.Error("Download URL was somehow null - this should never happen");
                onDownloadFail?.Invoke();
                return;
            }

            string path = GetMapPath(id, detailResponse);
            statusCallback?.Invoke("Downloading map...");
            await DownloadMapFromUrlAsync(downloadUrl, path, cancellationToken, onDownloadSuccess, onDownloadFail, progressCallback, statusCallback);
        }

        internal async Task DownloadMapFromUrlAsync(string downloadUrl, string path, CancellationToken cancellationToken, Action onDownloadSuccess, Action onDownloadFail, Action<float> progressCallback = null, Action<string> statusCallback = null)
        {
            if (isDownloading)
            {
                Plugin.logger.Debug("Already currently downloading a map");
                onDownloadFail?.Invoke();
                return;
            }

            statusCallback?.Invoke("Downloading map...");

            var www = await MakeRequestAsync(downloadUrl, cancellationToken, progressCallback);
            if (www == null || www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                onDownloadFail?.Invoke();
                return;
            }

            statusCallback?.Invoke("Finding folder name...");
            path = DeterminePathNumber(path, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Stream zipStream = new MemoryStream(www.downloadHandler.data);
            statusCallback?.Invoke("Extracting zip...");
            await ExtractZipAsync(zipStream, path, onDownloadSuccess, onDownloadFail);
        }

        internal string DeterminePathNumber(string path, CancellationToken cancellationToken)
        {
            int c = 1;
            while (Directory.Exists(path))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (c == 1)
                {
                    path += $" ({c})";
                }
                path.Replace($"({c - 1})", $"({c})");
            }
            return path;
        }

        private string GetDownloadUrlFromDetailResponse(DetailResponse detailResponse)
        {
            if (detailResponse.versions.Count == 0)
            {
                return null;
            }
            return detailResponse.versions[0].downloadURL;
        }

        private string GetMapPath(string id, DetailResponse detailResponse)
        {
            var path = CustomLevelPathHelper.customLevelsDirectoryPath;
            string folderName = id + " (" + detailResponse.metadata.songName + " - " + detailResponse.metadata.levelAuthorName + ")";

            return Path.Combine(path, folderName);
        }

        private async Task<DetailResponse> GetMapInformationAsync(string id, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            var url = BEATSAVER_PREFIX + ID_ENDPOINT + id.ToLower();
            var www = await MakeRequestAsync(url, cancellationToken, progressCallback);

            if (www == null || www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<DetailResponse>(www.downloadHandler.text);
        }

        private async Task<UnityWebRequest> MakeRequestAsync(string url, CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            isDownloading = true;
            Plugin.logger.Debug($"Making request to url: {url}");
            var www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", UserAgent);
            www.timeout = 15;

            www.SendWebRequest();
            while (!www.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Plugin.logger.Debug("cancellation requested");
                    www.Abort();
                    isDownloading = false;
                    return null;
                }
                progressCallback?.Invoke(www.downloadProgress);
                await Task.Yield();
            }

            isDownloading = false;
            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Plugin.logger.Debug(www.error);

                return null;
            }

            return www;
        }

        private async Task ExtractZipAsync(Stream zipStream, string path, Action onExtractSuccess, Action onExtractFail)
        {
            try
            {
                ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                await Task.Run(() => archive.ExtractToDirectory(path)).ConfigureAwait(false);
                archive.Dispose();
                zipStream.Close();
            }
            catch (Exception e)
            {
                Plugin.logger.Error(e.Message);
                onExtractFail?.Invoke();
                return;
            }
            onExtractSuccess?.Invoke();
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
