using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using System.Collections.Generic;
using UnityEngine;

using Zenject;

namespace CustomCampaigns.Managers
{
    class CampaignMissionInStandardLevelManager : IInitializable
    {
        List<MissionObjectiveChecker> _activeMissionObjectiveCheckers;
        AccuracyMissionObjectiveChecker _accuracyMissionObjectiveChecker;
        CustomMissionObjectivesUIController _customMissionObjectivesUIController;

        public CampaignMissionInStandardLevelManager(AccuracyMissionObjectiveChecker accuracyMissionObjectiveChecker, CustomMissionObjectivesUIController customMissionObjectivesUIController, ILevelEndActions gameplayManager)
        {
            _accuracyMissionObjectiveChecker = accuracyMissionObjectiveChecker;
            _customMissionObjectivesUIController = customMissionObjectivesUIController;
            gameplayManager.levelFailedEvent += OnLevelFailed;
            gameplayManager.levelFinishedEvent += OnLevelFinished;
        }

        public void Initialize()
        {
            Plugin.logger.Debug("initialize");
            List<MissionObjectiveChecker> customMissionObjectiveCheckers = ConstructCustomTrackerList();
            _activeMissionObjectiveCheckers = new List<MissionObjectiveChecker>();

            var missionObjectives = CustomCampaignManager.currentMissionData.missionObjectives;
            foreach (MissionObjective missionObjective in missionObjectives)
            {
                foreach (MissionObjectiveChecker missionObjectiveChecker in customMissionObjectiveCheckers)
                {
                    var customObjectiveChecker = missionObjectiveChecker as ICustomMissionObjectiveChecker;
                    if (customObjectiveChecker.GetMissionObjectiveType() == missionObjective.type.objectiveName)
                    {
                        Plugin.logger.Debug($"Found custom: {missionObjective.type.objectiveName}");
                        missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
                        _activeMissionObjectiveCheckers.Add(missionObjectiveChecker);
                        missionObjectiveChecker.statusDidChangeEvent += HandleMissionObjectiveCheckerStatusDidChange;
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

            //setup ui
            if (CustomCampaignManager.missionObjectiveGameUIViewPrefab != null)
            {
                _customMissionObjectivesUIController.CreateUIElements(_activeMissionObjectiveCheckers);
            }
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

        private List<MissionObjectiveChecker> ConstructCustomTrackerList()
        {
            List<MissionObjectiveChecker> customTrackerList = new List<MissionObjectiveChecker>();

            customTrackerList.Add(_accuracyMissionObjectiveChecker);

            return customTrackerList;
        }
    }
}
