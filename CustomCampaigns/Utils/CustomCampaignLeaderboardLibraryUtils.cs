using CustomCampaigns.Campaign;
using CustomCampaigns.Campaign.Missions;
using IPA.Utilities;
using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

        //public MissionObjective[] GetMissionObjectives()
        //{
        //    MissionObjective[] objectives = new MissionObjective[requirements.Length];
        //    for (int i = 0; i < requirements.Length; i++)
        //    {
        //        objectives[i] = requirements[i].GetAsMissionObjective();
        //    }
        //    return objectives;
        //}

        //public CustomPreviewBeatmapLevel FindSong()
        //{
        //    try
        //    {
        //        CustomPreviewBeatmapLevel level = null;
        //        //if (hash != "")
        //        //{
        //        //    var beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault(x => x.customLevelPackCollection != null);
        //        //    level = (CustomPreviewBeatmapLevel)beatmapLevelsModel?.GetLevelPreviewForLevelId("custom_level_" + hash.ToUpper());
        //        //    return level;
        //        //}

        //        //// Including the space is to ensure that if they have a map with an old style beatsaver id it won't be falsely detected
        //        string songidSearch = "\\" + songid + (customDownloadURL == "" ? " " : "");
        //        level = Loader.CustomLevels.Values.First(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.customLevelPath, songidSearch, CompareOptions.IgnoreCase) >= 0);
        //        return level;
        //    }

        //    catch
        //    {
        //        return null;
        //    }
        //}

        //public MissionDataSO GetMissionData(Campaign campaign)
        //{
        //    CustomMissionDataSO data = ScriptableObject.CreateInstance<CustomMissionDataSO>();
        //    data.campaign = campaign;
        //    data.mission = _mission;
        //    data.SetField<MissionDataSO, GameplayModifiers>("_gameplayModifiers", modifiers.GetGameplayModifiers());
        //    data.SetField<MissionDataSO, MissionObjective[]>("_missionObjectives", GetMissionObjectives());

        //    if (missionInfo != null)
        //    {
        //        CustomMissionHelpSO missionHelp = ScriptableObject.CreateInstance<CustomMissionHelpSO>();
        //        missionHelp.missionInfo = missionInfo;
        //        missionHelp.imagePath = campaign.campaignPath + "/images/";
        //        missionHelp.SetField<MissionHelpSO, string>("_missionHelpId", GetHash());
        //        data.SetField<MissionDataSO, MissionHelpSO>("_missionHelp", missionHelp);
        //    }

        //    data.SetField<MissionDataSO, BeatmapDifficulty>("_beatmapDifficulty", difficulty);
        //    CustomPreviewBeatmapLevel level = FindSong();
        //    data.customLevel = level;
        //    if (level != null)
        //    {
        //        try
        //        {
        //            data.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", level.previewDifficultyBeatmapSets.GetBeatmapCharacteristics().First(x => x.serializedName == characteristic));
        //        }
        //        catch
        //        {
        //            BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
        //            characteristicSO.SetField("_characteristicNameLocalizationKey", characteristic);
        //            characteristicSO.SetField("_descriptionLocalizationKey", "ERROR NOT FOUND");
        //            data.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", characteristicSO);
        //        }

        //        //data.SetField<MissionDataSO, BeatmapLevelSO>("_level", APITools.stubLevel);
        //    }
        //    return data;
        //}

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
