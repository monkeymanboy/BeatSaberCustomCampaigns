using HMUI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace BeatSaberCustomCampaigns.campaign
{
    public class Campaign : CustomCellInfo
    {
        private static Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
        public CampaignInfo info;
        public List<Challenge> challenges;
        public Sprite background;
        public string path;
        public string leaderboardID = "";
        public Campaign() : base("", "", null)
        {
        }
        public Campaign(CampaignListViewController viewController, string path) : base("", "", null)
        {
            this.path = path;
            challenges = new List<Challenge>();
            info = JsonConvert.DeserializeObject<CampaignInfo>(File.ReadAllText(path + "/info.json"));
            viewController.StartCoroutine(LoadSprite(viewController, "file:///" + path + "/cover.png", false));
            if(File.Exists(path + "/map background.png"))viewController.StartCoroutine(LoadSprite(viewController, "file:///" + path + "/map background.png", true));
            int i = 0;
            while (File.Exists(path + "/" + i + ".json"))
            {
                string rawJSON = File.ReadAllText(path + "/" + i + ".json").Replace("\n", "");
                Challenge challenge = JsonConvert.DeserializeObject<Challenge>(rawJSON);
                challenge.rawJSON = rawJSON;
                challenges.Add(challenge);
                i++;
            }
            if (File.Exists(path + "/id")) leaderboardID = File.ReadAllText(path + "/id");
            text = info.name;
            subtext = info.desc;
        }

        private IEnumerator LoadSprite(CampaignListViewController viewController, string spritePath, bool isBackground)
        {
            if (!loadedTextures.ContainsKey(spritePath))
            {
                using (var web = UnityWebRequestTexture.GetTexture(APITools.EncodePath(spritePath), true))
                {
                    yield return web.SendWebRequest();
                    if (web.isNetworkError || web.isHttpError)
                    {
                        icon = null;
                    }
                    else
                    {
                        Texture2D tex = DownloadHandlerTexture.GetContent(web);
                        //Sprite sprite = ;
                        loadedTextures.Add(spritePath, tex);
                        if (isBackground) background = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
                        else icon = tex;
                    }
                }
            }
            else
            {
                Texture2D tex = loadedTextures[spritePath];
                if (isBackground) background = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
                else icon = tex;
            }
            if (!isBackground)
            {
                viewController.customListTableData.tableView.ReloadData();
            }
        }
    }
}
