using CustomCampaigns.Campaign.Missions;
using System.Collections.Generic;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class PerfectCutsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker, ISaberSwingRatingCounterDidFinishReceiver
    {
        private BeatmapObjectManager _beatmapObjectManager;
        private Dictionary<ISaberSwingRatingCounter, NoteCutInfo> cutNotes = new Dictionary<ISaberSwingRatingCounter, NoteCutInfo>();

        [Inject]
        public void Construct(BeatmapObjectManager beatmapObjectManager)
        {
            _beatmapObjectManager = beatmapObjectManager;
        }

        protected override void Init()
        {
            Plugin.logger.Debug("init perfect cuts");

            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min || _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Equal)
            {
                status = Status.NotClearedYet;
            }
            else
            {
                status = Status.NotFailedYet;
            }

            if (_beatmapObjectManager != null)
            {
                _beatmapObjectManager.noteWasCutEvent -= OnNoteCut;
                _beatmapObjectManager.noteWasCutEvent += OnNoteCut;
            }
        }

        public virtual void OnDestroy()
        {
            if (_beatmapObjectManager != null)
            {
                _beatmapObjectManager.noteWasCutEvent -= OnNoteCut;
            }
        }

        private void OnNoteCut(NoteController noteController, in NoteCutInfo noteCutInfo)
        {
            if (noteController.noteData.colorType == ColorType.None || !noteCutInfo.allIsOK)
            {
                return;
            }

            cutNotes.Add(noteCutInfo.swingRatingCounter, noteCutInfo);
            noteCutInfo.swingRatingCounter.UnregisterDidFinishReceiver(this);
            noteCutInfo.swingRatingCounter.RegisterDidFinishReceiver(this);
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
                    checkedValue++;
                    CheckAndUpdateStatus();
                }
                cutNotes.Remove(saberSwingRatingCounter);
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("perfectCuts");
        }
    }
}
