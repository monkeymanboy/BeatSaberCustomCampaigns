using BS_Utils.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Custom_Trackers
{
    public class SpinsMissionObjectiveChecker : SimpleValueMissionObjectiveChecker, CustomTracker
    {
        protected Transform headTransform;
        protected PauseMenuManager pauseMenuManager;

        protected int consecutiveTurnCounter = 0;
        protected bool direction = false;
        protected int lastSegment = 3;

        const int CONSECUTIVE_SEGMENTS = 5;
        const int SEGMENT_COUNT = 6;
        const int SEGMENT_SIZE = 360 / SEGMENT_COUNT;

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
            PlayerTransforms playerTransforms = null;
            bool loaded = false;
            while (!loaded)
            {
                playerTransforms = Resources.FindObjectsOfTypeAll<PlayerTransforms>().FirstOrDefault();
                pauseMenuManager = Resources.FindObjectsOfTypeAll<PauseMenuManager>().FirstOrDefault();
                if (playerTransforms == null || pauseMenuManager == null)
                    yield return new WaitForSeconds(0.1f);
                else
                    loaded = true;
            }
            headTransform = playerTransforms.GetPrivateField<Transform>("_headTransform");
            yield return new WaitForSeconds(0.1f);
        }

        public void Update()
        {
            if (headTransform == null) return;
            if (pauseMenuManager != null && pauseMenuManager.enabled)
            {
                consecutiveTurnCounter = 0;
                return;
            }
            float rotation = headTransform.localRotation.eulerAngles.y;
            rotation -= SEGMENT_SIZE / 2;
            if (rotation < 0) rotation += 360;
            if ((int)(rotation / SEGMENT_SIZE) == GetSegmentLoop(lastSegment + 1))
            {
                if (!direction)
                {
                    consecutiveTurnCounter++;
                }
                else
                {
                    consecutiveTurnCounter = 0;
                }
                direction = false;
            }
            if ((int)(rotation / SEGMENT_SIZE) == GetSegmentLoop(lastSegment - 1))
            {
                if (direction)
                {
                    consecutiveTurnCounter++;
                }
                else
                {
                    consecutiveTurnCounter = 0;
                }
                direction = true;
            }
            lastSegment = (int)(rotation / SEGMENT_SIZE);
            if (consecutiveTurnCounter == CONSECUTIVE_SEGMENTS)
            {
                consecutiveTurnCounter -= CONSECUTIVE_SEGMENTS;
                base.checkedValue++;
                CheckAndUpdateStatus();
            }
        }

        private int GetSegmentLoop(int segment)
        {
            if (segment < 0) return segment + SEGMENT_COUNT;
            if (segment >= SEGMENT_COUNT) return segment - SEGMENT_COUNT;
            return segment;
        }

        public string GetMissionObjectiveTypeName()
        {
            return ChallengeRequirement.GetObjectiveName("spins");
        }
    }
}
