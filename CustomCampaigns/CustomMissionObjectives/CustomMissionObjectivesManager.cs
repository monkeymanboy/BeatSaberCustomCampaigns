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

                CheckMissionObjectiveChecker(missionObjectiveChecker, true);
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
                Plugin.logger.Debug($"active mission objective checkers: {activeMissionObjectiveCheckers.Length}");
                var missionObjectives = _missionObjectiveCheckersManager.GetField<MissionObjectiveCheckersManager.InitData, MissionObjectiveCheckersManager>("_initData").missionObjectives;
                _activeMissionObjectiveCheckers = activeMissionObjectiveCheckers.ToList();
                _missionObjectives = missionObjectives.ToList();

                // TODO: fix inefficiency? Should never be enough checkers to make a difference...
                foreach (var missionObjectiveChecker in _missionObjectiveCheckers)
                {
                    CheckMissionObjectiveChecker(missionObjectiveChecker, false);
                }

                _missionObjectiveCheckers.Clear();
                InitializeBaseGameMissionObjectiveCheckers();
                _missionObjectiveCheckersManager.GetField<Action, MissionObjectiveCheckersManager>("objectivesListDidChangeEvent")?.Invoke();
                objectiveListInitialized = true;
            }
        }

        private void InitializeBaseGameMissionObjectiveCheckers()
        {
            List<MissionObjectiveChecker> baseGameMissionObjectiveCheckers = _missionObjectiveCheckersManager.GetField<MissionObjectiveChecker[], MissionObjectiveCheckersManager>("_missionObjectiveCheckers").ToList();
            HashSet<MissionObjective> missionObjectivesToRemove = new HashSet<MissionObjective>();

            foreach (var missionObjective in _missionObjectives)
            {
                foreach (var missionObjectiveChecker in baseGameMissionObjectiveCheckers)
                {
                    if (missionObjectiveChecker.missionObjectiveType.objectiveName == missionObjective.type.objectiveName)
                    {
                        _activeMissionObjectiveCheckers.Add(missionObjectiveChecker);
                        missionObjectivesToRemove.Add(missionObjective);
                        baseGameMissionObjectiveCheckers.Remove(missionObjectiveChecker);

                        missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
                        break;
                    }
                }
            }

            _missionObjectiveCheckersManager.SetField("_activeMissionObjectiveCheckers", _activeMissionObjectiveCheckers.ToArray());
            foreach (var missionObjective in missionObjectivesToRemove)
            {
                _missionObjectives.Remove(missionObjective);
            }
            foreach (var missionObjectiveChecker in baseGameMissionObjectiveCheckers)
            {
                GameObject.Destroy(missionObjectiveChecker);
            }
        }

        private void CheckMissionObjectiveChecker(MissionObjectiveChecker missionObjectiveChecker, bool invokeListChangeEvent)
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
                    if (invokeListChangeEvent)
                    {
                        _missionObjectiveCheckersManager.GetField<Action, MissionObjectiveCheckersManager>("objectivesListDidChangeEvent")?.Invoke();
                    }
                    return;
                }
            }

            // No matching mission objective for this checker
            GameObject.Destroy(missionObjectiveChecker);
        }
    }
}
