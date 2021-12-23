using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.FlowCoordinators;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class ToolsHandler
    {
        private MenuTransitionsHelper _menuTransitionsHelper;
        private CreditsManager _creditsManager;

        public ToolsHandler(MenuTransitionsHelper menuTransitionsHelper, CreditsManager creditsManager)
        {
            _menuTransitionsHelper = menuTransitionsHelper;
            _creditsManager = creditsManager;
        }

        [UIAction("credits-click")]
        public void ShowCredits()
        {
            Plugin.logger.Debug("credits :D");
            _menuTransitionsHelper.GetField<CreditsScenesTransitionSetupDataSO, MenuTransitionsHelper>("_creditsScenesTransitionSetupData").didFinishEvent -= _creditsManager.OnCreditsFinish;
            _creditsManager.StartingCustomCampaignCredits(CustomCampaignFlowCoordinator.CustomCampaignManager.Campaign);
            _menuTransitionsHelper.ShowCredits();
        }

    }
}
