using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.CustomMissionObjectives.Accuracy;
using CustomCampaigns.CustomMissionObjectives.BombsHit;
using CustomCampaigns.CustomMissionObjectives.HeadTimeInWall;
using CustomCampaigns.CustomMissionObjectives.MaintainAccuracy;
using CustomCampaigns.CustomMissionObjectives.PerfectCuts;
using CustomCampaigns.CustomMissionObjectives.SaberTimeInWall;
using CustomCampaigns.CustomMissionObjectives.Spins;
using CustomCampaigns.CustomMissionObjectives.WallHeadbutts;
using CustomCampaigns.Managers;
using SiraUtil;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal class CampaignInstaller : Installer
    {
        public override void InstallBindings()
        {
            Plugin.logger.Debug("installing campaign bindings");

            #region Mission Objective Checkers
            Container.BindInterfacesAndSelfTo<AccuracyMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BombsHitMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HeadTimeInWallMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MaintainAccuracyMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PerfectCutsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaberTimeInWallMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpinsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WallHeadbuttsMissionObjectiveCheckerMission>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            #endregion

            Container.Bind<CustomMissionObjectivesManager>().AsSingle().NonLazy();
        }
    }
}
