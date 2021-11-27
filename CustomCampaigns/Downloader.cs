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
        private const string ID_ENDPOINT = "id/";

        private const string KNOWN_ID = "ff9";
        private const string KNOWN_HASH = "cb9f1581ff6c09130c991db8823c5953c660688f";

        private static string UserAgent = "";
        private static string DownloadUrlTemplate = "";

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

            if (DownloadUrlTemplate == "")
            {
                Plugin.logger.Debug("no download url template");
                statusCallback?.Invoke("Getting Download Url...");
                await GetDownloadUrlTemplateAsync(cancellationToken, progressCallback);
                // it failed...
                if (DownloadUrlTemplate == "")
                {
                    Plugin.logger.Debug("failed to retrieve template");
                    onDownloadFail?.Invoke();
                    return;
                }
            }

            string downloadUrl = DownloadUrlTemplate.Replace(KNOWN_HASH, hash.ToLower());
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

        private async Task GetDownloadUrlTemplateAsync(CancellationToken cancellationToken, Action<float> progressCallback = null)
        {
            DetailResponse detailResponse = await GetMapInformationAsync(KNOWN_ID, cancellationToken, progressCallback);
            if (detailResponse == null || detailResponse.versions.Count == 0)
            {
                Plugin.logger.Error($"Was unable to retrieve download url template");
                if (detailResponse == null)
                {
                    Plugin.logger.Error("detail response was null");
                }
                else
                {
                    Plugin.logger.Error("not enough versions in detailresponse");
                }
                return;
            }

            DownloadUrlTemplate = detailResponse.versions[0].downloadURL;
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
            if (www == null || www.isNetworkError || www.isHttpError)
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

            if (www == null || www.isNetworkError || www.isHttpError)
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
            if (www.isNetworkError || www.isHttpError)
            {
                if (www.isNetworkError)
                {
                    Plugin.logger.Debug("network error");
                }
                else
                {
                    Plugin.logger.Debug("http error");
                }

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
