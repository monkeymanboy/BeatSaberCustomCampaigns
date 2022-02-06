using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.External;
using CustomCampaigns.Managers;
using SiraUtil;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal abstract class CustomCampaignMissionInstaller : Installer
    {
        protected void InstallExternalModifiers()
        {
            foreach (var externalModifier in ExternalModifierManager.ExternalModifiersToInstall.Values)
            {
                InstallExternalModifier(externalModifier);
            }
        }

        protected void InstallExternalModifier(ExternalModifier externalModifier)
        {
            if (externalModifier.ModifierLocation == null)
            {
                return;
            }

            Plugin.logger.Debug($"Installing external modifier: {externalModifier.Name} ({externalModifier.ModifierType})");
            if (externalModifier.ModifierType.BaseType == typeof(MonoBehaviour))
            {
                Container.BindInterfacesAndSelfTo(externalModifier.ModifierType).FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            }
            else
            {
                Container.BindInterfacesAndSelfTo(externalModifier.ModifierType).AsSingle().NonLazy();
            }
        }

        protected void InstallObjectiveCheckers()
        {
            Container.BindInterfacesAndSelfTo<AccuracyMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BombsHitMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<HeadTimeInWallMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<MaintainAccuracyMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PerfectCutsMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SaberTimeInWallMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SpinsMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<WallHeadbuttsMissionObjectiveChecker>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
    }
}
