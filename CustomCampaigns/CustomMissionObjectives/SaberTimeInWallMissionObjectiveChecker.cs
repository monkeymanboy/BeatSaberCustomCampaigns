using CustomCampaigns.Campaign.Missions;
using System;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.CustomMissionObjectives
{
    public class SaberTimeInWallMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, ICustomMissionObjectiveChecker
    {
        private BeatmapObjectManager _beatmapObjectManager;
        private PauseMenuManager _pauseMenuManager;

        private SaberManager _saberManager;
        private Saber leftSaber;
        private Saber rightSaber;

        private float saberInWallTime = 0f;

        [Inject]
        public void Construct(SaberManager saberManager, BeatmapObjectManager beatmapObjectManager, PauseMenuManager pauseMenuManager)
        {
            _saberManager = saberManager;
            _beatmapObjectManager = beatmapObjectManager;
            _pauseMenuManager = pauseMenuManager;
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

            if (_saberManager != null)
            {
                leftSaber = _saberManager.leftSaber;
                rightSaber = _saberManager.rightSaber;
            }
        }

        public void Update()
        {
            if (leftSaber == null || _pauseMenuManager.enabled)
            {
                return;
            }

            bool leftIntersectingFound = false;
            bool rightIntersectingFound = false;
            foreach (ObstacleController obstacleController in _beatmapObjectManager.activeObstacleControllers)
            {
                if (!leftIntersectingFound)
                {
                    leftIntersectingFound = CheckObstacle(obstacleController, leftSaber);
                }

                if (!rightIntersectingFound)
                {
                    rightIntersectingFound |= CheckObstacle(obstacleController, rightSaber);
                }

                if (leftIntersectingFound && rightIntersectingFound)
                {
                    break;
                }
            }
        }

        private bool CheckObstacle(ObstacleController obstacleController, Saber saber)
        {
            if (saber.isActiveAndEnabled && isSaberInWall(obstacleController, saber))
            {
                saberInWallTime += Time.deltaTime;
                checkedValue = (int) Math.Round(saberInWallTime * 1000);
                CheckAndUpdateStatus();
                return true;
            }
            return false;
        }

        private bool isSaberInWall(ObstacleController obstacleController, Saber saber)
        {
            var saberBladeBotomPos = saber.saberBladeBottomPos;
            var saberBladeTopPos = saber.saberBladeTopPos;

            // Translate pos to obstacle local space
            saberBladeBotomPos = obstacleController.transform.InverseTransformPoint(saberBladeBotomPos);
            saberBladeTopPos = obstacleController.transform.InverseTransformPoint(saberBladeTopPos);

            var saberLength = Vector3.Distance(saberBladeBotomPos, saberBladeTopPos);
            Vector3 saberDirection = (saberBladeTopPos - saberBladeBotomPos).normalized;

            return (obstacleController.bounds.IntersectRay(new Ray(saberBladeBotomPos, saberDirection), out float distance) && distance <= saberLength);
        }

        public string GetMissionObjectiveType()
        {
            return MissionRequirement.GetObjectiveName("saberInWall");
        }
    }
}
