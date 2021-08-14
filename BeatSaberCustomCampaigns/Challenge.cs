using BeatSaberCustomCampaigns.campaign;
using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;

namespace BeatSaberCustomCampaigns
{
    [Serializable]
    public class Challenge
    {
        public string name;
        public string songid;
        public string hash = "";
        public string customDownloadURL = "";
        public string characteristic = "Standard";
        public BeatmapDifficulty difficulty;
        public ChallengeModifiers modifiers;
        public ChallengeRequirement[] requirements;
        public Dictionary<string, string[]> externalModifiers = new Dictionary<string, string[]>();
        public ChallengeInfo challengeInfo = null;

        public List<UnlockableItem> unlockableItems = new List<UnlockableItem>();

        public bool unlockMap = false;

        [JsonIgnore]
        public string rawJSON;

        public MissionObjective[] GetMissionObjectives()
        {
            MissionObjective[] objectives = new MissionObjective[requirements.Length];
            for(int i=0;i<requirements.Length;i++)
            {
                objectives[i] = requirements[i].GetAsMissionObjective();
            }
            return objectives;
        }
        public CustomPreviewBeatmapLevel FindSong()
        {
            try
            {
                CustomPreviewBeatmapLevel level = null;
                if (hash != "")
                {
                    var beatmapLevelsModel = Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault(x => x.customLevelPackCollection != null);
                    level = (CustomPreviewBeatmapLevel) beatmapLevelsModel?.GetLevelPreviewForLevelId("custom_level_" + hash.ToUpper());
                    return level;
                }

                // Including the space is to ensure that if they have a map with an old style beatsaver id it won't be falsely detected
                string songidSearch = "\\" + songid + (customDownloadURL == "" ? " " : "");
                level = Loader.CustomLevels.Values.First(x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.customLevelPath, songidSearch, CompareOptions.IgnoreCase) >= 0); 
                return level;
            }

            catch
            {
                return null;
            }
        }
        public MissionDataSO GetMissionData(Campaign campaign)
        {
            CustomMissionDataSO data = ScriptableObject.CreateInstance<CustomMissionDataSO>();
            data.campaign = campaign;
            data.challenge = this;
            data.SetField<MissionDataSO, GameplayModifiers>("_gameplayModifiers", modifiers.GetGameplayModifiers());
            data.SetField<MissionDataSO, MissionObjective[]>("_missionObjectives", GetMissionObjectives());

            if (challengeInfo != null)
            {
                CustomMissionHelpSO missionHelp = ScriptableObject.CreateInstance<CustomMissionHelpSO>();
                missionHelp.challengeInfo = challengeInfo;
                missionHelp.imagePath = campaign.path + "/images/";
                missionHelp.SetField<MissionHelpSO, string>("_missionHelpId", GetHash());
                data.SetField<MissionDataSO, MissionHelpSO>("_missionHelp", missionHelp);
            }

            data.SetField<MissionDataSO, BeatmapDifficulty>("_beatmapDifficulty", difficulty);
            CustomPreviewBeatmapLevel level = FindSong();
            data.customLevel = level;
            if (level != null)
            {
                try
                {
                    data.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", level.previewDifficultyBeatmapSets.GetBeatmapCharacteristics().First(x => x.serializedName == characteristic));
                }
                catch
                {
                    BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                    characteristicSO.SetField("_characteristicNameLocalizationKey", characteristic);
                    characteristicSO.SetField("_descriptionLocalizationKey", "ERROR NOT FOUND");
                    data.SetField<MissionDataSO, BeatmapCharacteristicSO>("_beatmapCharacteristic", characteristicSO);
                }
                
                data.SetField<MissionDataSO, BeatmapLevelSO>("_level", APITools.stubLevel);
            }
            return data;
        }

        public string GetHash()
        {
            return APITools.GetHash(rawJSON);
        }
    }
}
