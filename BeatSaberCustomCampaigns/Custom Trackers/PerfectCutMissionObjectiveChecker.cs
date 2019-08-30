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

        public virtual void HandleNoteWasCut(NoteData data, NoteCutInfo info, int combo)
        {
            if (data.noteType == NoteType.Bomb || !info.allIsOK) return;
            bool didDone = false;
            info.swingRatingCounter.didFinishEvent += e =>
            {
                if (didDone) return;
                didDone = true;
                ScoreController.RawScoreWithoutMultiplier(info, out int before, out int after, out int cutDist);
                int total = before + after + cutDist;
                if (total == 115)
                {
                    base.checkedValue++;
                    CheckAndUpdateStatus();
                }
            };
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("perfectCuts");
        }
    }
}
