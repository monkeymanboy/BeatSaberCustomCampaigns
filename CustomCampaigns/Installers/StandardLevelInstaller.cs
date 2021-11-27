using CustomCampaigns.CustomMissionObjectives;
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
                Container.Bind<AccuracyMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle();

                Container.Bind<CustomMissionObjectivesUIController>().FromNewComponentOnNewGameObject().AsSingle();
                Container.BindInterfacesAndSelfTo<CampaignMissionInStandardLevelManager>().AsSingle().NonLazy();
            }
        }
    }
}
