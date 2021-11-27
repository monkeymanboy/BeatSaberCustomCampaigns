using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class CustomMissionObjectivesManager
    {
        MissionObjectiveCheckersManager _missionObjectiveCheckersManager;
        AccuracyMissionObjectiveChecker _accuracyMissionObjectiveChecker;
        ILevelEndActions _gameplayManager;
        private MissionObjectiveCheckersManager missionObjectiveCheckersManagerObject;
        private AccuracyMissionObjectiveChecker accuracyMissionObjectiveChecker;
        private ILevelEndActions gameplayManager;

        public CustomMissionObjectivesManager(MissionLevelGameplayManager missionLevelGameplayManager, AccuracyMissionObjectiveChecker accuracyMissionObjectiveChecker)
        {
            Plugin.logger.Debug("custom mission objectives manager");
            _missionObjectiveCheckersManager = missionLevelGameplayManager.GetField<MissionObjectiveCheckersManager, MissionLevelGameplayManager>("_missionObjectiveCheckersManager");

            _accuracyMissionObjectiveChecker = accuracyMissionObjectiveChecker;
            _gameplayManager = gameplayManager;
            _missionObjectiveCheckersManager.objectivesListDidChangeEvent -= OnObjectivesListDidChange;
            _missionObjectiveCheckersManager.objectivesListDidChangeEvent += OnObjectivesListDidChange;
        }

        private void OnObjectivesListDidChange()
        {
            Plugin.logger.Debug("objective list change");
            _missionObjectiveCheckersManager.objectivesListDidChangeEvent -= OnObjectivesListDidChange;
            List<MissionObjectiveChecker> customMissionObjectiveCheckers = ConstructCustomTrackerList();

            var activeMissionObjectiveCheckers = _missionObjectiveCheckersManager.GetField<MissionObjectiveChecker[], MissionObjectiveCheckersManager>("_activeMissionObjectiveCheckers");
            var activeMissionObjectiveCheckersList = activeMissionObjectiveCheckers.ToList();

            var missionObjectives = _missionObjectiveCheckersManager.GetField<MissionObjectiveCheckersManager.InitData, MissionObjectiveCheckersManager>("_initData").missionObjectives;
            foreach (MissionObjective missionObjective in missionObjectives)
            {
                foreach (MissionObjectiveChecker missionObjectiveChecker in customMissionObjectiveCheckers)
                {
                    var customObjectiveChecker = missionObjectiveChecker as ICustomMissionObjectiveChecker;
                    if (customObjectiveChecker.GetMissionObjectiveType() == missionObjective.type.objectiveName)
                    {
                        Plugin.logger.Debug($"Found custom: { missionObjective.type.objectiveName}");
                        missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
                        activeMissionObjectiveCheckersList.Add(missionObjectiveChecker);
                        missionObjectiveChecker.statusDidChangeEvent += _missionObjectiveCheckersManager.HandleMissionObjectiveCheckerStatusDidChange;
                        customMissionObjectiveCheckers.Remove(missionObjectiveChecker);
                        break;
                    }
                }
            }

            // remove the unused checkers
            foreach (MissionObjectiveChecker missionObjectiveChecker in customMissionObjectiveCheckers)
            {
                GameObject.Destroy(missionObjectiveChecker.gameObject);
            }

            _missionObjectiveCheckersManager.SetField("_activeMissionObjectiveCheckers", activeMissionObjectiveCheckersList.ToArray());

            _missionObjectiveCheckersManager.GetField<Action, MissionObjectiveCheckersManager>("objectivesListDidChangeEvent")?.Invoke();
        }

        private List<MissionObjectiveChecker> ConstructCustomTrackerList()
        {
            List<MissionObjectiveChecker> customTrackerList = new List<MissionObjectiveChecker>();

            customTrackerList.Add(_accuracyMissionObjectiveChecker);

            return customTrackerList;
        }
    }
}
