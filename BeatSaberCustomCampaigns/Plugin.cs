using BeatSaberCustomCampaigns.campaign;
using CustomUI.MenuButton;
using CustomUI.Utilities;
using Harmony;
using IPA;
using Polyglot;
using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeatSaberCustomCampaigns
{
    public class Plugin : IBeatSaberPlugin
    {
        CustomCampaignFlowCoordinator campaignFlowCoordinator;
        public void OnApplicationStart()
        {

            try
            {
                var harmony = HarmonyInstance.Create("com.monkeymanboy.BeatSaber.BeatSaberCustomCampaigns");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Console.WriteLine("[Challenges] This plugin requires Harmony. Make sure you " +
                    "installed the plugin properly, as the Harmony DLL should have been installed with it.");
                Console.WriteLine(e);
            }
        }

        public void OnApplicationQuit()
        {

        }

        public void OnUpdate()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name.Equals("MenuCore"))
            {
                Assets.Init();
                UnlockedMaps.Load();

                MainFlowCoordinator _mainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();

                Button campaignButton = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First().GetPrivateField<Button>("_campaignButton");
                RectTransform rectTransform = campaignButton.transform as RectTransform;
                (rectTransform.Find("Wrapper") as RectTransform).localPosition = new Vector2(20, 7.5f);
                (rectTransform.Find("Wrapper") as RectTransform).sizeDelta = new Vector2(0, -16);
                (rectTransform.Find("Wrapper").Find("Content").Find("Text") as RectTransform).anchoredPosition = new Vector2(4, 0);
                (rectTransform.Find("Wrapper").Find("Content").Find("Icon") as RectTransform).anchoredPosition = new Vector2(-12, 0);
                (rectTransform.Find("Wrapper").Find("Content").Find("Icon") as RectTransform).anchorMax = new Vector2(1, 1);
                GameObject newWrapper = GameObject.Instantiate(campaignButton, campaignButton.transform.parent).gameObject;
                newWrapper.transform.SetAsFirstSibling();
                newWrapper.transform.Find("Wrapper").gameObject.SetActive(false);
                newWrapper.GetComponent<HoverHint>().enabled = false;
                campaignButton.transform.parent = newWrapper.transform;
                Button customButton = GameObject.Instantiate(campaignButton, newWrapper.transform);
                (customButton.transform.Find("Wrapper") as RectTransform).localPosition = new Vector2(20, -7.5f);
                customButton.GetComponent<HoverHint>().text = "Play compilations of challenges made by the community!";
                customButton.GetComponentInChildren<TextMeshProUGUI>().text = "Custom Campaigns";
                customButton.GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;
                customButton.onClick.AddListener(delegate {
                    if (campaignFlowCoordinator == null)
                    {
                        campaignFlowCoordinator = new GameObject("CustomCampaignFlowCoordinator").AddComponent<CustomCampaignFlowCoordinator>();
                        campaignFlowCoordinator._mainFlowCoordinator = _mainFlowCoordinator;
                    }
                    _mainFlowCoordinator.InvokePrivateMethod("PresentFlowCoordinator", new object[] { campaignFlowCoordinator, null, false, false });
                });

            }
        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }
    }
}
