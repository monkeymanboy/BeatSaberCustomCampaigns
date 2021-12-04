using CustomCampaigns.CustomMissionObjectives;
using CustomCampaigns.External;
using CustomCampaigns.UI.MissionObjectiveGameUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Managers
{
    class CustomMissionObjectivesStandardLevelManager : IInitializable
    {
        private BeatmapObjectManager _beatmapObjectManager;
        private IScoreController _scoreController;
        private SaberActivityCounter _saberActivityCounter;

        private CustomMissionObjectivesUIController _customMissionObjectivesUIController;

        private List<MissionObjective> _missionObjectives = new List<MissionObjective>();
        private List<MissionObjectiveChecker> _activeMissionObjectiveCheckers = new List<MissionObjectiveChecker>();

        private List<IMissionModifier> _missionModifiers;

        internal void Register(MissionObjectiveChecker missionObjectiveChecker)
        {
            Plugin.logger.Debug($"Registering: {missionObjectiveChecker}");
            CheckMissionObjectiveChecker(missionObjectiveChecker);
        }

        public CustomMissionObjectivesStandardLevelManager(CustomMissionObjectivesUIController customMissionObjectivesUIController, ILevelEndActions gameplayManager,
                                                            BeatmapObjectManager beatmapObjectManager, IScoreController scoreController, SaberActivityCounter saberActivityCounter,
                                                            List<IMissionModifier> missionModifiers)
        {
            _beatmapObjectManager = beatmapObjectManager;
            _scoreController = scoreController;
            _saberActivityCounter = saberActivityCounter;

            _customMissionObjectivesUIController = customMissionObjectivesUIController;
            gameplayManager.levelFailedEvent += OnLevelFailed;
            gameplayManager.levelFinishedEvent += OnLevelFinished;

            _missionObjectives = CustomCampaignManager.currentMissionData.missionObjectives.ToList();

            _missionModifiers = missionModifiers;
            Plugin.logger.Debug($"{missionModifiers.Count}");
        }

        public void Initialize()
        {
            HashSet<MissionObjective> missionObjectivesToRemove = new HashSet<MissionObjective>();

            foreach (var missionObjective in _missionObjectives)
            {
                MissionObjectiveChecker missionObjectiveChecker = GetBaseGameCheckerForObjective(missionObjective);
                if (missionObjectiveChecker != null)
                {
                    missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);

                    _activeMissionObjectiveCheckers.Add(missionObjectiveChecker);
                    missionObjectivesToRemove.Add(missionObjective);

                    missionObjectiveChecker.statusDidChangeEvent += HandleMissionObjectiveCheckerStatusDidChange;

                    // setup ui
                    if (CustomCampaignManager.missionObjectiveGameUIViewPrefab != null)
                    {
                        _customMissionObjectivesUIController.AddUIElement(missionObjectiveChecker);
                    }
                }
            }

            foreach (var missionObjective in missionObjectivesToRemove)
            {
                _missionObjectives.Remove(missionObjective);
            }

            foreach (var missionModifier in _missionModifiers)
            {
                Plugin.logger.Debug($"Telling {missionModifier} it's starting");
                missionModifier.OnMissionStart();
            }
        }

        private MissionObjectiveChecker GetBaseGameCheckerForObjective(MissionObjective missionObjective)
        {
            Plugin.logger.Debug($"Looking for: {missionObjective.type.objectiveName}");
            switch (missionObjective.type.objectiveName)
            {
                case "OBJECTIVE_MISS":
                    var missMissionObjectiveChecker = new GameObject().AddComponent<MissMissionObjectiveChecker>();
                    missMissionObjectiveChecker.SetField("_beatmapObjectManager", _beatmapObjectManager);
                    return missMissionObjectiveChecker;

                case "OBJECTIVE_SCORE":
                    var scoreMissionObjectiveChecker = new GameObject().AddComponent<ScoreMissionObjectiveChecker>();
                    scoreMissionObjectiveChecker.SetField("_scoreController", _scoreController);
                    return scoreMissionObjectiveChecker;

                case "OBJECTIVE_HANDS_MOVEMENT":
                    var handsMovementMissionObjectiveChecker = new GameObject().AddComponent<HandsMovementMissionObjectiveChecker>();
                    handsMovementMissionObjectiveChecker.SetField("_saberActivityCounter", _saberActivityCounter);
                    return handsMovementMissionObjectiveChecker;

                case "OBJECTIVE_COMBO":
                    var comboMissionObjectiveChecker = new GameObject().AddComponent<ComboMissionObjectiveChecker>();
                    comboMissionObjectiveChecker.SetField("_scoreController", _scoreController);
                    return comboMissionObjectiveChecker;

                case "OBJECTIVE_FULL_COMBO":
                    var fullComboMissionObjectiveChecker = new GameObject().AddComponent<FullComboMissionObjectiveChecker>();
                    fullComboMissionObjectiveChecker.SetField("_scoreController", _scoreController);
                    return fullComboMissionObjectiveChecker;

                case "OBJECTIVE_BAD_CUTS":
                    var badCutsMissionObjectiveChecker = new GameObject().AddComponent<BadCutsMissionObjectiveChecker>();
                    badCutsMissionObjectiveChecker.SetField("_beatmapObjectManager", _beatmapObjectManager);
                    return badCutsMissionObjectiveChecker;

                default:
                    return null;
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

            foreach (var missionModifier in _missionModifiers)
            {
                missionModifier.OnMissionEnd();
            }
        }

        private void OnLevelFailed()
        {
            Plugin.logger.Debug("on level failed");
            CustomCampaignManager.missionObjectiveResults = GetResults();

            foreach (var missionModifier in _missionModifiers)
            {
                missionModifier.OnMissionFail();
            }
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
