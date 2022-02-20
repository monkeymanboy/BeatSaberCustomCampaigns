using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.HarmonyPatches.ScoreSaber;
using CustomCampaigns.Managers;
using HMUI;
using System;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.UI.ViewControllers
{
    [HotReload(RelativePathToLayout = @"..\Views\leaderboard-navigation.bsml")]
    [ViewDefinition("CustomCampaigns.UI.Views.leaderboard-navigation.bsml")]
    public class LeaderboardNavigationViewController : BSMLAutomaticViewController, IInitializable, IDisposable
    {
        private Config _config;

        private PlatformLeaderboardViewController _platformLeaderboardViewController;
        private CampaignMissionSecondaryLeaderboardViewController _campaignMissionSecondaryLeaderboardViewController;

        private FloatingScreen _floatingScreen;

        private Transform _containerTransform;
        private Vector3 _containerPosition;

        private Transform _ssLeaderboardElementsTransform;
        private Vector3 _ssLeaderboardElementsPosition;

        private Transform _ssPanelScreenTransform;
        private Vector3 _ssPanelScreenPosition;

        public bool ssShown = false;

        [UIComponent("toggle-button")]
        private ImageView _toggleButtonImage;

        private bool _initializedWithGlobalLeaderboards = false;

        [Inject]
        public void Consruct(Config config, PlatformLeaderboardViewController platformLeaderboardViewController, CampaignMissionSecondaryLeaderboardViewController campaignMissionSecondaryLeaderboardViewController,
                             CustomCampaignUIManager customCampaignUIManager)
        {
            _config = config;

            _platformLeaderboardViewController = platformLeaderboardViewController;
            _campaignMissionSecondaryLeaderboardViewController = campaignMissionSecondaryLeaderboardViewController;

            customCampaignUIManager.customCampaignEnabledEvent += CustomCampaignEnabled;
            customCampaignUIManager.baseCampaignEnabledEvent += CustomCampaignDisabled;

            customCampaignUIManager.missionDataReadyEvent += OnMissionDataReady;
            customCampaignUIManager.leaderboardUpdateEvent += SetGlobalLeaderboardViewController;
        }
        public void Initialize()
        {
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(25f, 25f), false, Vector3.zero, Quaternion.identity);

            _floatingScreen.transform.SetParent(_platformLeaderboardViewController.transform);
            _floatingScreen.transform.localPosition = new Vector3(-40, 25f);
            _floatingScreen.transform.localScale = new Vector3(0.5f, 0.5f);

            _floatingScreen.gameObject.SetActive(false);
            _floatingScreen.gameObject.SetActive(true);

            _floatingScreen.gameObject.name = "CustomCampaignsLeaderboardNavigation";
        }

        private void OnViewActivated()
        {
            if (_containerTransform == null)
            {
                _containerTransform = _platformLeaderboardViewController.transform.Find("Container");
                _containerPosition = _containerTransform.localPosition;
            }

            if (_ssLeaderboardElementsTransform == null)
            {
                _ssLeaderboardElementsTransform = _platformLeaderboardViewController.transform.Find("ScoreSaberLeaderboardElements");
                _ssLeaderboardElementsPosition = _ssLeaderboardElementsTransform.localPosition;
            }


            if (_ssPanelScreenTransform == null)
            {
                _ssPanelScreenTransform = _platformLeaderboardViewController.transform.Find("ScoreSaberPanelScreen");
                _ssPanelScreenPosition = _ssPanelScreenTransform.localPosition;
            }

            _floatingScreen.SetRootViewController(this, AnimationType.None);
            _initializedWithGlobalLeaderboards = true;
            OnViewLoaded(true);
        }

        private void OnViewLoaded(bool isLoaded)
        {
            Plugin.logger.Debug("view loaded");
            if (!isLoaded || ssShown)
            {
                UnYeetSS();
                HideMissionLeaderboard();

            }
            else if (isLoaded && !ssShown)
            {
                YeetSS();
                ShowMissionLeaderboard();
            }

            AdjustIcon();
        }

        public void Dispose()
        {
            if (_floatingScreen != null && _floatingScreen.gameObject != null)
            {
                GameObject.Destroy(_floatingScreen.gameObject);
            }
            UnYeetSS();
        }

        [UIAction("icon-click")]
        private void IconClick()
        {
            ssShown = !ssShown;
            OnViewLoaded(true);
        }

        private void AdjustIcon()
        {
            if (ssShown)
            {
                _toggleButtonImage.SetImage("#CampaignIcon");
                _toggleButtonImage.GetComponent<HoverHint>().text = "View Campaign Leaderboard";
            }
            else
            {
                _toggleButtonImage.sprite = AssetsManager.ScoreSaberLogo;
                _toggleButtonImage.GetComponent<HoverHint>().text = "View ScoreSaber Leaderboard";
            }
        }

        private void YeetSS()
        {
            if (_containerTransform != null && _ssLeaderboardElementsTransform != null && _ssPanelScreenTransform != null)
            {
                Plugin.logger.Debug("yeeting ss");
                _containerTransform.localPosition = new Vector3(-999, -999);
                _ssLeaderboardElementsTransform.localPosition = new Vector3(-999, -999);
                _ssPanelScreenTransform.localPosition = new Vector3(-999, -999);
            }
        }

        private void UnYeetSS()
        {
            if (_containerTransform != null && _ssLeaderboardElementsTransform != null && _ssPanelScreenTransform != null)
            {
                Plugin.logger.Debug("unyeeting ss");
                _containerTransform.localPosition = _containerPosition;
                _ssLeaderboardElementsTransform.localPosition = _ssLeaderboardElementsPosition;
                _ssPanelScreenTransform.localPosition = _ssPanelScreenPosition;
            }
        }

        private void ShowMissionLeaderboard()
        {
            Plugin.logger.Debug("showing mission leaderboard");
            _campaignMissionSecondaryLeaderboardViewController.transform.localPosition = _containerPosition;
            if (_campaignMissionSecondaryLeaderboardViewController.screen == null)
            {
                _campaignMissionSecondaryLeaderboardViewController.__Init(_platformLeaderboardViewController.screen, _platformLeaderboardViewController, null);
            }
            _campaignMissionSecondaryLeaderboardViewController.__Activate(false, false);
            _campaignMissionSecondaryLeaderboardViewController.transform.SetParent(_platformLeaderboardViewController.transform);
            _campaignMissionSecondaryLeaderboardViewController.Shown();
        }

        private void HideMissionLeaderboard()
        {
            if (_campaignMissionSecondaryLeaderboardViewController != null)
            {
                Plugin.logger.Debug("hiding mission leaderboard");
                _campaignMissionSecondaryLeaderboardViewController.__Deactivate(false, true, false);
            }
        }

        internal void CustomCampaignEnabled()
        {
            if (_config.floorLeaderboard)
            {
                return;
            }

            Plugin.logger.Debug("custom campaign enabled");

            PanelViewShowPatch.ViewShown -= OnViewActivated;
            PanelViewsIsLoadedSetterPatch.ViewLoaded -= OnViewLoaded;
            PanelViewShowPatch.ViewShown += OnViewActivated;
            PanelViewsIsLoadedSetterPatch.ViewLoaded += OnViewLoaded;

            if (_toggleButtonImage)
            {
                Plugin.logger.Debug("activating toggle button image");
                _toggleButtonImage.gameObject.SetActive(true);
            }
            //Initialize();
        }

        internal void CustomCampaignDisabled()
        {
            Plugin.logger.Debug("custom campaign disabled");

            if (_toggleButtonImage)
            {
                Plugin.logger.Debug("deactivating toggle button");
                _toggleButtonImage.gameObject.SetActive(false);
            }

            PanelViewShowPatch.ViewShown -= OnViewActivated;
            PanelViewsIsLoadedSetterPatch.ViewLoaded -= OnViewLoaded;

            if (_initializedWithGlobalLeaderboards)
            {
                Plugin.logger.Debug("was initialized");
                UnYeetSS();
                //Dispose();
                _initializedWithGlobalLeaderboards = false;
                HideMissionLeaderboard();
            }
        }

        internal void SetGlobalLeaderboardViewController()
        {
            Plugin.logger.Debug("in SetGlobalLeaderboardViewController");
            if (!_initializedWithGlobalLeaderboards)
            {
                Plugin.logger.Debug("not initialized with global leaderboards");
                OnViewActivated();
            }
            else if (!ssShown)
            {
                _campaignMissionSecondaryLeaderboardViewController.UpdateLeaderboards();
            }
        }

        private void OnMissionDataReady(Mission mission, string customURL, Color color)
        {
            _campaignMissionSecondaryLeaderboardViewController.customURL = customURL;
            _campaignMissionSecondaryLeaderboardViewController.mission = mission;
        }
    }
}
