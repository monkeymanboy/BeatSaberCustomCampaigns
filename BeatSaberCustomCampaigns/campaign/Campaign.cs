using CustomUI.BeatSaber;
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

namespace BeatSaberCustomCampaigns.campaign
{
    public class Campaign : CustomCellInfo
    {
        private static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
        public CampaignInfo info;
        public List<Challenge> challenges;
        public Sprite background;
        public string path;
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
            text = info.name;
            subtext = info.desc;
        }

        private IEnumerator LoadSprite(CampaignListViewController viewController, string spritePath, bool isBackground)
        {
            if (!loadedSprites.ContainsKey(spritePath))
            {
                using (var web = UnityWebRequestTexture.GetTexture(EncodePath(spritePath), true))
                {
                    yield return web.SendWebRequest();
                    if (web.isNetworkError || web.isHttpError)
                    {
                        icon = null;
                    }
                    else
                    {
                        var tex = DownloadHandlerTexture.GetContent(web);
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
                        loadedSprites.Add(spritePath, sprite);
                        if (isBackground) background = sprite;
                        else icon = sprite;
                    }
                }
            }
            else
            {
                if (isBackground) background = loadedSprites[spritePath];
                else icon = loadedSprites[spritePath];
            }
            if (!isBackground)
            {
                viewController._customListTableView.ReloadData();
            }
        }
        private static string EncodePath(string path)
        {
            path = Uri.EscapeDataString(path);
            path = path.Replace("%2F", "/");
            path = path.Replace("%3A", ":");
            return path;
        }
    }
}
