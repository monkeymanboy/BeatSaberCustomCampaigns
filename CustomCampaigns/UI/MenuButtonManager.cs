using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using CustomCampaigns.UI.FlowCoordinators;
using System;
using Zenject;

namespace CustomCampaigns.UI
{
    internal class MenuButtonManager : IInitializable, IDisposable
    {
        private MenuButton _menuButton;
        private MainFlowCoordinator _mainFlowCoordinator;
        private CustomCampaignFlowCoordinator _customCampaignFlowCoordinator;

        public MenuButtonManager(MainFlowCoordinator mainFlowCoordinator, CustomCampaignFlowCoordinator customCampaignFlowCoordinator)
        {
            _mainFlowCoordinator = mainFlowCoordinator;
            _customCampaignFlowCoordinator = customCampaignFlowCoordinator;
            _menuButton = new MenuButton("Custom Campaigns", PresentFlowCoordinator);
        }

        public void Initialize()
        {
            MenuButtons.Instance.RegisterButton(_menuButton);
        }

        private void PresentFlowCoordinator()
        {
            _mainFlowCoordinator.PresentFlowCoordinator(_customCampaignFlowCoordinator);
        }

        public void Dispose()
        {
            if (MenuButtons.Instance != null && BSMLParser.Instance != null)
            {
                MenuButtons.Instance.UnregisterButton(_menuButton);
            }
        }
    }
}
