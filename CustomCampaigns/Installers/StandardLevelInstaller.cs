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
using Zenject;

namespace CustomCampaigns.Installers
{
    class StandardLevelInstaller : ExternalModifierInstaller
    {
        public override void InstallBindings()
        {
            if (CustomCampaignManager.isCampaignLevel)
            {
                InstallObjectiveCheckers();
                InstallExternalModifiers();

                Container.Bind<CustomMissionObjectivesUIController>().FromNewComponentOnNewGameObject().AsSingle();
                Container.BindInterfacesAndSelfTo<CustomMissionObjectivesStandardLevelManager>().AsSingle().NonLazy();
                Container.BindInterfacesAndSelfTo<ScoreSubmissionManager>().AsSingle().NonLazy();
            }
        }

        private void InstallObjectiveCheckers()
        {
            Container.BindInterfacesAndSelfTo<AccuracyMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BombsHitMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HeadTimeInWallMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MaintainAccuracyMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PerfectCutsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaberTimeInwallMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpinsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WallHeadbuttsMissionObjectiveCheckerStandard>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
