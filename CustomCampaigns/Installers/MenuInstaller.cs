using CustomCampaigns.Managers;
using CustomCampaigns.UI;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.ViewControllers;
using SiraUtil;
using System;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<UserInfoManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.BindInterfacesTo<AssetsManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UnlockableSongsManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<LeaderboardNavigationViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignTotalLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionSecondaryLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();

            Container.BindInterfacesAndSelfTo<CustomCampaignUIManager>().AsSingle();
            Container.Bind<Downloader>().AsSingle();
            Container.Bind<CustomCampaignManager>().AsSingle();
            Container.Bind<CustomCampaignFlowCoordinator>().FromNewComponentOnNewGameObject(nameof(CustomCampaignFlowCoordinator)).AsSingle();

            InstallExternalLoaders();
        }

        private void InstallExternalLoaders()
        {
            foreach (var externalModifier in ExternalModifierManager.ExternalModifiers.Values)
            {
                InstallExternalLoader(externalModifier);
                break;
            }
        }

        private void InstallExternalLoader(External.ExternalModifier externalModifier)
        {
            if (externalModifier.HandlerLocation == null)
            {
                return;
            }

            Plugin.logger.Debug($"Installing external loader: {externalModifier.Name} ({externalModifier.HandlerType})");
            if (externalModifier.HandlerType.BaseType == typeof(MonoBehaviour))
            {
                Container.BindInterfacesAndSelfTo(externalModifier.HandlerType).FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }
            else
            {
                Container.BindInterfacesAndSelfTo(externalModifier.HandlerType).AsSingle().NonLazy();
            }
        }
    }
}
