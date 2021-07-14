using IPA.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class WallHeadbuttMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
    {
        protected PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction;
        protected List<ObstacleData> headbuttedWalls = new List<ObstacleData>();


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
                playerHeadAndObstacleInteraction = Resources.FindObjectsOfTypeAll<ScoreController>().LastOrDefault()?.GetField<PlayerHeadAndObstacleInteraction, ScoreController>("_playerHeadAndObstacleInteraction");
                if (playerHeadAndObstacleInteraction == null)
                    yield return new WaitForSeconds(0.1f);
                else
                    loaded = true;
            }

            yield return new WaitForSeconds(0.1f);
        }

        public void Update()
        {
            if (playerHeadAndObstacleInteraction == null || playerHeadAndObstacleInteraction.intersectingObstacles.Count == 0) return;
            foreach (ObstacleController obstacleController in playerHeadAndObstacleInteraction.intersectingObstacles)
            {
                if (!headbuttedWalls.Contains(obstacleController.obstacleData)) headbuttedWalls.Add(obstacleController.obstacleData);
                base.checkedValue = headbuttedWalls.Count();
                CheckAndUpdateStatus();
            }
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("wallHeadbutts");
        }
    }
}
