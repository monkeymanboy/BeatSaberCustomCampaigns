using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using CustomCampaigns.Utils;
using HMUI;
using IPA.Utilities;
using SongCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

        public Mission mission;

        public event PropertyChangedEventHandler PropertyChanged;

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
        }

        public void UpdateLeaderboards()
        {
            if (mission != null)
            {
                isLoaded = false;
                StartCoroutine(UpdateLeaderboardsCoroutine());
            }
        }

        private IEnumerator UpdateLeaderboardsCoroutine()
        {
            if (mission != null)
            {
                var hash = CustomCampaignLeaderboardLibraryUtils.GetHash(mission);
                Task<LeaderboardResponse> task = CustomCampaignLeaderboard.LoadLeaderboards(UserInfoManager.UserInfo.platformUserId, hash);
                yield return new WaitUntil(() => task.IsCompleted);
                LeaderboardResponse response = task.Result;

                UpdateScores(response);
                isLoaded = true;
            }
        }

        private void UpdateScores(LeaderboardResponse response)
        {
            List<ScoreData> scores = new List<ScoreData>();
            int specialPos = -1;

            var maxScore = GetMaxScore();
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
            Double acc = Math.Round((double) score.score / (double) maxScore * 100, 2);

            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(score.id, score.name);
            name = $"{name} - <size=75%> (<color=#FFD42A>{acc}%</color>)</size>";

            return new ScoreData(score.score, name, rank, false);
        }

        private ScoreData GetScoreData(YourData yourData, int maxScore)
        {
            Double acc = Math.Round((double) yourData.score / (double) maxScore * 100, 2);

            var id = UserInfoManager.UserInfo.platformUserId;
            var username = UserInfoManager.UserInfo.userName;

            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(id, username);
            name = $"{name} - <size=75%>(<color=#FFD42A>{acc}%</color>)</size>";

            return new ScoreData(yourData.score, name, yourData.position, false);
        }

        private int GetMaxScore()
        {
            var levelId = mission.FindSong().levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(levelId);
            if (beatmapLevel == null)
            {
                return 0;
            }
            var missionData = mission.GetMissionData(null); // campaign doesn't matter here

            IDifficultyBeatmap beatmapDifficulty = BeatmapUtils.GetMatchingBeatmapDifficulty(levelId, missionData.beatmapCharacteristic, mission.difficulty);
            if (beatmapDifficulty == null)
            {
                return 0;
            }
            var noteCount = GetTrueNoteCount(beatmapDifficulty.beatmapData, beatmapLevel.beatmapLevelData.audioClip.length);
            return ScoreModel.MaxRawScoreForNumberOfNotes(noteCount);
        }

        private int GetTrueNoteCount(BeatmapData beatmapData, float length)
        {
            int noteCount = 0;
            foreach (var beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                if (beatmapObjectData is NoteData note && note.colorType != ColorType.None && note.time <= length)
                {
                    noteCount++;
                }
            }
            return noteCount;
        }
    }
}
