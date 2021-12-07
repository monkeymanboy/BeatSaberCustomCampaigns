using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Managers;
using CustomCampaigns.Utils;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static LeaderboardTableView;

namespace CustomCampaigns.UI.ViewControllers
{
    [ViewDefinition("CustomCampaigns.UI.Views.campaign-leaderboard.bsml")]
    [HotReload(RelativePathToLayout = @"..\Views\campaign-leaderboard.bsml")]
    public class CampaignTotalLeaderboardViewController : BSMLAutomaticViewController, INotifyPropertyChanged
    {
        [UIComponent("leaderboard")]
        internal LeaderboardTableView table;

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

        public string campaignID = "";

        public void UpdateLeaderboards()
        {
            isLoaded = false;
            StartCoroutine(UpdateLeaderboardCoroutine());
        }

        public IEnumerator UpdateLeaderboardCoroutine()
        {
            Task<LeaderboardResponse> task = CustomCampaignLeaderboard.LoadTotalLeaderboards(UserInfoManager.UserInfo.platformUserId, campaignID);
            yield return new WaitUntil(() => task.IsCompleted);
            LeaderboardResponse response = task.Result;

            UpdateScores(response);
            isLoaded = true;
        }

        private void UpdateScores(LeaderboardResponse response)
        {
            if (response == null)
            {
                List<ScoreData> scores = new List<ScoreData>();
                scores.Add(new ScoreData(0, "This campaign is not configured for", 0, false));
                scores.Add(new ScoreData(0, "total score leaderboards", 1, false));
                table.SetScores(scores, -1);
                return;
            }

            else
            {
                try
                {
                    List<ScoreData> scores = new List<ScoreData>();
                    int specialPos = -1;

                    foreach (OtherData score in response.scores)
                    {
                        if (score.id == UserInfoManager.UserInfo.platformUserId + "")
                        {
                            specialPos = scores.Count;
                        }

                        scores.Add(GetScoreData(score, scores.Count + 1));
                    }

                    if (response.you.position > 0 && specialPos == -1)
                    {
                        specialPos = scores.Count;

                        scores.Add(GetScoreData(response.you));
                    }

                    if (scores.Count == 0)
                    {
                        scores.Add(new ScoreData(0, "No scores for this campaign", 0, false));
                    }

                    table.SetScores(scores, specialPos);
                    foreach (TextMeshProUGUI mesh in table.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        mesh.richText = true;
                    }

                }
                catch
                {
                    table.SetScores(new ScoreData[] { new ScoreData(0, "Error loading global scores", 0, false) }.ToList(), 0);
                }
            }
        }

        private ScoreData GetScoreData(OtherData score, int rank)
        {
            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(score.id, score.name);
            name = $"{name}<size=50%> (Completed - {score.count})</size>";

            return new ScoreData(score.score, name, rank, false);
        }

        private ScoreData GetScoreData(YourData yourData)
        {
            var id = UserInfoManager.UserInfo.platformUserId;
            var username = UserInfoManager.UserInfo.userName;

            var name = CustomCampaignLeaderboardLibraryUtils.GetSpecialName(id, username);
            name = $"{name}<size=50%> (Completed - {yourData.count})</size>";

            return new ScoreData(yourData.score, name, yourData.position, false);
        }
    }
}
