using CustomCampaigns.Campaign.Missions;
using System.Collections.Generic;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class WallHeadbuttsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;
        private PauseMenuManager _pauseMenuManager;

        private HashSet<ObstacleData> headbuttedWalls = new HashSet<ObstacleData>();

        [Inject]
        public void Construct(PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction, PauseMenuManager pauseMenuManager)
        {
            _playerHeadAndObstacleInteraction = playerHeadAndObstacleInteraction;
            _pauseMenuManager = pauseMenuManager;
        }

        protected override void Init()
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
            if (_playerHeadAndObstacleInteraction == null || _pauseMenuManager.enabled)
            {
                return;
            }

            foreach (ObstacleController obstacleController in _playerHeadAndObstacleInteraction.intersectingObstacles)
            {
                if (headbuttedWalls.Add(obstacleController.obstacleData))
                {
                    checkedValue++;
                    CheckAndUpdateStatus();
                }
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("wallHeadbutts");
        }
    }
}
