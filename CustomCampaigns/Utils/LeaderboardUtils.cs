using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomCampaigns.Utils
{

    public class LeaderboardResponse
    {
        public List<OtherData> scores;
        public YourData you;
    }

    public class OtherData
    {
        public string name;
        public string id;
        public int score;
        public int count = 0;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            name = Regex.Replace(name, "<[^>]*(>|$)", "");
        }
    }

    public class YourData
    {
        public int position;
        public int score;
        public int count = 0;
    }

    public struct SubmissionData
    {
        public string id;
        public string challengeHash;
        public int score;
        public string user;
        public string hash;
    }

    public class CustomCampaignLeaderboard
    {
        private static string USER_AGENT = $"CustomCampaigns/v{Plugin.version}";
        private static Color[] RainbowArray;

        public static async Task<LeaderboardResponse> LoadLeaderboards(string url)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SetRequestHeader("User-Agent", USER_AGENT);

            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Plugin.logger.Debug($"Error loading leaderboard: {www.error}");
            }
            else
            {
                LeaderboardResponse response = JsonConvert.DeserializeObject<LeaderboardResponse>(www.downloadHandler.text);
                return response;
            }

            return null;
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

        public static string GetHash(Mission mission)
        {
            return GetHash(mission.rawJSON);
        }


        public static string GetURL(Mission mission, string customURL)
        {
            string url = customURL.Replace("{missionHash}", GetHash(mission))
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
}
