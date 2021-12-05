using BeatSaberMarkupLanguage;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.ViewControllers;
using HMUI;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.UI.FlowCoordinators
{
    public class CustomCampaignFlowCoordinator : FlowCoordinator
    {
        public static CustomCampaignManager CustomCampaignManager;

        private CampaignFlowCoordinator _campaignFlowCoordinator;

        private MainFlowCoordinator _mainFlowCoordinator;
        private NavigationController _campaignListNavigationController;
        private CampaignListViewController _campaignListViewController;
        private CampaignDetailViewController _campaignDetailViewController;

        private CampaignTotalLeaderboardViewController _campaignTotalLeaderboardViewController;

        [Inject]
        protected void Construct(CustomCampaignManager customCampaignManager, CampaignFlowCoordinator campaignFlowCoordinator, MainFlowCoordinator mainFlowCoordinator,
                                 CampaignListViewController campaignListViewController, CampaignDetailViewController campaignDetailViewController,
                                 CampaignTotalLeaderboardViewController campaignTotalLeaderboardViewController)
        {
            CustomCampaignManager = customCampaignManager;
            _campaignFlowCoordinator = campaignFlowCoordinator;
            _mainFlowCoordinator = mainFlowCoordinator;

            _campaignListViewController = campaignListViewController;
            _campaignListViewController.DidClickCampaignEvent += OnClickCampaign;
            _campaignDetailViewController = campaignDetailViewController;
            _campaignDetailViewController.DidClickPlayButton += OnClickPlay;

            _campaignTotalLeaderboardViewController = campaignTotalLeaderboardViewController;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                showBackButton = true;
                SetTitle("Custom Campaigns");
                _campaignListNavigationController = BeatSaberUI.CreateViewController<NavigationController>();
                CustomCampaignManager.FirstActivation();

                CustomCampaignManager.CampaignClosed += ClosedCampaign;
            }
            if (addedToHierarchy)
            {
                CustomCampaignManager.CustomCampaignEnabled();

                SetViewControllerToNavigationController(_campaignListNavigationController, _campaignListViewController);
                ProvideInitialViewControllers(_campaignListNavigationController);
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            if (removedFromHierarchy)
            {
                CustomCampaignManager.BaseCampaignEnabled();
            }
        }

        private void OnClickCampaign(Campaign.Campaign campaign)
        {
            Plugin.logger.Debug("clicked campaign");
            _campaignDetailViewController.SelectedCampaign = campaign;
            _campaignTotalLeaderboardViewController.campaignID = campaign.leaderboardId;
            if (!_campaignDetailViewController.isInViewControllerHierarchy)
            {
                Plugin.logger.Debug("pushing view controller");
                PushViewControllerToNavigationController(_campaignListNavigationController, _campaignDetailViewController);
                SetRightScreenViewController(_campaignTotalLeaderboardViewController, ViewController.AnimationType.None);

                (_campaignTotalLeaderboardViewController.table.transform.GetChild(1).GetChild(0).transform as RectTransform).localScale = new Vector3(1, 0.9f, 1);
            }

            _campaignTotalLeaderboardViewController.UpdateLeaderboards();
        }

        private void OnClickPlay(Campaign.Campaign campaign)
        {
            CustomCampaignManager.SetupCampaign(campaign, OnCampaignSetup);
        }

        private void OnCampaignSetup()
        {
            PresentFlowCoordinator(_campaignFlowCoordinator, CustomCampaignManager.FlowCoordinatorPresented);
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            _mainFlowCoordinator.DismissFlowCoordinator(this);
        }

        #region Events
        public void ClosedCampaign(CampaignFlowCoordinator campaignFlowCoordinator)
        {
            DismissFlowCoordinator(campaignFlowCoordinator);
        }
        #endregion
    }
}
