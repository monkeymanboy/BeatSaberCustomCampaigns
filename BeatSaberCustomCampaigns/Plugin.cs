using BeatSaberCustomCampaigns.campaign;
using CustomUI.MenuButton;
using CustomUI.Utilities;
using Harmony;
using IPA;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                MenuButtonUI.AddButton("Custom Campaigns", "Play compilations of challenges by the community", delegate {
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
