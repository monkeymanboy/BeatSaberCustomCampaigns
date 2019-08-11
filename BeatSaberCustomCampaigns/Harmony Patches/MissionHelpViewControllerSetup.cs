using Harmony;
using Polyglot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(MissionHelpViewController), "Setup",
        new Type[] { typeof(MissionHelpSO) })]
    class MissionHelpViewControllerSetup
    {
        private static Transform lastInfo = null;
        private static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
        private static MonoBehaviour imageLoader = null;

        static void Postfix(MissionHelpSO missionHelp, MissionHelpViewController __instance)
        {
            if (missionHelp is CustomMissionHelpSO)
            {
                ChallengeInfo challengeInfo = (missionHelp as CustomMissionHelpSO).challengeInfo;
                string imagePath = (missionHelp as CustomMissionHelpSO).imagePath;
                Transform content = __instance.transform.GetChild(0);
                TextMeshProUGUI title = content.GetChild(0).GetComponent<TextMeshProUGUI>();
                Transform seperatorPrefab = content.GetChild(1).GetChild(1);
                Transform segmentPrefab = content.GetChild(1).GetChild(2);

                GameObject.Destroy(title.GetComponent<LocalizedTextMeshProUGUI>());
                title.text = challengeInfo.title;
                Transform infoContainer = GameObject.Instantiate(content.GetChild(1), content);
                infoContainer.gameObject.SetActive(true);
                if (lastInfo != null)
                {
                    GameObject.Destroy(lastInfo.gameObject);
                }
                lastInfo = infoContainer;
                foreach (Transform child in infoContainer)
                {
                    GameObject.Destroy(child.gameObject);
                }
                foreach (ChallengeInfo.InfoSegment infoSegment in challengeInfo.segments)
                {
                    Transform segment = GameObject.Instantiate(segmentPrefab, infoContainer);
                    GameObject.Destroy(segment.GetComponentInChildren<LocalizedTextMeshProUGUI>());
                    if (infoSegment.text == "")
                    {
                        GameObject.Destroy(segment.GetComponentInChildren<TextMeshProUGUI>().gameObject);
                    }
                    else
                    {
                        segment.GetComponentInChildren<TextMeshProUGUI>().text = infoSegment.text;
                    }
                    Image image = segment.GetComponentInChildren<Image>();
                    if (infoSegment.imageName == "")
                    {
                        GameObject.Destroy(image.gameObject);
                    }
                    else
                    {
                        image.sprite = null;
                        if (imageLoader == null) imageLoader = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
                        imageLoader.StartCoroutine(LoadSprite("file:///" + imagePath + infoSegment.imageName, image));
                    }
                    if (infoSegment.hasSeperator)
                    {
                        GameObject.Instantiate(seperatorPrefab, infoContainer);
                    }
                }
            }
            else
            {
                __instance.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "NEW OBJECTIVE";
            }
        }

        private static IEnumerator LoadSprite(string spritePath, Image imageToApplyTo)
        {
            if (!loadedSprites.ContainsKey(spritePath))
            {
                using (var web = UnityWebRequestTexture.GetTexture(APITools.EncodePath(spritePath), true))
                {
                    yield return web.SendWebRequest();
                    if (web.isNetworkError || web.isHttpError)
                    {

                    }
                    else
                    {
                        var tex = DownloadHandlerTexture.GetContent(web);
                        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f, 100, 1);
                        loadedSprites.Add(spritePath, sprite);
                        imageToApplyTo.sprite = sprite;
                    }
                }
            }
            else
            {
                imageToApplyTo.sprite = loadedSprites[spritePath];
            }
        }
    }
}
