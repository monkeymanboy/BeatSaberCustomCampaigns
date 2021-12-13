using CustomCampaigns.CustomMissionObjectives;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class CampaignInstaller : CustomCampaignMissionInstaller
    {
        public override void InstallBindings()
        {
            Plugin.logger.Debug("installing campaign bindings");

            InstallObjectiveCheckers();
            InstallExternalModifiers(); // TODO: call on missionstart, onmissionend, and onmissionfail

            Container.Bind<CustomMissionObjectivesManager>().AsSingle().NonLazy();
        }
    }
}
