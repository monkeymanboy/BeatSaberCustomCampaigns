using CustomCampaigns.Managers;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using Zenject;

namespace CustomCampaigns.Installers
{
    class StandardLevelInstaller : CustomCampaignMissionInstaller
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
    }
}
