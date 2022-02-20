﻿using BeatSaberMarkupLanguage;
using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using HMUI;
using Polyglot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionHelpViewController), "Setup")]
    class MissionHelpViewControllerSetup
    {
        private static Transform lastInfo = null;
        private static Dictionary<string, Sprite> loadedSprites = new Dictionary<string, Sprite>();
        private static MonoBehaviour imageLoader = null;

        // TODO: move this out of harmony patch
        static void Postfix(MissionHelpSO missionHelp, MissionHelpViewController __instance)
        {
            CustomMissionHelpSO customMissionHelp = missionHelp as CustomMissionHelpSO;
            if (customMissionHelp != null)
            {
                MissionInfo missionInfo = (missionHelp as CustomMissionHelpSO).missionInfo;
                string imagePath = (missionHelp as CustomMissionHelpSO).imagePath;

                Transform content = __instance.transform.GetChild(0);
                CurvedTextMeshPro title = content.GetChild(0).GetChild(1).GetComponent<CurvedTextMeshPro>();
                Transform seperatorPrefab = content.GetChild(6).GetChild(1);
                Transform segmentPrefab = content.GetChild(1).GetChild(1);

                GameObject.Destroy(title.GetComponent<LocalizedTextMeshProUGUI>());
                title.text = missionInfo.title;
                title.richText = true;
                Transform infoContainer = GameObject.Instantiate(content.GetChild(1), content);
                infoContainer.SetSiblingIndex(content.childCount - 2);

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

                foreach (InfoSegment infoSegment in missionInfo.segments)
                {
                    Transform segment = GameObject.Instantiate(segmentPrefab, infoContainer);
                    GameObject.Destroy(segment.GetComponentInChildren<LocalizedTextMeshProUGUI>());
                    if (infoSegment.text == "")
                    {
                        GameObject.Destroy(segment.GetComponentInChildren<CurvedTextMeshPro>().gameObject);
                    }
                    else
                    {
                        segment.GetComponentInChildren<CurvedTextMeshPro>().text = infoSegment.text;
                    }

                    ImageView imageView = segment.GetComponentInChildren<ImageView>();
                    if (infoSegment.imageName == "")
                    {
                        GameObject.Destroy(imageView.gameObject);
                    }
                    else
                    {
                        imageView.sprite = null;
                        if (imageLoader == null)
                        {
                            imageLoader = BeatSaberUI.MainFlowCoordinator;
                        }
                        imageView.gradient = false;
                        imageLoader.StartCoroutine(LoadSprite("file:///" + imagePath + infoSegment.imageName, imageView));
                    }
                    if (infoSegment.hasSeparator)
                    {
                        GameObject.Instantiate(seperatorPrefab, infoContainer);
                    }
                }
            }
            else
            {
                __instance.transform.GetChild(0).GetComponentInChildren<CurvedTextMeshPro>().text = "NEW OBJECTIVE";
            }
        }

        private static IEnumerator LoadSprite(string spritePath, Image imageToApplyTo)
        {
            if (!loadedSprites.ContainsKey(spritePath))
            {
                using (var web = UnityWebRequestTexture.GetTexture(spritePath, true))
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
