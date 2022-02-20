using CustomCampaigns.Campaign.Missions;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class BombsHitMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private BeatmapObjectManager _beatmapObjectManager;

        [Inject]
        public void Construct(BeatmapObjectManager beatmapObjectManager)
        {
            _beatmapObjectManager = beatmapObjectManager;
        }

        protected override void Init()
        {
            Plugin.logger.Debug("init bombs hit");

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
            if (noteController.noteData.colorType == ColorType.None)
            {
                checkedValue++;
                CheckAndUpdateStatus();
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("bombsHit");
        }
    }
}
