using BeatSaberMarkupLanguage.Attributes;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.FlowCoordinators;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.UI.GameplaySetupUI
{
    public class ToolsHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MenuTransitionsHelper _menuTransitionsHelper;
        private CreditsManager _creditsManager;

        [UIValue("credits-visible")]
        public bool CreditsVisible { get; private set; }

        public ToolsHandler(MenuTransitionsHelper menuTransitionsHelper, CreditsManager creditsManager)
        {
            _menuTransitionsHelper = menuTransitionsHelper;
            _creditsManager = creditsManager;
        }

        internal void SetCampaign(Campaign.Campaign campaign)
        {
            CreditsVisible = File.Exists(campaign.campaignPath + "/" + "credits.json");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CreditsVisible)));
        }

        [UIAction("credits-click")]
        public void ShowCredits()
        {
            Plugin.logger.Debug("credits :D");
            _creditsManager.StartingCustomCampaignCredits(CustomCampaignFlowCoordinator.CustomCampaignManager.Campaign);
            _menuTransitionsHelper.ShowCredits();
        }

    }
}
