using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class BombsHitMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
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
            if (scoreController != null)
            {
                scoreController.noteWasCutEvent -= HandleNoteWasCut;
            }
        }

        public virtual void HandleNoteWasCut(NoteData data, NoteCutInfo info, int combo)
        {
            if (data.noteType == NoteType.Bomb)
            {
                base.checkedValue++;
                CheckAndUpdateStatus();
            }
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("bombsHit");
        }
    }
}
