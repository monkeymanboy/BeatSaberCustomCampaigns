using CustomCampaignLeaderboardLibrary;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using Newtonsoft.Json;
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
        private static Color[] RainbowArray;

        public static async void SubmitScoreAsync(Mission mission, MissionCompletionResults missionCompletionResults)
        {
            SubmissionData submissionData = new SubmissionData();
            submissionData.challengeHash = GetHash(mission);
            submissionData.score = missionCompletionResults.levelCompletionResults.multipliedScore;
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

        public static string GetURL(Mission mission, string customURL)
        {
            string url = customURL.Replace("{missionHash}", CustomCampaignLeaderboardLibraryUtils.GetHash(mission))
                                  .Replace("{mapHash}", mission.hash)
                                  .Replace("{characteristic}", mission.characteristic)
                                  .Replace("{difficulty}", ((int) mission.difficulty).ToString())
                                  .Replace("{userID}", UserInfoManager.UserInfo.platformUserId);
            return url;
        }

        public static string GetSpecialName(string id, string name)
        {
            if (IsRainbowName(id))
            {
                return RainbowifyName(name);
            }
            else
            {
                var specialColor = GetSpecialPlayerColor(id);

                if (specialColor != "")
                {
                    return $"<size=90%><color=#{specialColor}>{name}</color></size>";
                }
                else
                {
                    return $"<size=90%>{name}</size>";
                }
            }
        }

        private static string RainbowifyName(string name)
        {
            if (RainbowArray == null)
            {
                InitializeRainbowArray();
            }

            var sb = new StringBuilder();

            int i = 0;
            foreach (char c in name)
            {
                if (i >= RainbowArray.Length)
                {
                    i %= RainbowArray.Length;
                }

                var color = RainbowArray[i];
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{c}</color>");
                i++;
            }

            return sb.ToString();
        }

        private static void InitializeRainbowArray()
        {
            RainbowArray = new Color[] { Color.red,
                                         new Color(1, 0.647f, 0), // Orange
                                         Color.yellow,
                                         Color.green,
                                         Color.blue,
                                         new Color(0.29f, 0, 0.51f), // Indigo
                                         new Color(0.56f, 0, 1f) }; // Violet
        }

        public static string GetSpecialPlayerColor(string userID)
        {
            switch (userID)
            {
                case "76561198012241978": // Pulse
                    return "12CCC2";
                case "76561198059398643": // monkey
                    return "82560b";
                default:
                    return "";
            }
        }

        private static bool IsRainbowName(string id)
        {
            return id == "76561198182060577"; // :)
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
