using CustomCampaigns.Campaign.Missions;
using System;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives.Accuracy
{
    public class AccuracyMissionObjectiveChecker : MissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        protected RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRankCounter;

        [Inject]
        public void Construct(RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter)
        {
            Plugin.logger.Debug("construct acc");
            _relativeScoreAndImmediateRankCounter = relativeScoreAndImmediateRankCounter;
        }

        protected override void Init()
        {
            Plugin.logger.Debug("init acc");
            checkedValue = _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min ? 10000 : 0;
            status = Status.NotClearedYet;
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
            //Plugin.logger.Debug($"score update: {acc}");
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
                //Plugin.logger.Debug($"{checkedValue} / {_missionObjective.referenceValue}");
                if (checkedValue >= _missionObjective.referenceValue)
                {
                    status = Status.Cleared;
                    return;
                }
                else
                {
                    status = Status.NotClearedYet;
                    return;
                }
            }
            else if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Max)
            {
                if (checkedValue <= _missionObjective.referenceValue)
                {
                    status = MissionObjectiveChecker.Status.Cleared;
                    return;
                }
                else
                {
                    status = Status.None;
                    return;
                }
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("accuracy");
        }
    }
}
