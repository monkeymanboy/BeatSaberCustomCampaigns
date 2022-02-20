using CustomCampaigns.CustomMissionObjectives;

namespace CustomCampaigns.Installers
{
    internal class CampaignInstaller : CustomCampaignMissionInstaller
    {
        private readonly Config _config;

        public CampaignInstaller(Config config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();

            InstallObjectiveCheckers();
            InstallExternalModifiers();

            Container.Bind<CustomMissionObjectivesManager>().AsSingle().NonLazy();
        }
    }
}
