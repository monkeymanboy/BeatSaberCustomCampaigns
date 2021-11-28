using CustomCampaigns.Campaign.Missions;
using System;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.MaintainAccuracy
{
    public class MaintainAccuracyMissionObjectiveChecker : MissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        protected RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;

        [Inject]
        public void Construct(RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter)
        {
            _relativeScoreAndImmediateRankCounter = relativeScoreAndImmediateRankCounter;
        }

        protected override void Init()
        {
            Plugin.logger.Debug("init maintain acc");
            checkedValue = _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min ? 10000 : 0;
            status = Status.NotFailedYet;

            if (_relativeScoreAndImmediateRankCounter != null)
            {
                _relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= OnScoreUpdate;
                _relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent += OnScoreUpdate;
            }
        }

        public virtual void OnDestroy()
        {
            if (_relativeScoreAndImmediateRankCounter != null)
            {
                _relativeScoreAndImmediateRankCounter.relativeScoreOrImmediateRankDidChangeEvent -= OnScoreUpdate;
            }
        }

        protected void OnScoreUpdate()
        {
            var acc = _relativeScoreAndImmediateRankCounter.relativeScore;
            checkedValue = (int) Math.Round(acc * 10000);

            if (_missionObjective != null)
            {
                CheckAndUpdateStatus();
            }
            else
            {
                Plugin.logger.Debug("null mission objective");
            }
        }

        protected void CheckAndUpdateStatus()
        {
            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min)
            {
                if (checkedValue < _missionObjective.referenceValue)
                {
                    status = Status.Failed;
                }
            }

            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Max)
            {
                if (checkedValue > _missionObjective.referenceValue)
                {
                    status = Status.Failed;
                }
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("maintainAccuracy");
        }
    }
}
