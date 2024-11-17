using IPA.Utilities;
using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using static BeatmapLevelSaveDataVersion4.BeatmapLevelSaveData;
using static CustomLevelLoader;
using static IPA.Logging.Logger;

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
        public Mission missionParent;
        [JsonIgnore]
        public List<Mission> missionAlts = new List<Mission>();

        [JsonIgnore]
        private CustomMissionDataSO missionData = null;

        private static Dictionary<string, BeatmapCharacteristicSO> _customCharacteristicMap  = new Dictionary<string, BeatmapCharacteristicSO>();
        public static Action<string> OnUnkownCharacteristic;

        public MissionObjective[] GetMissionObjectives()
        {
            MissionObjective[] objectives = new MissionObjective[requirements.Length];
            for (int i = 0; i < requirements.Length; i++)
            {
                objectives[i] = requirements[i].GetAsMissionObjective();
            }
            return objectives;
        }

        public BeatmapLevel FindSong()
        {
            try
            {
                string missionHash = hash;
                if (missionHash == "")
                {
                    string songidSearch = "\\" + songid + (customDownloadURL == "" ? " " : "");
                    Dictionary<string, LoadedSaveData> loadedBeatmapSaveData = Loader.CustomLevelLoader.GetField<Dictionary<string, LoadedSaveData>, CustomLevelLoader>("_loadedBeatmapSaveData");
                    LoadedSaveData loadedSaveData = Loader.CustomLevelLoader.GetField<Dictionary<string, LoadedSaveData>, CustomLevelLoader>("_loadedBeatmapSaveData")
                                                        .Values.First(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.customLevelFolderInfo.folderPath, songidSearch, CompareOptions.IgnoreCase) >= 0);
                    missionHash = SongCore.Utilities.Hashing.GetCustomLevelHash(loadedSaveData.customLevelFolderInfo, loadedSaveData.standardLevelInfoSaveData);
                }

                if (missionHash != "")
                {
                    List<string> levelIDs = SongCore.Collections.levelIDsForHash(missionHash);
                    BeatmapLevel beatmapLevel = Loader.CustomLevels.Values.First(x => levelIDs.Contains(x.levelID));
                    foreach (var kvp in Loader.CustomLevelLoader.GetField<Dictionary<string, IBeatmapLevelData>, CustomLevelLoader>("_loadedBeatmapLevelsData"))
                    {
                        if (levelIDs.Contains(kvp.Key))
                        {
                            if (missionData == null)
                            {
                                Plugin.logger.Error("missionData should not be null here!");
                                return null;
                            }

                            missionData.beatmapLevelData = kvp.Value;
                            break;
                        }
                    }

                    return beatmapLevel;
                }

                return null;
            }

            catch
            {
                return null;
            }
        }

        public MissionDataSO GetMissionData(Campaign campaign)
        {
            if (missionData != null && missionData.beatmapCharacteristic.descriptionLocalizationKey != "ERROR NOT FOUND")
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

        public MissionDataSO TryGetMissionData()
        {
            if (missionData == null)
            {
                throw new Exception("No mission data");
            }

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
                BeatmapLevel beatmapLevel = FindSong();
                missionData.beatmapLevel = beatmapLevel;
                if (beatmapLevel != null)
                {
                    try
                    {
                        missionData.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", beatmapLevel.GetCharacteristics().First(x => x.serializedName == characteristic));
                    }
                    catch
                    {
                        if (_customCharacteristicMap.ContainsKey(characteristic))
                        {

                            missionData.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", _customCharacteristicMap[characteristic]);
                        }
                        else
                        {
                            OnUnkownCharacteristic?.Invoke(characteristic);

                            BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                            characteristicSO.SetField("_characteristicNameLocalizationKey", characteristic);
                            characteristicSO.SetField("_descriptionLocalizationKey", "ERROR NOT FOUND");
                            missionData.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", characteristicSO);
                        }
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

        public Dictionary<string, Mission> GetMissionAlts()
        {
            if (missionParent != null)
            {
                return missionParent.GetMissionAlts();
            }

            Dictionary<string, Mission> alts = new Dictionary<string, Mission>();

            alts.Add(name, this);
            foreach (var mission in missionAlts)
            {
                alts.Add(mission.name, mission);
            }

            return alts;
        }

        public List<string> GetMissionAltNames()
        {
            List<string> altNames = new List<string>();
            altNames.Add(name);
            foreach (var mission in missionAlts)
            {
                altNames.Add(mission.name);
            }

            return altNames;
        }

        public static void AddCustomCharacteristicSO(string characteristic, BeatmapCharacteristicSO beatmapCharacteristicSO)
        {
            _customCharacteristicMap[characteristic] = beatmapCharacteristicSO;
        }

        private void OnSongsLoaded(Loader loader, ConcurrentDictionary<string, BeatmapLevel> levels)
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
