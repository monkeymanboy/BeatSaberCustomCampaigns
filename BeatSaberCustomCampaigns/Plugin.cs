using BeatSaberCustomCampaigns.campaign;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HarmonyLib;
using HMUI;
using IPA;
using IPA.Loader;
using Polyglot;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using HVersion =  Hive.Versioning.Version;

namespace BeatSaberCustomCampaigns
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static HVersion version;
        public static IPA.Logging.Logger logger { get; private set; }

        private CustomCampaignFlowCoordinator campaignFlowCoordinator;

        [Init]
        public void Init(IPA.Logging.Logger log, PluginMetadata metadata)
        {
            logger = log;
            version = metadata?.HVersion;
        }
        [OnStart]
        public async void OnApplicationStart()
        {

            try
            {
                var harmony = new Harmony("com.monkeymanboy.BeatSaber.BeatSaberCustomCampaigns");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                logger.Critical("[Challenges] This plugin requires Harmony. Make sure you " +
                    "installed the plugin properly, as the Harmony DLL should have been installed with it.");
                logger.Critical(e);
            }
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            await APITools.InitializeUserInfo();
        }

        //Base game does a ton of stuff when everything gets enabled so this just makes sure that happens, without this some stuff will break
        IEnumerator InitializeMap()
        {
            MissionSelectionMapViewController map = Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().First();
            bool mapState = map.gameObject.activeSelf;
            bool parentState = map.transform.parent.gameObject.activeSelf;
            map.gameObject.SetActive(true);
            map.transform.parent.gameObject.SetActive(true);
            yield return new WaitForFixedUpdate();
            map.gameObject.SetActive(mapState);
            map.transform.parent.gameObject.SetActive(parentState);
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name.Equals("MainMenu") && prevScene.name.Equals("EmptyTransition"))
            {
                Assets.Init();
                UnlockedMaps.Load();

                //MainFlowCoordinator _mainFlowCoordinator = Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First();
                //Button campaignButton = Resources.FindObjectsOfTypeAll<MainMenuViewController>().First().GetPrivateField<Button>("_campaignButton");
                //RectTransform rectTransform = campaignButton.transform as RectTransform;
                //(rectTransform.Find("Wrapper") as RectTransform).localPosition = new Vector2(20, 7.5f);
                //(rectTransform.Find("Wrapper") as RectTransform).sizeDelta = new Vector2(0, -16);
                //(rectTransform.Find("Wrapper").Find("Content").Find("Text") as RectTransform).anchoredPosition = new Vector2(4, 0);
                //(rectTransform.Find("Wrapper").Find("Content").Find("Icon") as RectTransform).anchoredPosition = new Vector2(-12, 0);
                //(rectTransform.Find("Wrapper").Find("Content").Find("Icon") as RectTransform).anchorMax = new Vector2(1, 1);
                //GameObject newWrapper = GameObject.Instantiate(campaignButton, campaignButton.transform.parent).gameObject;
                //newWrapper.transform.SetAsFirstSibling();
                ////newWrapper.transform.Find("Wrapper").gameObject.SetActive(false);
                //newWrapper.GetComponent<HoverHint>().enabled = false;
                //campaignButton.transform.parent = newWrapper.transform;
                //Button customButton = GameObject.Instantiate(campaignButton, newWrapper.transform);
                //(customButton.transform.Find("Wrapper") as RectTransform).localPosition = new Vector2(20, -7.5f);
                //customButton.GetComponent<LocalizedHoverHint>().enabled = false;
                //customButton.GetComponent<HoverHint>().text = "Play compilations of challenges made by the community!";
                //customButton.GetComponentInChildren<TextMeshProUGUI>().text = "Custom Campaigns";
                //customButton.GetComponentInChildren<LocalizedTextMeshProUGUI>().enabled = false;
                //customButton.GetComponentsInChildren<Image>().First(x => x.gameObject.name == "Icon").sprite = Assets.ButtonIcon;
                //customButton.onClick.AddListener(delegate {
                //    if (campaignFlowCoordinator == null)
                //        campaignFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<CustomCampaignFlowCoordinator>();
                //    campaignFlowCoordinator.StartCoroutine(InitializeMap());
                //    BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(campaignFlowCoordinator, new Action(delegate {
                //        //Quick fix for an issue where if they open the regular campaign first the map appears on the campaign list
                //        Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().First().gameObject.SetActive(false);
                //    }));
                //});

                MenuButton customButton = new MenuButton("Custom Campaigns", "Play compilations of challenges made by the community!", new Action(delegate {
                    if (campaignFlowCoordinator == null)
                        campaignFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<CustomCampaignFlowCoordinator>();

                    campaignFlowCoordinator.StartCoroutine(InitializeMap());
                    BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinator(campaignFlowCoordinator, new Action(delegate
                    {
                        //Quick fix for an issue where if they open the regular campaign first the map appears on the campaign list
                        Resources.FindObjectsOfTypeAll<MissionSelectionMapViewController>().First().gameObject.SetActive(false);
                    }));
                }), true);

                MenuButtons.instance.RegisterButton(customButton);
            }

        }
    }
}
