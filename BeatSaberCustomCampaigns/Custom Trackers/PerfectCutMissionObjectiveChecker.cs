using System.Linq;
using UnityEngine;
using System.Collections;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class PerfectCutMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
    {
        protected ScoreController scoreController;

        protected override void Init()
        {
            StartCoroutine(WaitForLoad());
            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min || _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Equal)
            {
                base.status = Status.NotClearedYet;
            }
            else
            {
                base.status = Status.NotFailedYet;
            }
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
            scoreController.noteWasCutEvent -= HandleNoteWasCut;
            scoreController.noteWasCutEvent += HandleNoteWasCut;
        }

        public virtual void OnDestroy()
        {
            if (scoreController!=null)
            {
                scoreController.noteWasCutEvent -= HandleNoteWasCut;
            }
        }

        public virtual void HandleNoteWasCut(NoteData data, in NoteCutInfo info, int multiplier)
        {
            if (data.colorType == ColorType.None || !info.allIsOK) return;

            info.swingRatingCounter.RegisterDidFinishReceiver(new PerfectCutMissionObjectiveReceiver(this, info));
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("perfectCuts");
        }

        public void IncrementCheckAndUpdateStatus()
        {
            checkedValue++;
            CheckAndUpdateStatus();
        }
    }

    public class PerfectCutMissionObjectiveReceiver : ISaberSwingRatingCounterDidFinishReceiver
    {
        private readonly PerfectCutMissionObjectiveChecker _checker;
        private readonly NoteCutInfo _noteCutInfo;

        public PerfectCutMissionObjectiveReceiver(PerfectCutMissionObjectiveChecker checker, NoteCutInfo noteCutInfo)
        {
            _checker = checker;
            _noteCutInfo = noteCutInfo;
        }

        public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter)
        {
            ScoreModel.RawScoreWithoutMultiplier(saberSwingRatingCounter, _noteCutInfo.cutDistanceToCenter, out var beforeCutRawScore, out var afterCutRawScore, out var cutDistanceRawScore);
            var total = beforeCutRawScore + afterCutRawScore + cutDistanceRawScore;

            saberSwingRatingCounter.UnregisterDidFinishReceiver(this);

            if (total == 115)
            {
                _checker.IncrementCheckAndUpdateStatus();
            }
        }
    }
}
