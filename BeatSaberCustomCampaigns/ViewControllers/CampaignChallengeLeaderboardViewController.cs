using BeatSaberCustomCampaigns.campaign;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaignLeaderboardLibrary;
using HMUI;
using IPA.Utilities;
using SongCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BeatSaberCustomCampaigns
{
    public class CampaignChallengeLeaderboardViewController : BSMLResourceViewController
    {
        public override string ResourceName => "BeatSaberCustomCampaigns.Views.challenge-leaderboard.bsml";

        [UIComponent("leaderboard")]
        internal LeaderboardTableView table;
        TextMeshProUGUI text;
        public Challenge lastClicked;
        
        public void UpdateLeaderboards()
        {
            if (lastClicked != null) StartCoroutine(CustomCampaignLeaderboard.LoadLeaderboards(table, lastClicked));
        }

        public IEnumerator UpdateLeaderboardsCoroutine()
        {
            if (lastClicked != null)
            {
                yield return StartCoroutine(CustomCampaignLeaderboard.LoadLeaderboards(table, lastClicked));
                CreateAccuracy();
            }
        }

        public void UpdateAccuracy()
        {
            CreateAccuracy();
        }

        private void CreateAccuracy()
        {
            if (table == null)
            {
                return;
            }
            if (lastClicked.FindSong() == null)
            {
                return;
            }
            var maxScore = GetMaxScore();
            if (maxScore <= 0)
            {
                return;
            }

            // Go through the table's cells to get the accuracies
            Dictionary<LeaderboardTableCell, Double> Accuracies = new Dictionary<LeaderboardTableCell, Double>();
            var numberOfCells = table.NumberOfCells();
            for (int row = 0; row < numberOfCells; row++)
            {
                var tableView = table.GetField<TableView, LeaderboardTableView>("_tableView");
                LeaderboardTableCell tableCell = (LeaderboardTableCell)table.CellForIdx(tableView, row);
                TextMeshProUGUI scoreText = tableCell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                try
                {
                    int score = GetScoreFromScoreText(scoreText.text);
                    Double acc = Math.Round((double)score / (double)maxScore * 100, 2);
                    Accuracies.Add(tableCell, acc);
                }

                catch (Exception e)
                {
                }

            }

            // Changing the score text in the above cells doesn't work; find the duplicate table cells instead and change those ones
            foreach (LeaderboardTableCell cell in Resources.FindObjectsOfTypeAll<LeaderboardTableCell>())
            {
                var score = cell.GetField<TextMeshProUGUI, LeaderboardTableCell>("_scoreText");
                try
                {
                    if (cell.name.Equals("LeaderboardTableCell(Clone)"))
                    {
                        var acc = GetAccFromCell(Accuracies, cell);
                        if (acc >= 0)
                        {
                            score.text += $" (<color=#FFD42A>{acc}%</color>)";
                        }
                        
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private int GetMaxScore()
        {
            var id = lastClicked.FindSong().levelID;
            var beatmapLevel = Loader.BeatmapLevelsModelSO.GetBeatmapLevelIfLoaded(id);
            if (beatmapLevel == null)
            {
                return 0;
            }

            var levelDifficultyBeatmapSets = beatmapLevel.beatmapLevelData.difficultyBeatmapSets;
            var missionData = lastClicked.GetMissionData(new Campaign()); // campaign doesn't matter here
            var levelDifficultyBeatmaps = levelDifficultyBeatmapSets.First(x => x.beatmapCharacteristic.Equals(missionData.beatmapCharacteristic)).difficultyBeatmaps;
            foreach (var diff in levelDifficultyBeatmaps)
            {
                if (diff.difficulty.Equals(lastClicked.difficulty))
                {
                    var noteCount = GetTrueNoteCount(diff.beatmapData, beatmapLevel.beatmapLevelData.audioClip.length);
                    return ScoreModel.MaxRawScoreForNumberOfNotes(noteCount);
                }
            }

            return 0;
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

        private int GetScoreFromScoreText(string score)
        {
            var noSpace = score.Replace(" ", String.Empty);
            return Int32.Parse(noSpace);
        }

        private double GetAccFromCell(Dictionary<LeaderboardTableCell, Double> dict, LeaderboardTableCell tableCell)
        {
            foreach (var leaderboardTableCell in dict.Keys)
            {
                if (SameTableCell(leaderboardTableCell, tableCell))
                {
                    return dict[leaderboardTableCell];
                }
            }
            return -1;
        }

        private bool SameTableCell(LeaderboardTableCell tableCellA, LeaderboardTableCell tableCellB)
        {
            String tableCellARank = tableCellA.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText").text;
            String tableCellAPlayer = tableCellA.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText").text;
            String tableCellBRank = tableCellB.GetField<TextMeshProUGUI, LeaderboardTableCell>("_rankText").text;
            String tableCellBPlayer = tableCellB.GetField<TextMeshProUGUI, LeaderboardTableCell>("_playerNameText").text;

            return tableCellAPlayer == tableCellBPlayer && tableCellARank == tableCellBRank;
        }
    }
}