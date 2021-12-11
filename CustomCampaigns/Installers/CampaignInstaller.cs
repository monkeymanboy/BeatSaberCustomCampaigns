using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.CustomMissionObjectives.Accuracy;
using CustomCampaigns.CustomMissionObjectives.BombsHit;
using CustomCampaigns.CustomMissionObjectives.HeadTimeInWall;
using CustomCampaigns.CustomMissionObjectives.MaintainAccuracy;
using CustomCampaigns.CustomMissionObjectives.PerfectCuts;
using CustomCampaigns.CustomMissionObjectives.SaberTimeInWall;
using CustomCampaigns.CustomMissionObjectives.Spins;
using CustomCampaigns.CustomMissionObjectives.WallHeadbutts;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class CampaignInstaller : ExternalModifierInstaller
    {
        public override void InstallBindings()
        {
            Plugin.logger.Debug("installing campaign bindings");

            InstallObjectiveCheckers();
            InstallExternalModifiers(); // TODO: call on missionstart, onmissionend, and onmissionfail

            Container.Bind<CustomMissionObjectivesManager>().AsSingle().NonLazy();
        }

        private void InstallObjectiveCheckers()
        {
            Container.BindInterfacesAndSelfTo<AccuracyMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BombsHitMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HeadTimeInWallMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MaintainAccuracyMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PerfectCutsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaberTimeInWallMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpinsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WallHeadbuttsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
