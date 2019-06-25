using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{

    public class TimeInWallMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
    {
        protected PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction;
        protected float currentValue;


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
                playerHeadAndObstacleInteraction = Resources.FindObjectsOfTypeAll<PlayerHeadAndObstacleInteraction>().FirstOrDefault();
                if (playerHeadAndObstacleInteraction == null)
                    yield return new WaitForSeconds(0.1f);
                else
                    loaded = true;
            }

            yield return new WaitForSeconds(0.1f);
        }

        public void Update()
        {
            if (playerHeadAndObstacleInteraction == null) return;
            if(playerHeadAndObstacleInteraction.intersectingObstacles.Count > 0)
            {
                Console.WriteLine("intersect" + currentValue);
                currentValue += Time.deltaTime;
                base.checkedValue = (int)(currentValue*1000);
                CheckAndUpdateStatus();
            }
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("timeInWall");
        }
    }
}
