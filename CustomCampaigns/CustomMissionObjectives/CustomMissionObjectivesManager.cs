using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class CustomMissionObjectivesManager
    {
        private MissionObjectiveCheckersManager _missionObjectiveCheckersManager;
        private List<MissionObjective> _missionObjectives = new List<MissionObjective>();
        private List<MissionObjectiveChecker> _missionObjectiveCheckers = new List<MissionObjectiveChecker>();
        private List<MissionObjectiveChecker> _activeMissionObjectiveCheckers = new List<MissionObjectiveChecker>();

        private bool objectiveListInitialized = false;

        public void Register(MissionObjectiveChecker missionObjectiveChecker)
        {
            Plugin.logger.Debug($"Registering: {missionObjectiveChecker.name}");
            lock (_activeMissionObjectiveCheckers)
            {
                // Don't register our custom mission objective checkers until the base game has dealt with its objective checkers
                if (!objectiveListInitialized)
                {
                    Plugin.logger.Debug("Base game mission objective checkers manager hasn't initialized yet; waiting to initialize");
                    _missionObjectiveCheckers.Add(missionObjectiveChecker);
                    return;
                }

                CheckMissionObjectiveChecker(missionObjectiveChecker);
            }
        }

        public CustomMissionObjectivesManager(MissionLevelGameplayManager missionLevelGameplayManager)
        {
            Plugin.logger.Debug("custom mission objectives manager");

            _missionObjectiveCheckersManager = missionLevelGameplayManager.GetField<MissionObjectiveCheckersManager, MissionLevelGameplayManager>("_missionObjectiveCheckersManager");

            _missionObjectiveCheckersManager.objectivesListDidChangeEvent -= OnObjectivesListDidChange;
            _missionObjectiveCheckersManager.objectivesListDidChangeEvent += OnObjectivesListDidChange;
        }

        private void OnObjectivesListDidChange()
        {
            Plugin.logger.Debug("objective list change");
            _missionObjectiveCheckersManager.objectivesListDidChangeEvent -= OnObjectivesListDidChange;

            lock (_activeMissionObjectiveCheckers)
            {
                var activeMissionObjectiveCheckers = _missionObjectiveCheckersManager.GetField<MissionObjectiveChecker[], MissionObjectiveCheckersManager>("_activeMissionObjectiveCheckers");
                var missionObjectives = _missionObjectiveCheckersManager.GetField<MissionObjectiveCheckersManager.InitData, MissionObjectiveCheckersManager>("_initData").missionObjectives;
                _activeMissionObjectiveCheckers = activeMissionObjectiveCheckers.ToList();
                _missionObjectives = missionObjectives.ToList();

                // TODO: fix inefficiency? Should never be enough checkers to make a difference...
                foreach (var missionObjectiveChecker in _missionObjectiveCheckers)
                {
                    CheckMissionObjectiveChecker(missionObjectiveChecker);
                }

                _missionObjectiveCheckers.Clear();
                objectiveListInitialized = true;
            }
        }

        private void CheckMissionObjectiveChecker(MissionObjectiveChecker missionObjectiveChecker)
        {
            foreach (MissionObjective missionObjective in _missionObjectives)
            {
                var customObjectiveChecker = missionObjectiveChecker as ICustomMissionObjectiveChecker;
                if (customObjectiveChecker.GetMissionObjectiveType() == missionObjective.type.objectiveName)
                {
                    Plugin.logger.Debug($"Found custom: { missionObjective.type.objectiveName}");
                    missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);

                    _activeMissionObjectiveCheckers.Add(missionObjectiveChecker);
                    _missionObjectives.Remove(missionObjective);

                    missionObjectiveChecker.statusDidChangeEvent += _missionObjectiveCheckersManager.HandleMissionObjectiveCheckerStatusDidChange;

                    _missionObjectiveCheckersManager.SetField("_activeMissionObjectiveCheckers", _activeMissionObjectiveCheckers.ToArray());
                    _missionObjectiveCheckersManager.GetField<Action, MissionObjectiveCheckersManager>("objectivesListDidChangeEvent")?.Invoke();
                    return;
                }
            }

            // No matching mission objective for this checker
            GameObject.Destroy(missionObjectiveChecker);
        }
    }
}
