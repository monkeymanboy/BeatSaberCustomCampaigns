using BeatSaberCustomCampaigns.campaign;
using Newtonsoft.Json;
using SongCore;
using SongCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                CustomPreviewBeatmapLevel level = Loader.CustomLevels.Values.First(x => (x.customLevelPath.Contains("\\" + songid)));
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
            data.SetPrivateField("_gameplayModifiers", modifiers.GetGameplayModifiers());
            data.SetPrivateField("_missionObjectives", GetMissionObjectives());

            data.SetPrivateField("_beatmapDifficulty", difficulty);
            CustomPreviewBeatmapLevel level = FindSong();
            data.customLevel = level;
            if (level != null)
            {
                try
                {
                    data.SetPrivateField("_beatmapCharacteristic", level.beatmapCharacteristics.First(x => x.serializedName == characteristic));
                } catch
                {
                    BeatmapCharacteristicSO characteristicSO = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                    characteristicSO.SetPrivateField("_characteristicName", characteristic);
                    characteristicSO.SetPrivateField("_hintText", "ERROR NOT FOUND");
                    data.SetPrivateField("_beatmapCharacteristic", characteristicSO);
                }
                data.SetPrivateField("_level", APITools.stubLevel);
            }
            return data;
        }
        public string GetDownloadURL()
        {
            return customDownloadURL == "" ? ("https://www.beatsaver.com/api/download/key/" + songid) : (customDownloadURL);
        }
        public string GetHash()
        {
            return APITools.GetHash(rawJSON);
        }
    }
}
