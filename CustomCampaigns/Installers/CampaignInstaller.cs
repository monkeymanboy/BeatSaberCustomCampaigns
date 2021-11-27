using CustomCampaigns.CustomMissionObjectives;
using SiraUtil;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class CampaignInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.logger.Debug("installing campaign bindings");
            Container.Bind<AccuracyMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle();
            Container.Bind<CustomMissionObjectivesManager>().AsSingle().NonLazy();
        }
    }
}
