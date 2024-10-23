using CustomCampaigns.Campaign.Missions;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class SpinsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        const int CONSECUTIVE_SEGMENTS = 5;
        const int SEGMENT_COUNT = 6;
        const int SEGMENT_SIZE = 360 / SEGMENT_COUNT;

        private PauseMenuManager _pauseMenuManager;

        private Transform _playerHeadTransform;

        private int _consecutiveTurnCounter = 0;
        private bool _cw = false; // rotating clockwise
        private int _lastSegment = 3;

        [Inject]
        public void Construct(PauseMenuManager pauseMenuManager, PlayerTransforms playerTransforms)
        {
            _pauseMenuManager = pauseMenuManager;

            _playerHeadTransform = playerTransforms.GetField<Transform, PlayerTransforms>("_headTransform");
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
        }

        public void Update()
        {
            if (_playerHeadTransform == null)
            {
                return;
            }

            if (_pauseMenuManager.enabled)
            {
                _consecutiveTurnCounter = 0;
                return;
            }

            float rotation = _playerHeadTransform.localRotation.eulerAngles.y;
            rotation -= SEGMENT_SIZE / 2;
            if (rotation < 0)
            {
                rotation += 360;
            }

            int currentSegment = (int) (rotation / SEGMENT_SIZE);
            if (currentSegment == GetSegment(_lastSegment + 1))
            {
                UpdateTurnCounter(false);
            }

            if (currentSegment == GetSegment(_lastSegment - 1))
            {
                UpdateTurnCounter(true);
            }

            _lastSegment = currentSegment;
            if (_consecutiveTurnCounter == CONSECUTIVE_SEGMENTS)
            {
                _consecutiveTurnCounter -= CONSECUTIVE_SEGMENTS;
                checkedValue++;
                CheckAndUpdateStatus();
            }
        }

        private void UpdateTurnCounter(bool isTurningClockwise)
        {
            if (_cw == isTurningClockwise)
            {
                _consecutiveTurnCounter++;
            }
            else
            {
                _consecutiveTurnCounter = 0;
            }
            _cw = isTurningClockwise;

        }

        private int GetSegment(int segment)
        {
            return segment < 0 ? segment + SEGMENT_COUNT : segment < SEGMENT_COUNT ? segment : segment - SEGMENT_COUNT;
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("spins");
        }
    }
}
