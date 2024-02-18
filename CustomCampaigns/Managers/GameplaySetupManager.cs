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

        [UIComponent("tools-tab")]
        private Tab _toolsTab;

        [UIComponent("cc-tab")]
        private Tab _mainTab;

        private TabSelector _tabSelector;
        private int _tabIndex;

        private GameplaySetupViewController _gameplaySetupViewController;
        private SettingsHandler _settingsHandler;
        private ToolsHandler _toolsHandler;

        public GameplaySetupManager(GameplaySetupViewController gameplaySetupViewController, SettingsHandler settingsHandler, ToolsHandler toolsHandler)
        {
            _gameplaySetupViewController = gameplaySetupViewController;
            _settingsHandler = settingsHandler;
            _toolsHandler = toolsHandler;
        }

        public void Setup(Campaign.Campaign campaign)
        {
            if (_mainTab == null)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.gameplay-setup-settings.bsml"), _gameplaySetupViewController.transform.Find("BSMLBackground").gameObject, this);
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.settings.bsml"), _settingsTab.gameObject, _settingsHandler);
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "CustomCampaigns.UI.Views.tools.bsml"), _toolsTab.gameObject, _toolsHandler);

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

            _toolsHandler.SetCampaign(campaign);
        }

        public void CampaignExit()
        {
            _gameplaySetupViewController.didDeactivateEvent -= OnGameplaySetupDeactivated;

            if (_mainTab != null)
            {
                _mainTab.IsVisible = false;

                if (_tabSelector.GetField<int, TabSelector>("currentPage") == _tabIndex)
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
