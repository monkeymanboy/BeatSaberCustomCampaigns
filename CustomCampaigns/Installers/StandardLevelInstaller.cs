using CustomCampaigns.Managers;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using Zenject;

namespace CustomCampaigns.Installers
{
    class StandardLevelInstaller : CustomCampaignMissionInstaller
    {
        private readonly Config _config;
        public StandardLevelInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            if (CustomCampaignManager.isCampaignLevel)
            {
                Container.BindInstance(_config).AsSingle();

                InstallObjectiveCheckers();
                InstallExternalModifiers();

                Container.Bind<CustomMissionObjectivesUIController>().FromNewComponentOnNewGameObject().AsSingle();
                Container.BindInterfacesAndSelfTo<CustomMissionObjectivesStandardLevelManager>().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<ScoreSubmissionManager>().AsSingle().NonLazy();
            }
        }
    }
}
