using CustomCampaigns.CustomMissionObjectives.Accuracy;
using CustomCampaigns.CustomMissionObjectives.BombsHit;
using CustomCampaigns.CustomMissionObjectives.HeadTimeInWall;
using CustomCampaigns.CustomMissionObjectives.MaintainAccuracy;
using CustomCampaigns.CustomMissionObjectives.PerfectCuts;
using CustomCampaigns.CustomMissionObjectives.SaberTimeInWall;
using CustomCampaigns.CustomMissionObjectives.Spins;
using CustomCampaigns.CustomMissionObjectives.WallHeadbutts;
using CustomCampaigns.Managers;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using SiraUtil;
using Zenject;

namespace CustomCampaigns.Installers
{
    class StandardLevelInstaller : Installer
    {
        public override void InstallBindings()
        {
            if (CustomCampaignManager.isCampaignLevel)
            {
                #region Mission Objective Checkers
                Container.BindInterfacesAndSelfTo<AccuracyMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<BombsHitMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<HeadTimeInWallMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<MaintainAccuracyMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<PerfectCutsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<SaberTimeInwallMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<SpinsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<WallHeadbuttsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
                #endregion

                Container.Bind<CustomMissionObjectivesUIController>().FromNewComponentOnNewGameObject().AsSingle();
                Container.Bind<CustomMissionObjectivesStandardLevelManager>().AsSingle().NonLazy();
            }
        }
    }
}
