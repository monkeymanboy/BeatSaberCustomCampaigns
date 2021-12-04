using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.UI.ViewControllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace CustomCampaigns.Campaign
{
    public class Campaign : CustomCellInfo
    {
        public List<Mission> missions;
        public CampaignInfo info;
        public Sprite background = null;
        public string campaignPath;
        public string leaderboardId;
        public string completionPost;

        private static Dictionary<string, Sprite> textures = new Dictionary<string, Sprite>();

        private const string INFO_LOCATION = "/info.json";
        private const string COVER_LOCATION = "/cover.png";
        private const string BACKGROUND_LOCATION = "/map background.png";
        private const string ID_LOCATION = "/id";
        private const string COMPLETION_POST_LOCATION = "/completion_post";

        public Campaign(CampaignListViewController campaignListViewController, string campaignPath) : base("", "", null)
        {
            this.campaignPath = campaignPath;

            info = JsonConvert.DeserializeObject<CampaignInfo>(File.ReadAllText(campaignPath + INFO_LOCATION));
            text = info.name;
            subtext = info.desc;

            GetSprites(campaignListViewController);

            missions = new List<Mission>();
            for (int i = 0; File.Exists(campaignPath + "/" + i + ".json"); i++)
            {
                string rawJSON = File.ReadAllText(campaignPath + "/" + i + ".json").Replace("\n", "");
                Mission mission = JsonConvert.DeserializeObject<Mission>(rawJSON);
                mission.rawJSON = rawJSON;
                missions.Add(mission);
            }

            leaderboardId = File.Exists(campaignPath + ID_LOCATION) ? File.ReadAllText(campaignPath + ID_LOCATION) : "";
            completionPost = File.Exists(campaignPath + COMPLETION_POST_LOCATION) ? File.ReadAllText(campaignPath + COMPLETION_POST_LOCATION) : "";
        }

        private async void GetSprites(CampaignListViewController viewController)
        {
            icon = await LoadSprite(campaignPath + COVER_LOCATION);
        }

        public async Task LoadBackground()
        {
            if (background == null)
            {
                background = await LoadSprite(campaignPath + BACKGROUND_LOCATION);
            }
        }

        public async Task LoadNodeSprites()
        {
            Plugin.logger.Debug("loading node sprites");
            foreach (var mapPosition in info.mapPositions)
            {
                if (mapPosition.nodeOutlineLocation != null)
                {
                    mapPosition.nodeOutline = await LoadSprite(campaignPath + "/" + mapPosition.nodeOutlineLocation);
                }

                if (mapPosition.nodeBackgroundLocation != null)
                {
                    mapPosition.nodeBackground = await LoadSprite(campaignPath + "/" + mapPosition.nodeBackgroundLocation);
                }
            }
            Plugin.logger.Debug("loaded node sprites");
        }

        private async Task<Sprite> LoadSprite(string spritePath)
        {
            if (!textures.ContainsKey(spritePath))
            {
                if (!File.Exists(spritePath))
                {
                    return null;
                }

                try
                {
                    using (FileStream stream = File.Open(spritePath, FileMode.Open))
                    {
                        byte[] bytes = new byte[stream.Length];
                        await stream.ReadAsync(bytes, 0, (int)stream.Length);
                        Sprite sprite = BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(bytes);
                        textures[spritePath] = sprite;
                    }
                }
                catch (Exception e)
                {
                    Plugin.logger.Warn($"Failed to load sprite at {spritePath}: {e.Message}");
                    return null;
                }
            }

            return textures[spritePath];
        }
    }
}
