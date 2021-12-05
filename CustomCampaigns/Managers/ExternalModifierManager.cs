using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.External.Interfaces;
using CustomCampaigns.HarmonyPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.Managers
{
    public class ExternalModifierManager : IInitializable
    {
        internal static Dictionary<Assembly, ExternalModifier> ExternalModifiers { get; private set; } = new Dictionary<Assembly, ExternalModifier>();

        private List<IModifierHandler> _modifierHandlers;
        private List<INotifyMissionCompletionResults> _notifyMissionCompletionResults;

        public ExternalModifierManager(List<IModifierHandler> modifierHandlers, List<INotifyMissionCompletionResults> notifyMissionCompletionResults)
        {
            Plugin.logger.Debug("external modifier manager");
            _modifierHandlers = modifierHandlers;
            _notifyMissionCompletionResults = notifyMissionCompletionResults;
        }

        public void Initialize()
        {
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish += OnMissionSceneFinish;
        }

        public HashSet<string> CheckForModLoadIssues(Mission mission)
        {
            HashSet<string> modFailures = new HashSet<string>();

            foreach (var externalModifier in ExternalModifiers.Values)
            {
                if (externalModifier.HandlerLocation == null)
                {
                    continue;
                }

                var handlers = _modifierHandlers.Where(x => x.GetType() == externalModifier.HandlerType);
                if (handlers.Count() == 0)
                {
                    continue;
                }

                var handler = handlers.First();
                if (handler != null && !handler.OnLoadMission(mission.externalModifiers[externalModifier.Name]))
                {
                    modFailures.Add(externalModifier.Name);
                }
            }

            foreach (var modName in mission.externalModifiers.Keys)
            {
                bool foundMod = false;
                foreach (var externalModifier in ExternalModifierManager.ExternalModifiers.Values)
                {
                    if (externalModifier.Name == modName)
                    {
                        foundMod = true;
                    }
                }

                if (!foundMod)
                {
                    modFailures.Add(modName);
                }
            }

            if (modFailures.Count > 0)
            {
                foreach (var handler in _modifierHandlers)
                {
                    handler.OnMissionFailedToLoad();
                }
            }

            return modFailures;
        }

        private void OnMissionSceneFinish(MissionLevelScenesTransitionSetupDataSO _, MissionCompletionResults missionCompletionResults)
        {
            Plugin.logger.Debug("mission scene finish");

            foreach (var modifierHandler in _modifierHandlers)
            {
                modifierHandler.OnMissionEnd();
            }
            foreach (var notifyMissionCompletionResults in _notifyMissionCompletionResults)
            {
                notifyMissionCompletionResults.OnMissionCompletionResultsAvailable(missionCompletionResults);
            }
        }
    }
}
