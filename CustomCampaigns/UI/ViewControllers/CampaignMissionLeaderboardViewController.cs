using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
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
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static LeaderboardTableView;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.mission-leaderboard.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\mission-leaderboard.bsml")]
    public class CampaignMissionLeaderboardViewController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
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
            Plugin.logger.Debug("destroying...");
            GameObject.Destroy(_leaderboardTransform.Find("LoadingControl").gameObject);

            scoreMultiplierCounter = new ScoreMultiplierCounter();
        }

        public void UpdateLeaderboards()
        {
            Plugin.logger.Debug("update leaderboards");
            if (mission != null && _leaderboardTransform != null)
            {
                Plugin.logger.Debug("updating leaderboard");
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
                    Plugin.logger.Debug("normal leaderboard");
                    var hash = CustomCampaignLeaderboardLibraryUtils.GetHash(mission);
                    task = CustomCampaignLeaderboard.LoadLeaderboards(UserInfoManager.UserInfo.platformUserId, hash);
                }

                else
                {
                    Plugin.logger.Debug("custom leaderboard");
                    string url = GetURL(mission, customURL);
                    task = CustomCampaignLeaderboard.LoadLeaderboards(url);
                }

                yield return new WaitUntil(() => task.IsCompleted);
                LeaderboardResponse response = task.Result;

                UpdateScores(response);
                isLoaded = true;

            }
        }

        private string GetURL(Mission mission, string customURL)
        {
            string url = customURL.Replace("{missionHash}", CustomCampaignLeaderboardLibraryUtils.GetHash(mission))
                                  .Replace("{mapHash}", mission.hash)
                                  .Replace("{characteristic}", mission.characteristic)
                                  .Replace("{difficulty}", ((int) mission.difficulty).ToString())
                                  .Replace("{userID}", UserInfoManager.UserInfo.platformUserId);
            return url;
        }

        private async void UpdateScores(LeaderboardResponse response)
        {
            List<ScoreData> scores = new List<ScoreData>();
            int specialPos = -1;

            var maxScore = await GetMaxScore();
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
                scores.Add(new ScoreData(0, "No scores for this challenge", 0, false));
            }

            table.SetScores(scores, specialPos);
            foreach (TextMeshProUGUI textMeshPro in table.GetComponentsInChildren<TextMeshProUGUI>())
            {
                textMeshPro.richText = true;
            }
        }

        private ScoreData GetScoreData(OtherData score, int maxScore, int rank)
        {
            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(score.id, score.name);
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

            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(id, username);

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
                return 0;
            }
            var levelId = level.levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(levelId);
            if (beatmapLevel == null)
            {
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


            IDifficultyBeatmap difficultyBeatmap = BeatmapUtils.GetMatchingBeatmapDifficulty(levelId, missionData.beatmapCharacteristic, mission.difficulty);
            if (difficultyBeatmap == null)
            {
                return 0;
            }

            // TODO: Get beatmap version to determine if simple or complex method
            return await GetMaxScoreBeatmapVThree(difficultyBeatmap);
        }

        private async Task<int> GetMaxScoreBeatmapVThree(IDifficultyBeatmap difficultyBeatmap)
        {
            CustomDifficultyBeatmap customDifficultyBeatmap = difficultyBeatmap as CustomDifficultyBeatmap;

            if (customDifficultyBeatmap == null)
            {
                Plugin.logger.Error("difficulty beatmap was not a custom beatmap??????!111");
                return 0;
            }

            IReadonlyBeatmapData beatmapData = null;
            await Task.Run(delegate ()
            {
                beatmapData = BeatmapDataLoader.GetBeatmapDataFromSaveData(customDifficultyBeatmap.beatmapSaveData, customDifficultyBeatmap.difficulty, customDifficultyBeatmap.level.beatsPerMinute, false, null, null);
            });

            IEnumerable<NoteData> beatmapDataItems = beatmapData.GetBeatmapDataItems<NoteData>(0);
            IEnumerable<SliderData> beatmapDataItems2 = beatmapData.GetBeatmapDataItems<SliderData>(0);

            Type maxScoreCounterElementType = typeof(ScoreModel).GetNestedType("MaxScoreCounterElement", System.Reflection.BindingFlags.NonPublic);
            ConstructorInfo maxScoreCounterConstructorInfo = maxScoreCounterElementType.GetConstructor(AccessTools.all, null, new Type[] { typeof(NoteData.ScoringType), typeof(float) }, null);
            List<object> elements = new List<object>(1000);


            foreach (NoteData noteData in beatmapDataItems)
            {
                if (noteData.scoringType != NoteData.ScoringType.Ignore && noteData.gameplayType != NoteData.GameplayType.Bomb)
                {
                    elements.Add(maxScoreCounterConstructorInfo?.Invoke(new object[] { noteData.scoringType, noteData.time }));
                }
            }

            foreach (SliderData sliderData in beatmapDataItems2)
            {
                if (sliderData.sliderType == SliderData.Type.Burst)
                {
                    elements.Add(maxScoreCounterConstructorInfo?.Invoke(new object[] { NoteData.ScoringType.BurstSliderHead, sliderData.tailTime }));
                    for (int i = 1; i < sliderData.sliceCount; i++)
                    {
                        float t = (float) i / (float) (sliderData.sliceCount - 1);
                        elements.Add(maxScoreCounterConstructorInfo?.Invoke(new object[] { NoteData.ScoringType.BurstSliderElement, Mathf.LerpUnclamped(sliderData.time, sliderData.tailTime, t) }));
                    }
                }
            }
            elements.Sort();
            int maxScore = 0;

            scoreMultiplierCounter.Reset();

            PropertyInfo noteScoreDefinitionProperty = maxScoreCounterElementType.GetProperty("noteScoreDefinition", AccessTools.all);
            PropertyInfo maxCutScoreProperty = typeof(ScoreModel.NoteScoreDefinition).GetProperty("maxCutScore", AccessTools.all);

            foreach (object maxScoreCounterElement in elements)
            {
                scoreMultiplierCounter.ProcessMultiplierEvent(ScoreMultiplierCounter.MultiplierEventType.Positive);
                object noteScoreDefinition = noteScoreDefinitionProperty?.GetValue(maxScoreCounterElement);
                int maxCutScore = (int?)maxCutScoreProperty?.GetValue(noteScoreDefinition) ?? 0;

                maxScore += maxCutScore * scoreMultiplierCounter.multiplier;
            }

            return maxScore;
        }
    }
}
