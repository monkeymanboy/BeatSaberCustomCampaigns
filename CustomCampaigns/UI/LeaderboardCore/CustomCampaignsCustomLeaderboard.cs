using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.ViewControllers;
using HMUI;
using LeaderboardCore.Managers;
using LeaderboardCore.Models;
using System;
using UnityEngine;

namespace CustomCampaigns.UI.LeaderboardCore
{
    public class CustomCampaignsCustomLeaderboard : CustomLeaderboard, IDisposable
    {
        private readonly CustomLeaderboardManager _customLeaderboardManager;

        private readonly CampaignMissionLeaderboardCoreViewController _campaignMissionLeaderboardCoreViewController;

        protected override ViewController leaderboardViewController => _campaignMissionLeaderboardCoreViewController;

        private readonly CampaignPanelViewController _campaignMissionPanelViewController;
        protected override ViewController panelViewController => _campaignMissionPanelViewController;

        internal CustomCampaignsCustomLeaderboard(CustomLeaderboardManager customLeaderboardManager, CampaignMissionLeaderboardCoreViewController campaignMissionLeaderboardCoreViewController,
                                                  CampaignPanelViewController campaignPanelViewController, CustomCampaignUIManager customCampaignUIManager)
        {
            _customLeaderboardManager = customLeaderboardManager;
            _campaignMissionLeaderboardCoreViewController = campaignMissionLeaderboardCoreViewController;
            _campaignMissionPanelViewController = campaignPanelViewController;

            customCampaignUIManager.customCampaignEnabledEvent += Register;
            customCampaignUIManager.baseCampaignEnabledEvent += Unregister;

            customCampaignUIManager.missionDataReadyEvent += OnMissionDataReady;
            customCampaignUIManager.leaderboardUpdateEvent += UpdateLeaderboards;
        }

        public void Dispose()
        {
            _customLeaderboardManager.Unregister(this);
        }

        internal void Register()
        {
            _customLeaderboardManager.Register(this);
        }

        internal void Unregister()
        {
            _customLeaderboardManager.Unregister(this);
        }

        internal void SetMission(Mission mission)
        {
            _campaignMissionLeaderboardCoreViewController.mission = mission;
            
        }

        internal void SetCustomURL(string customURL)
        {
            _campaignMissionLeaderboardCoreViewController.customURL = customURL;
        }

        internal void UpdateLeaderboards()
        {
            _campaignMissionLeaderboardCoreViewController.UpdateLeaderboards();
        }

        internal void SetColor(Color color)
        {
            _campaignMissionPanelViewController.SetColor(color);
        }

        private void OnMissionDataReady(Mission mission, string customURL, Color color)
        {
            SetMission(mission);
            SetCustomURL(customURL);
            SetColor(color);
        }
    }
}
