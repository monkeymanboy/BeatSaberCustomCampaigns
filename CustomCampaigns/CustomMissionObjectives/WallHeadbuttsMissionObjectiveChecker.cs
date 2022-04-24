using CustomCampaigns.Campaign.Missions;
using System;
using System.Collections.Generic;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class WallHeadbuttsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private HashSet<ObstacleData> headbuttedWalls = new HashSet<ObstacleData>();

        [Inject]
        public void Construct(PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction)
        {
            playerHeadAndObstacleInteraction.headDidEnterObstacleEvent += OnHeadDidEnterObstacle;
        }

        private void OnHeadDidEnterObstacle(ObstacleController obstacleController)
        {
            if (headbuttedWalls.Add(obstacleController.obstacleData))
            {
                checkedValue++;
                CheckAndUpdateStatus();
            }
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

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("wallHeadbutts");
        }
    }
}
