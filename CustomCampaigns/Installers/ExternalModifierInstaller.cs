using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.Managers;
using SiraUtil;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Installers
{
    internal abstract class ExternalModifierInstaller : Installer
    {
        protected void InstallExternalModifiers()
        {
            foreach (string modName in (CustomCampaignManager.currentMissionData as CustomMissionDataSO).mission.externalModifiers.Keys)
            {
                foreach (var externalModifier in ExternalModifierManager.ExternalModifiers.Values)
                {
                    if (externalModifier.Name == modName)
                    {
                        InstallExternalModifier(externalModifier);
                        break;
                    }
                }
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
    }
}
