using CustomCampaigns.Campaign.Missions;
using IPA.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// WHY IN THE ACTUAL FUCK DOES THIS CIRCULAR DEPENDENCY EXIST
namespace BeatSaberCustomCampaigns
{
    public class Challenge
    {
        public string name;
        public string songid;
        public string hash = "";
        public string customDownloadURL = "";
        public string characteristic = "Standard";
        public BeatmapDifficulty difficulty;
        public MissionModifiers modifiers;
        public MissionRequirement[] requirements;
        public Dictionary<string, string[]> externalModifiers = new Dictionary<string, string[]>();
        public MissionInfo missionInfo = null;

        //public List<UnlockableItem> unlockableItems = new List<UnlockableItem>();

        public bool unlockMap = false;
        public bool allowStandardLevel = false;

        [JsonIgnore]
        private Mission _mission;

        [JsonIgnore]
        public string rawJSON;

        public Challenge(Mission mission)
        {
            // should hopefully be enough?
            name = mission.name;
            songid = mission.songid;
            hash = mission.hash;
            customDownloadURL = mission.customDownloadURL;
            characteristic = mission.characteristic;
            difficulty = mission.difficulty;
            modifiers = mission.modifiers;
            requirements = mission.requirements;
            externalModifiers = mission.externalModifiers;
            missionInfo = mission.missionInfo;

            _mission = mission;
            unlockMap = mission.unlockMap;
            rawJSON = mission.rawJSON;
        }
        public string GetHash()
        {
            return APITools.GetHash(rawJSON);
        }
    }

    public class APITools
    {
        public static string LeaderboardServer = "https://bsaber.com";
        public static string Username { get { return _username; } }
        public static string UserID { get { return _userID; } }
        public static BeatmapLevelSO stubLevel
        {
            get
            {
                if (_stubLevel == null)
                {
                    _stubLevel = ScriptableObject.CreateInstance<BeatmapLevelSO>();
                    _stubLevel.SetField("_songName", "Stub song");
                    _stubLevel.SetField("_songSubName", "Don't worry about it");
                    _stubLevel.SetField("_levelID", "custom_level_stub");
                    /*
                    BeatmapCharacteristicSO[] characteristics = new BeatmapCharacteristicSO[1];
                    characteristics[0] = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                    characteristics[0].SetPrivateField("_characteristicName", characteristic);
                    errorSO.SetBeatmapCharacteristics(characteristics);
                    errorSO.SetPrivateField("_audioClip", SongLoader.TemporaryAudioClip);
                    errorSO.SetPrivateField("_previewStartTime", 0);
                    errorSO.SetPrivateField("_previewDuration", 0);*/

                }
                return _stubLevel;
            }
        }
        private static string _username;
        private static string _userID;
        private static BeatmapLevelSO _stubLevel;

        public static string GetHash(string usedString)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(usedString + "%");
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string EncodePath(string path)
        {
            path = Uri.EscapeDataString(path);
            path = path.Replace("%2F", "/");
            path = path.Replace("%3A", ":");
            return path;
        }

        public static GameplayModifierParamsSO CreateModifierParam(Sprite icon, string title, string desc)
        {
            GameplayModifierParamsSO param = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
            param.SetField("_modifierNameLocalizationKey", title);
            param.SetField("_descriptionLocalizationKey", desc);
            param.SetField("_icon", icon);
            return param;
        }

        internal static void SetUserInfo(UserInfo userInfo)
        {
            CustomCampaigns.Plugin.logger.Debug("setting user info");
            _username = userInfo.userName + "";
            _userID = userInfo.platformUserId;
            CustomCampaigns.Plugin.logger.Debug($"username: {_username}");
            CustomCampaigns.Plugin.logger.Debug($"setting: {_userID}");
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
