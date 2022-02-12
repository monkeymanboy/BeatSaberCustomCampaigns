using CustomCampaigns.Managers;
using CustomCampaigns.UI;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.GameplaySetupUI;
using CustomCampaigns.UI.LeaderboardCore;
using CustomCampaigns.UI.ViewControllers;
using SiraUtil;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class MenuInstaller : Installer
    {
        private readonly Config _config;

        public MenuInstaller(Config config)
        {
            _config = config;
        }


        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();

            Container.Bind<UserInfoManager>().AsSingle().NonLazy();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            Container.BindInterfacesTo<AssetsManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UnlockableSongsManager>().AsSingle();
            Container.Bind<DownloadManager>().AsSingle();

            Container.Bind<ModalController>().AsSingle();
            Container.BindInterfacesAndSelfTo<LeaderboardNavigationViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignListViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignDetailViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignTotalLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionSecondaryLeaderboardViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<CampaignMissionLeaderboardCoreViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<CampaignPanelViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomCampaignsCustomLeaderboard>().AsSingle();

            Container.Bind<SettingsHandler>().AsSingle();
            Container.Bind<ToolsHandler>().AsSingle();
            Container.Bind<GameplaySetupManager>().AsSingle();

            Container.BindInterfacesAndSelfTo<CustomCampaignUIManager>().AsSingle();
            Container.Bind<Downloader>().AsSingle();
            Container.BindInterfacesAndSelfTo<CustomCampaignManager>().AsSingle();

            GameObjectCreationParameters gameObjectCreationParameters = new GameObjectCreationParameters();
            gameObjectCreationParameters.Name = nameof(CustomCampaignFlowCoordinator);
            Container.Bind<CustomCampaignFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            InstallExternalLoaders();

            Container.BindInterfacesAndSelfTo<ExternalModifierManager>().AsSingle().NonLazy();
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
