using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Zenject;

namespace CustomCampaigns.Managers
{
    class CustomMissionObjectivesStandardLevelManager
    {
        private CustomMissionObjectivesUIController _customMissionObjectivesUIController;

        private List<MissionObjective> _missionObjectives = new List<MissionObjective>();
        private List<MissionObjectiveChecker> _activeMissionObjectiveCheckers = new List<MissionObjectiveChecker>();

        internal void Register(MissionObjectiveChecker missionObjectiveChecker)
        {
            Plugin.logger.Debug($"Registering: {missionObjectiveChecker}");
            CheckMissionObjectiveChecker(missionObjectiveChecker);
        }

        public CustomMissionObjectivesStandardLevelManager(CustomMissionObjectivesUIController customMissionObjectivesUIController, ILevelEndActions gameplayManager)
        {
            _customMissionObjectivesUIController = customMissionObjectivesUIController;
            gameplayManager.levelFailedEvent += OnLevelFailed;
            gameplayManager.levelFinishedEvent += OnLevelFinished;

            _missionObjectives = CustomCampaignManager.currentMissionData.missionObjectives.ToList();
        }

        // TODO: MissionClearedEnvironmentEffect - does anything even use this???
        private void HandleMissionObjectiveCheckerStatusDidChange(MissionObjectiveChecker missionObjectiveChecker)
        {
            //throw new NotImplementedException();
        }

        private void OnLevelFinished()
        {
            Plugin.logger.Debug("on level finished");
            CustomCampaignManager.missionObjectiveResults = GetResults();
        }

        private void OnLevelFailed()
        {
            Plugin.logger.Debug("on level failed");
            CustomCampaignManager.missionObjectiveResults = GetResults();
        }

        private MissionObjectiveResult[] GetResults()
        {
            MissionObjectiveResult[] missionObjectiveResults = new MissionObjectiveResult[_activeMissionObjectiveCheckers.Count];
            int i = 0;
            foreach (var missionObjectiveChecker in _activeMissionObjectiveCheckers)
            {
                missionObjectiveChecker.disableChecking = true;
                //missionObjectiveChecker.statusDidChangeEvent -= this.HandleMissionObjectiveCheckerStatusDidChange;
                bool cleared = missionObjectiveChecker.status == MissionObjectiveChecker.Status.Cleared || missionObjectiveChecker.status == MissionObjectiveChecker.Status.NotFailedYet;
                int checkedValue = missionObjectiveChecker.checkedValue;
                MissionObjectiveResult missionObjectiveResult = new MissionObjectiveResult(missionObjectiveChecker.missionObjective, cleared, checkedValue);
                missionObjectiveResults[i] = missionObjectiveResult;
                i++;
            }
            return missionObjectiveResults;
        }

        private void CheckMissionObjectiveChecker(MissionObjectiveChecker missionObjectiveChecker)
        {
            Plugin.logger.Debug($"Checking: {(missionObjectiveChecker as ICustomMissionObjectiveChecker).GetMissionObjectiveType()}");
            foreach (MissionObjective missionObjective in _missionObjectives)
            {
                var customObjectiveChecker = missionObjectiveChecker as ICustomMissionObjectiveChecker;
                Plugin.logger.Debug(missionObjective.type.objectiveName);
                if (customObjectiveChecker.GetMissionObjectiveType() == missionObjective.type.objectiveName)
                {
                    Plugin.logger.Debug($"Found custom: {missionObjective.type.objectiveName}");
                    missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);

                    _activeMissionObjectiveCheckers.Add(missionObjectiveChecker);
                    _missionObjectives.Remove(missionObjective);

                    missionObjectiveChecker.statusDidChangeEvent += HandleMissionObjectiveCheckerStatusDidChange;

                    // setup ui
                    if (CustomCampaignManager.missionObjectiveGameUIViewPrefab != null)
                    {
                        _customMissionObjectivesUIController.AddUIElement(missionObjectiveChecker);
                    }
                    return;
                }
            }

            // No matching mission objective for this checker
            GameObject.Destroy(missionObjectiveChecker);
        }
    }
}
