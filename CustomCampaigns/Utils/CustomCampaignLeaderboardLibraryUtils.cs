using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using IPA.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomCampaigns.Utils
{
    public static class CustomCampaignLeaderboardLibraryUtils
    {
        public static async void SubmitScoreAsync(Mission mission, MissionCompletionResults missionCompletionResults)
        {
            SubmissionData submissionData = new SubmissionData();
            submissionData.challengeHash = GetHash(mission);
            submissionData.score = missionCompletionResults.levelCompletionResults.rawScore;
            submissionData.id = UserInfoManager.UserInfo.platformUserId;
            submissionData.user = UserInfoManager.UserInfo.userName + "";
            submissionData.hash = GetHash(submissionData.id + submissionData.user + submissionData.score + submissionData.challengeHash);

            await CustomCampaignLeaderboard.SubmitScore(submissionData);
        }

        public static string GetHash(Mission mission)
        {
            return GetHash(mission.rawJSON);
        }

        public static string GetHash(string s)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(s + "%");
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string GetSpecialPlayerColor(string userID)
        {
            switch (userID)
            {
                case "76561198012241978":
                    return "12CCC2";
                case "76561198059398643":
                    return "82560b";
                default:
                    return "";
            }
        }
    }

    class CompleteSubmission
    {
        public string userID;
        public string challengeHash;
        public int score;
        public List<Requirement> requirements = new List<Requirement>();

        internal IEnumerator Submit(string completionPost)
        {
            UnityWebRequest www = UnityWebRequest.Post(completionPost, JsonConvert.SerializeObject(this));
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
        }
    }
    class Requirement
    {
        public string name;
        public int value;
    }
}
