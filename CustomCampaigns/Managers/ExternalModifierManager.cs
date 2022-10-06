using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.External;
using CustomCampaigns.External.Interfaces;
using CustomCampaigns.HarmonyPatches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Zenject;

namespace CustomCampaigns.Managers
{
    public class ExternalModifierManager : IInitializable, IDisposable
    {
        internal static Dictionary<Assembly, ExternalModifier> ExternalModifiers { get; private set; } = new Dictionary<Assembly, ExternalModifier>();
        internal static Dictionary<Assembly, ExternalModifier> ExternalModifiersToInstall { get; private set; } = new Dictionary<Assembly, ExternalModifier>();

        private List<IModifierHandler> _modifierHandlers;
        private List<INotifyMissionCompletionResults> _notifyMissionCompletionResults;

        private Mission _currentMission;

        public ExternalModifierManager(List<IModifierHandler> modifierHandlers, List<INotifyMissionCompletionResults> notifyMissionCompletionResults)
        {
            Plugin.logger.Debug("external modifier manager");
            _modifierHandlers = modifierHandlers;
            _notifyMissionCompletionResults = notifyMissionCompletionResults;
        }

        public void Initialize()
        {
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish -= OnMissionSceneFinish;
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish += OnMissionSceneFinish;
        }

        public async Task<(Dictionary<string, string>, HashSet<string>, Dictionary<string, string>)> CheckForModLoadIssues(Mission mission)
        {
            _currentMission = mission;

            // requiredModFailures, missingOptionalMods, optionalModFailures
            (Dictionary<string, string>, HashSet<string>, Dictionary<string, string>) modFailures = (new Dictionary<string, string>(), new HashSet<string>(), new Dictionary<string, string>());

            List<IModifierHandler> currentMissionHandlers = new List<IModifierHandler>();

            ExternalModifiersToInstall = new Dictionary<Assembly, ExternalModifier>();

            foreach (var kvp in ExternalModifiers)
            {
                var externalModifier = kvp.Value;
                if (!mission.externalModifiers.ContainsKey(externalModifier.Name) && !mission.optionalExternalModifiers.ContainsKey(externalModifier.Name))
                {
                    continue;
                }

                if (externalModifier.HandlerLocation == null)
                {
                    Plugin.logger.Debug("null handler location");
                    continue;
                }

                var handlers = _modifierHandlers.Where(x => x.GetType() == externalModifier.HandlerType);
                if (handlers.Count() == 0)
                {
                    Plugin.logger.Debug("no handlers");
                    continue;
                }

                var handler = handlers.First();
                currentMissionHandlers.Add(handler);

                if (handler != null)
                {
                    ExternalModifiersToInstall.Add(kvp.Key, kvp.Value);
                    if (mission.externalModifiers.ContainsKey(externalModifier.Name))
                    {
                        if (!await handler.OnLoadMission(mission.externalModifiers[externalModifier.Name], mission))
                        {
                            string failureReason = (handler is IModifierFailureReasonProvider) ?
                                                   (handler as IModifierFailureReasonProvider).GetFailureReason()
                                                  : "";
                            modFailures.Item1.Add(externalModifier.Name, failureReason);
                        }
                    }
                    else
                    {
                        if (!await handler.OnLoadMission(mission.optionalExternalModifiers[externalModifier.Name], mission))
                        {
                            string failureReason = (handler is IModifierFailureReasonProvider) ?
                                                   (handler as IModifierFailureReasonProvider).GetFailureReason()
                                                  : "";
                            modFailures.Item3.Add(externalModifier.Name, failureReason);
                        }
                    }
                }
            }

            Plugin.logger.Debug("checking required external mods");
            foreach (var modName in mission.externalModifiers.Keys)
            {
                CheckExternalModifier(modName, ref modFailures.Item1);
            }

            Plugin.logger.Debug("checking optional external mods");
            foreach (var modName in mission.optionalExternalModifiers.Keys)
            {
                CheckExternalModifier(modName, ref modFailures.Item2);
            }

            if (modFailures.Item1.Count > 0)
            {
                foreach (var handler in currentMissionHandlers)
                {
                    handler.OnMissionFailedToLoad(mission);
                }
            }

            return modFailures;
        }

        private void CheckExternalModifier(string modName, ref HashSet<string> modFailures)
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

        private void CheckExternalModifier(string modName, ref Dictionary<string, string> modFailures)
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
                modFailures.Add(modName, "");
            }
        }

        public void OnMissionSceneFinish(MissionLevelScenesTransitionSetupDataSO _, MissionCompletionResults missionCompletionResults)
        {
            Plugin.logger.Debug("mission scene finish");

            foreach (var modifierHandler in _modifierHandlers)
            {
                modifierHandler.OnMissionEnd(_currentMission);
            }

            foreach (var notifyMissionCompletionResults in _notifyMissionCompletionResults)
            {
                notifyMissionCompletionResults.OnMissionCompletionResultsAvailable(missionCompletionResults);
            }
        }

        public void Dispose()
        {
            CampaignFlowCoordinatorHandleMissionLevelSceneDidFinishPatch.onMissionSceneFinish -= OnMissionSceneFinish;
        }
    }
}
