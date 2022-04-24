using CustomCampaigns.Campaign.Missions;
using System;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class HeadTimeInWallMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private PlayerHeadAndObstacleInteraction _playerHeadAndObstacleInteraction;
        private PauseMenuManager _pauseMenuManager;
        private float timeInWall = 0;

        [Inject]
        public void Construct(PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction, PauseMenuManager pauseMenuManager)
        {
            _playerHeadAndObstacleInteraction = playerHeadAndObstacleInteraction;
            _pauseMenuManager = pauseMenuManager;
        }

        protected override void Init()
        {
            Plugin.logger.Debug("init head time in wall");

            if (_missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Min || _missionObjective.referenceValueComparisonType == MissionObjective.ReferenceValueComparisonType.Equal)
            {
                status = Status.NotClearedYet;
            }
            else
            {
                status = Status.NotFailedYet;
            }
        }

        public void LateUpdate()
        {
            if (_playerHeadAndObstacleInteraction == null || _pauseMenuManager.enabled)
            {
                return;
            }

            if (_playerHeadAndObstacleInteraction.playerHeadIsInObstacle)
            {
                timeInWall += Time.deltaTime;
                checkedValue = (int) Math.Round(timeInWall * 1000);
                CheckAndUpdateStatus();
            }
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("headInWall");
        }
    }
}
