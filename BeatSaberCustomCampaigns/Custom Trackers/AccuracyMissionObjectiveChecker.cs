using System.Collections;
using System.Linq;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class AccuracyMissionObjectiveChecker : MissionObjectiveChecker, CustomTracker
    {
        protected ScoreController scoreController;

        protected override void Init()
        {
            StartCoroutine(WaitForLoad());
            base.checkedValue = _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min ? 10000 : 0;
            base.status = Status.NotClearedYet;
        }

        IEnumerator WaitForLoad()
        {
            bool loaded = false;
            while (!loaded)
            {
                scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().FirstOrDefault();
                if (scoreController == null)
                    yield return new WaitForSeconds(0.1f);
                else
                    loaded = true;
            }

            yield return new WaitForSeconds(0.1f);
            scoreController.immediateMaxPossibleScoreDidChangeEvent -= HandleScoreControllerImmediateMaxPossibleScoreDidChange;
            scoreController.immediateMaxPossibleScoreDidChangeEvent += HandleScoreControllerImmediateMaxPossibleScoreDidChange;
        }
        public virtual void OnDestroy()
        {
            if (scoreController != null)
            {
                scoreController.immediateMaxPossibleScoreDidChangeEvent -= HandleScoreControllerImmediateMaxPossibleScoreDidChange;
            }

        }

        public virtual void HandleScoreControllerImmediateMaxPossibleScoreDidChange(int immediateMaxPossibleScore, int immediateMaxPossibleModifiedScore)
        {
            base.checkedValue = (int)(((float)scoreController.prevFrameRawScore / (float)immediateMaxPossibleScore)*10000);
            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min)
            {
                if (base.checkedValue >= _missionObjective.referenceValue)
                {
                    base.status = Status.Cleared;
                } else
                {
                    base.status = Status.NotClearedYet;
                }
            }

            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Max)
            {
                if (base.checkedValue <= _missionObjective.referenceValue)
                {
                    base.status = Status.Cleared;
                }
                else
                {
                    base.status = Status.NotClearedYet;
                }
            }
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("accuracy");
        }
    }
}
