using BeatSaberCustomCampaigns.Custom_Trackers;
using BS_Utils.Utilities;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{

    [HarmonyPatch(typeof(MissionObjectiveCheckersManager), "Init",
        new Type[] { typeof(MissionObjective[])})]
    class MissionObjectiveCheckersManagerInit
    {
        static bool Prefix(MissionObjective[] missionObjectives, MissionObjectiveCheckersManager __instance, MissionObjectiveChecker[] ____missionObjectiveCheckers)
        {
            __instance.SetPrivateField("_isInitialized",true);
            List<MissionObjectiveChecker> list = new List<MissionObjectiveChecker>(____missionObjectiveCheckers.Length);
            List<MissionObjectiveChecker> list2 = new List<MissionObjectiveChecker>(____missionObjectiveCheckers);
            List<MissionObjectiveChecker> customCheckers = new List<MissionObjectiveChecker>();
            customCheckers.Add(new GameObject().AddComponent<PerfectCutMissionObjectiveChecker>());
            customCheckers.Add(new GameObject().AddComponent<BombsHitMissionObjectiveChecker>());
            customCheckers.Add(new GameObject().AddComponent<HeadTimeInWallMissionObjectiveChecker>());
            customCheckers.Add(new GameObject().AddComponent<SaberTimeInWallMissionObjectiveChecker>());
            customCheckers.Add(new GameObject().AddComponent<WallHeadbuttMissionObjectiveChecker>());
            customCheckers.Add(new GameObject().AddComponent<SpinsMissionObjectiveChecker>());
            foreach (MissionObjective missionObjective in missionObjectives)
            {
                bool flag = false;
                foreach (MissionObjectiveChecker missionObjectiveChecker in list2)
                {
                    if (missionObjectiveChecker.missionObjectiveType.objectiveName == missionObjective.type.objectiveName)
                    {
                        list.Add(missionObjectiveChecker);
                        missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
                        list2.Remove(missionObjectiveChecker);
                        flag = true;
                        break;
                    }
                }
                foreach (MissionObjectiveChecker missionObjectiveChecker in customCheckers)
                {
                    if((missionObjectiveChecker as CustomTracker).GetMissionObjectiveTypeName() == missionObjective.type.objectiveName)
                    {
                        list.Add(missionObjectiveChecker);
                        missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
                        customCheckers.Remove(missionObjectiveChecker);
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    UnityEngine.Debug.LogError("No missionObjectiveCheckers for missionOjective");
                }
            }
            foreach (MissionObjectiveChecker missionObjectiveChecker in list2)
            {
                GameObject.Destroy(missionObjectiveChecker.gameObject);
            }
            foreach (MissionObjectiveChecker missionObjectiveChecker in customCheckers)
            {
                GameObject.Destroy(missionObjectiveChecker.gameObject);
            }
            __instance.SetPrivateField("_activeMissionObjectiveCheckers", list.ToArray());
            foreach (MissionObjectiveChecker missionObjectiveChecker2 in __instance.activeMissionObjectiveCheckers)
            {
                missionObjectiveChecker2.statusDidChangeEvent += __instance.HandleMissionObjectiveCheckerStatusDidChange;
            }
            if (__instance.GetPrivateField<Action>("objectivesListDidChangeEvent") != null)
            {
                __instance.GetPrivateField<Action>("objectivesListDidChangeEvent").Invoke();
            }
            return false;
        }
    }
}
