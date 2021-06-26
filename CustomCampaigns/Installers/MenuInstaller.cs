using Zenject;
using SiraUtil;
using System;
using System.Collections.Generic;
using System.Text;
using CustomCampaigns.UI;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.ViewControllers;
using CustomCampaigns.Managers;

namespace CustomCampaigns.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<UserInfoManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.Bind<CampaignListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomCampaignUIManager>().AsSingle();
            Container.Bind<Downloader>().AsSingle();
            Container.Bind<CustomCampaignManager>().AsSingle();
            Container.Bind<CustomCampaignFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(CustomCampaignFlowCoordinator)).AsSingle();
        }
    }
}
