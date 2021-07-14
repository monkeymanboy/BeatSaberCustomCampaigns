using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BS_Utils.Utilities;
using System.Collections;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class PerfectCutMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker, ISaberSwingRatingCounterDidFinishReceiver
    {
        protected ScoreController scoreController;
        private Dictionary<ISaberSwingRatingCounter, NoteCutInfo> cutNotes = new Dictionary<ISaberSwingRatingCounter, NoteCutInfo>();

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
                scoreController = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault();
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
            cutNotes.Add(info.swingRatingCounter, info);
            info.swingRatingCounter.RegisterDidFinishReceiver(this);
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("perfectCuts");
        }

        public void HandleSaberSwingRatingCounterDidFinish(ISaberSwingRatingCounter saberSwingRatingCounter)
        {
            saberSwingRatingCounter.UnregisterDidFinishReceiver(this);
            if (cutNotes.ContainsKey(saberSwingRatingCounter))
            {
                NoteCutInfo info = cutNotes[saberSwingRatingCounter];
                ScoreModel.RawScoreWithoutMultiplier(saberSwingRatingCounter, info.cutDistanceToCenter, out int before, out int after, out int cutDist);
                int total = before + after + cutDist;
                if (total == 115)
                {
                    base.checkedValue++;
                    CheckAndUpdateStatus();
                }
                cutNotes.Remove(saberSwingRatingCounter);
            }
        }
    }
}
