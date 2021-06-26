using BeatSaberCustomCampaigns.campaign;
using Newtonsoft.Json;
using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using UnityEngine;

namespace BeatSaberCustomCampaigns
{
    [Serializable]
    public class Challenge
    {
        public string name;
        public string songid;
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
                CustomPreviewBeatmapLevel level = Loader.CustomLevels.Values.First(x => x.customLevelPath.Contains("\\" + songid + (customDownloadURL == "" ? " " : ""))); //Including the space is to ensure that if they have a map with an old style beatsaver id it won't be falsely detected
                return level;
            } catch
            {
                return null;
            }
        }
        public MissionDataSO GetMissionData(Campaign campaign)
        {
            CustomMissionDataSO data = ScriptableObject.CreateInstance<CustomMissionDataSO>();
            data.campaign = campaign;
            data.challenge = this;
            FieldAccessor<CustomMissionDataSO, GameplayModifiers>.Access(ref data, "_gameplayModifiers") = modifiers.GetGameplayModifiers();
            FieldAccessor<CustomMissionDataSO, MissionObjective[]>.Access(ref data, "_missionObjectives") = GetMissionObjectives();

            if (challengeInfo != null)
            {
                CustomMissionHelpSO missionHelp = ScriptableObject.CreateInstance<CustomMissionHelpSO>();
                missionHelp.challengeInfo = challengeInfo;
                missionHelp.imagePath = campaign.path + "/images/";
                FieldAccessor<CustomMissionHelpSO, string>.Access(ref missionHelp, "_missionHelpId") = GetHash();
                FieldAccessor<CustomMissionDataSO, MissionHelpSO>.Access(ref data, "_missionHelp") = missionHelp;
            }

            FieldAccessor<CustomMissionDataSO, BeatmapDifficulty>.Access(ref data, "_beatmapDifficulty") = difficulty;
            CustomPreviewBeatmapLevel level = FindSong();
            data.customLevel = level;
            if (level != null)
            {
                try
                {
                    FieldAccessor<CustomMissionDataSO, BeatmapCharacteristicSO>.Access(ref data, "_beatmapCharacteristic") =
                        level.previewDifficultyBeatmapSets.GetBeatmapCharacteristics().First(x => x.serializedName == characteristic);
                } catch
                {
                    BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                    FieldAccessor<BeatmapCharacteristicSO, string>.Access(ref characteristicSO, "_characteristicNameLocalizationKey") = characteristic;
                    FieldAccessor<BeatmapCharacteristicSO, string>.Access(ref characteristicSO, "_descriptionLocalizationKey") = "ERROR NOT FOUND";
                    FieldAccessor<CustomMissionDataSO, BeatmapCharacteristicSO>.Access(ref data, "_beatmapCharacteristic") = characteristicSO;
                }
                
                FieldAccessor<CustomMissionDataSO, BeatmapLevelSO>.Access(ref data, "_level") = APITools.stubLevel;
            }
            return data;
        }
        public string GetDownloadURL()
        {
            return customDownloadURL == "" ? ("https://beatsaver.com/api/download/key/" + songid) : (customDownloadURL);
        }
        public string GetHash()
        {
            return APITools.GetHash(rawJSON);
        }
    }
}
