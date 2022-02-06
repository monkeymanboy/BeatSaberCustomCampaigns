using IPA.Utilities;
using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace CustomCampaigns.Campaign.Missions
{
    public class Mission
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
        public Dictionary<string, string[]> optionalExternalModifiers = new Dictionary<string, string[]>();
        [JsonProperty("challengeInfo")]
        public MissionInfo missionInfo = null;

        public List<UnlockableItem> unlockableItems = new List<UnlockableItem>();

        public bool unlockMap = false;
        public bool allowStandardLevel = false;

        [JsonIgnore]
        public string rawJSON;
        [JsonIgnore]
        private CustomMissionDataSO missionData = null;

        public MissionObjective[] GetMissionObjectives()
        {
            MissionObjective[] objectives = new MissionObjective[requirements.Length];
            for (int i = 0; i < requirements.Length; i++)
            {
                objectives[i] = requirements[i].GetAsMissionObjective();
            }
            return objectives;
        }

        public CustomPreviewBeatmapLevel FindSong()
        {
            try
            {
                if (hash != "")
                {
                    List<string> levelIDs = SongCore.Collections.levelIDsForHash(hash);
                    CustomPreviewBeatmapLevel level = Loader.CustomLevels.Values.First(x => levelIDs.Contains(x.levelID));
                    return level;
                }
                // VS >:(
                else
                {
                    // Including the space is to ensure that if they have a map with an old style beatsaver id it won't be falsely detected
                    string songidSearch = "\\" + songid + (customDownloadURL == "" ? " " : "");
                    CustomPreviewBeatmapLevel level = Loader.CustomLevels.Values.First(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.customLevelPath, songidSearch, CompareOptions.IgnoreCase) >= 0);
                    return level;
                }
            }

            catch
            {
                return null;
            }
        }

        public MissionDataSO GetMissionData(Campaign campaign)
        {
            if (missionData != null)
            {
                return missionData;
            }

            missionData = ScriptableObject.CreateInstance<CustomMissionDataSO>();
            missionData.campaign = campaign;
            missionData.mission = this;
            missionData.SetField<MissionDataSO, GameplayModifiers>("_gameplayModifiers", modifiers.GetGameplayModifiers());
            missionData.SetField<MissionDataSO, MissionObjective[]>("_missionObjectives", GetMissionObjectives());

            if (missionInfo != null)
            {
                CustomMissionHelpSO missionHelp = ScriptableObject.CreateInstance<CustomMissionHelpSO>();
                missionHelp.missionInfo = missionInfo;
                missionHelp.imagePath = campaign.campaignPath + "/images/";
                missionHelp.SetField<MissionHelpSO, string>("_missionHelpId", GetHash());
                missionData.SetField<MissionDataSO, MissionHelpSO>("_missionHelp", missionHelp);
            }

            missionData.SetField<MissionDataSO, BeatmapDifficulty>("_beatmapDifficulty", difficulty);
            SetCustomLevel();
            return missionData;
        }

        public void SetCustomLevel()
        {
            if (missionData == null)
            {
                Plugin.logger.Error("Tried to set custom level before mission data was set!");
                return;
            }

            else
            {
                CustomPreviewBeatmapLevel level = FindSong();
                missionData.customLevel = level;
                if (level != null)
                {
                    try
                    {
                        missionData.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", level.previewDifficultyBeatmapSets.GetBeatmapCharacteristics().First(x => x.serializedName == characteristic));
                    }
                    catch
                    {
                        BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                        characteristicSO.SetField("_characteristicNameLocalizationKey", characteristic);
                        characteristicSO.SetField("_descriptionLocalizationKey", "ERROR NOT FOUND");
                        missionData.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", characteristicSO);
                    }
                }

                SongCore.Loader.SongsLoadedEvent -= OnSongsLoaded;
                SongCore.Loader.SongsLoadedEvent += OnSongsLoaded;
            }
        }

        public void CampaignClosed()
        {
            SongCore.Loader.SongsLoadedEvent -= OnSongsLoaded;
        }

        private void OnSongsLoaded(Loader loader, ConcurrentDictionary<string, CustomPreviewBeatmapLevel> levels)
        {
            SetCustomLevel();
        }

        private string GetHash()
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(rawJSON + "%");
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
