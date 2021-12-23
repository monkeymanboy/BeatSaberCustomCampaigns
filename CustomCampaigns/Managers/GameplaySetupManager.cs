using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using CustomCampaigns.UI.GameplaySetupUI;
using IPA.Utilities;
using System.Collections.Generic;
using System.Reflection;

namespace CustomCampaigns.Managers
{
    public class GameplaySetupManager
    {
        [UIComponent("settings-tab")]
        private Tab _settingsTab;

        [UIComponent("cc-tab")]
        private Tab _mainTab;

        private TabSelector _tabSelector;
        private int _tabIndex;

        private GameplaySetupViewController _gameplaySetupViewController;
        private SettingsHandler _settingsHandler;

        public GameplaySetupManager(GameplaySetupViewController gameplaySetupViewController, SettingsHandler settingsHandler)
        {
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsHandler = settingsHandler;
        }

        public void Setup()
        {
            Plugin.logger.Debug("setup");

            if (_mainTab == null)
            {
                Plugin.logger.Debug("parsing gameplaysetup");
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.gameplay-setup-settings.bsml"), _gameplaySetupViewController.transform.Find("BSMLBackground").gameObject, this);
                Plugin.logger.Debug("parsing settings");
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.settings.bsml"), _settingsTab.gameObject, _settingsHandler);

                _tabSelector = _gameplaySetupViewController.transform.Find("BSMLBackground/BSMLTabSelector").GetComponent<TabSelector>();
                _tabIndex = _tabSelector.GetField<List<Tab>, TabSelector>("tabs").Count;
                _tabSelector.GetField<List<Tab>, TabSelector>("tabs").Add(_mainTab);
                _mainTab.selector = _tabSelector;
                _tabSelector.Refresh();
            }

            _gameplaySetupViewController.didDeactivateEvent -= OnGameplaySetupDeactivated;
            _gameplaySetupViewController.didDeactivateEvent += OnGameplaySetupDeactivated;

            _mainTab.gameObject.SetActive(false);
            _mainTab.IsVisible = true;
        }

        public void CampaignExit()
        {
            _gameplaySetupViewController.didDeactivateEvent -= OnGameplaySetupDeactivated;

            if (_mainTab != null)
            {
                _mainTab.IsVisible = false;

                if (_tabSelector.GetField<int, TabSelector>("lastClickedIndex") == _tabIndex)
                {
                    Plugin.logger.Debug("reselected index");
                    _tabSelector.InvokeMethod<object, TabSelector>("TabSelected", null, 0);
                }
            }
        }

        public void OnGameplaySetupDeactivated(bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (_mainTab != null)
            {
                _mainTab.gameObject.SetActive(false);
            }
        }
    }
}
