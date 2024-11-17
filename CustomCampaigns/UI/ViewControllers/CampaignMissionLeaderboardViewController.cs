using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using CustomCampaigns.Utils;
using HarmonyLib;
using SongCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;
using static LeaderboardTableView;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.mission-leaderboard.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\mission-leaderboard.bsml")]
    public class CampaignMissionLeaderboardViewController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        [Inject]
        private BeatmapDataLoader _beatmapDataLoader;
        [Inject]
        private BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;

        [UIComponent("leaderboard")]
        internal LeaderboardTableView table;
        [UIComponent("leaderboard")]
        protected readonly Transform _leaderboardTransform;

        private ScoreMultiplierCounter scoreMultiplierCounter;

        public Mission mission;
        public string customURL = "";

        public new event PropertyChangedEventHandler PropertyChanged;

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

        private bool _isLoaded = true;

        [UIAction("#post-parse")]
        public void PostParse()
        {
            GameObject.Destroy(_leaderboardTransform.Find("LoadingControl").gameObject);

            scoreMultiplierCounter = new ScoreMultiplierCounter();
        }

        public void UpdateLeaderboards()
        {
            if (mission != null && _leaderboardTransform != null)
            {
                isLoaded = false;
                StartCoroutine(UpdateLeaderboardsCoroutine());
            }
        }

        private IEnumerator UpdateLeaderboardsCoroutine()
        {
            if (mission != null)
            {
                Task<LeaderboardResponse> task;
                if (customURL == "")
                {
                    // Bsaber killed custom campaign leaderboards :(
                    //var hash = CustomCampaignLeaderboardLibraryUtils.GetHash(mission);
                    //task = CustomCampaignLeaderboard.LoadLeaderboards(UserInfoManager.UserInfo.platformUserId, hash);

                    List<ScoreData> scores = new List<ScoreData>();
                    scores.Add(new ScoreData(0, "Leaderboards are unavailable for this campaign", 0, false));
                    table.SetScores(scores, -1);
                    isLoaded = true;
                    yield break;
                }

                else
                {
                    string url = CustomCampaignLeaderboard.GetURL(mission, customURL);
                    task = CustomCampaignLeaderboard.LoadLeaderboards(url);
                }

                yield return new WaitUntil(() => task.IsCompleted);
                LeaderboardResponse response = task.Result;

                UpdateScores(response);
                isLoaded = true;

            }
        }

        private async void UpdateScores(LeaderboardResponse response)
        {
            List<ScoreData> scores = new List<ScoreData>();
            int specialPos = -1;

            var maxScore = await GetMaxScore();

            if (response is null)
            {
                scores.Add(new ScoreData(0, "Unable to find scores", 0, false));
            }
            else
            {
                foreach (OtherData score in response.scores)
                {
                    if (score.id == UserInfoManager.UserInfo.platformUserId + "")
                    {
                        specialPos = scores.Count;
                    }

                    scores.Add(GetScoreData(score, maxScore, scores.Count + 1));
                }

                if (response.you.position > 0 && specialPos == -1)
                {
                    specialPos = scores.Count;
                    scores.Add(GetScoreData(response.you, maxScore));
                }

                if (scores.Count == 0)
                {
                    scores.Add(new ScoreData(0, "No scores for this mission", 0, false));
                }
            }

            table.SetScores(scores, specialPos);
            foreach (TextMeshProUGUI textMeshPro in table.GetComponentsInChildren<TextMeshProUGUI>())
            {
                textMeshPro.richText = true;
            }
        }

        private ScoreData GetScoreData(OtherData score, int maxScore, int rank)
        {
            var name = CustomCampaignLeaderboard.GetSpecialName(score.id, score.name);
            if (maxScore > 0)
            {
                
                Double acc = Math.Round((double) score.score / (double) maxScore * 100, 2);
                name = $"{name} - <size=75%> (<color=#FFD42A>{acc}%</color>)</size>";
            }

            return new ScoreData(score.score, name, rank, false);
        }

        private ScoreData GetScoreData(YourData yourData, int maxScore)
        {
            var id = UserInfoManager.UserInfo.platformUserId;
            var username = UserInfoManager.UserInfo.userName;

            var name = CustomCampaignLeaderboard.GetSpecialName(id, username);

            if (maxScore > 0)
            {
                Double acc = Math.Round((double) yourData.score / (double) maxScore * 100, 2);
                name = $"{name} - <size=75%>(<color=#FFD42A>{acc}%</color>)</size>";
            }

            return new ScoreData(yourData.score, name, yourData.position, false);
        }

        private async Task<int> GetMaxScore()
        {
            var level = mission.FindSong();
            if (level == null)
            {
                Plugin.logger.Debug("level null");
                return 0;
            }
            var levelId = level.levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevel(levelId);
            if (beatmapLevel == null)
            {
                Plugin.logger.Debug("beatmaplevel null");
                return 0;
            }

            MissionDataSO missionData;
            try
            {
                missionData = mission.TryGetMissionData();
            }
            catch
            {
                Plugin.logger.Error("Tried to get mission data before it was set! Please report this to the mod dev.");
                return 0;
            }

            if (!(missionData is CustomMissionDataSO))
            {
                Plugin.logger.Debug("mission data is not custom");
                return 0;
            }

            if (_beatmapLevelsEntitlementModel is null)
            {
                Plugin.logger.Debug("_beatmapLevelsEntitlementModel null");
                return 0;
            }

            if (_beatmapDataLoader is null)
            {
                Plugin.logger.Debug("_beatmapDataLoader null");
                return 0;
            }

            BeatmapLevelDataVersion beatmapLevelDataVersion = (await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(levelId, CancellationToken.None));
            BeatmapKey beatmapKey = BeatmapUtils.GetMatchingBeatmapKey(levelId, missionData.beatmapCharacteristic, mission.difficulty);
            return ScoreModel.ComputeMaxMultipliedScoreForBeatmap(await _beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData: (missionData as CustomMissionDataSO).beatmapLevelData,
                                                                                                                beatmapKey: beatmapKey,
                                                                                                                startBpm: beatmapLevel.beatsPerMinute,
                                                                                                                loadingForDesignatedEnvironment: false,
                                                                                                                targetEnvironmentInfo: null,
                                                                                                                originalEnvironmentInfo: null,
                                                                                                                beatmapLevelDataVersion,
                                                                                                                gameplayModifiers: null,
                                                                                                                playerSpecificSettings: null,
                                                                                                                enableBeatmapDataCaching: false));
        }
    }
}
