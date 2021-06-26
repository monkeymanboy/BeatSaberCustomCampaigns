using Zenject;
using SiraUtil;
using System;
using System.Collections.Generic;
using System.Text;
using CustomCampaigns.UI;
using CustomCampaigns.UI.FlowCoordinators;
using CustomCampaigns.UI.ViewControllers;
using CustomCampaigns.CustomMissionObjectives;

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
