using CustomCampaigns.Campaign.Missions;
using System;
using System.Collections.Generic;
using Zenject;
using static ScoreModel;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class PerfectCutsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private ScoreController _scoreController;

        [Inject]
        public void Construct(ScoreController scoreController)
        {
            _scoreController = scoreController;
        }

        public override void Init()
        {
            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min || _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Equal)
            {
                status = Status.NotClearedYet;
            }
            else
            {
                status = Status.NotFailedYet;
            }

            if (_scoreController != null)
            {
                _scoreController.scoringForNoteFinishedEvent -= OnScoringForNoteFinished;
                _scoreController.scoringForNoteFinishedEvent += OnScoringForNoteFinished;
            }
        }

        private void OnScoringForNoteFinished(ScoringElement scoringElement)
        {
            if (scoringElement is GoodCutScoringElement goodCutScoringElement)
            {
                IReadonlyCutScoreBuffer cutScoreBuffer = goodCutScoringElement.cutScoreBuffer;
                // Only count perfect cuts for notes that have acc component
                if (cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore > 0)
                {
                    if (cutScoreBuffer.cutScore == cutScoreBuffer.maxPossibleCutScore)
                    {
                        checkedValue++;
                        CheckAndUpdateStatus();
                    }
                }
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("perfectCuts");
        }
    }
}
