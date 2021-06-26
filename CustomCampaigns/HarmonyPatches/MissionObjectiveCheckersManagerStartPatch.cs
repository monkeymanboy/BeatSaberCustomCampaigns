//using CustomCampaigns.CustomMissionObjectives;
//using HarmonyLib;
//using IPA.Utilities;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace CustomCampaigns.HarmonyPatches
//{
//    [HarmonyPatch(typeof(MissionObjectiveCheckersManager), "Start")]
//    public class MissionObjectiveCheckersManagerStartPatch
//    {
//        // TODO: Replace with transpiler?
//        // We have to change the base method to use objectiveType so that it recognizes custom campaigns checkers
//        public static bool Prefix(MissionObjectiveCheckersManager __instance, MissionObjectiveChecker[] ____missionObjectiveCheckers, ILevelEndActions ____gameplayManager,
//                                MissionObjectiveCheckersManager.InitData ____initData, Action ___objectivesListDidChangeEvent)
//        {
//            ____gameplayManager.levelFailedEvent += __instance.HandleLevelFailed;
//            ____gameplayManager.levelFinishedEvent += __instance.HandleLevelFinished;
//            MissionObjective[] missionObjectives = ____initData.missionObjectives;

//            List<MissionObjectiveChecker> activeObjectiveCheckers = new List<MissionObjectiveChecker>(____missionObjectiveCheckers.Length);
//            List<MissionObjectiveChecker> allObjectiveCheckers = new List<MissionObjectiveChecker>(____missionObjectiveCheckers);
//            //Plugin.logger.Debug($"Before: {allObjectiveCheckers.Count}");
//            //AddCustomCheckers(allObjectiveCheckers);
//            //Plugin.logger.Debug($"After: {allObjectiveCheckers.Count}");

//            foreach (MissionObjective missionObjective in missionObjectives)
//            {
//                foreach (MissionObjectiveChecker missionObjectiveChecker in allObjectiveCheckers)
//                {
//                    //var customObjectiveChecker = missionObjectiveChecker as ICustomMissionObjectiveChecker;
//                    //if (customObjectiveChecker != null)
//                    //{
//                    //    if (customObjectiveChecker.GetMissionObjectiveType() == missionObjective.type.objectiveName)
//                    //    {
//                    //        Plugin.logger.Debug($"Found custom: { missionObjective.type.objectiveName}");
//                    //        SetFoundObjectiveChecker(missionObjectiveChecker, missionObjective, activeObjectiveCheckers, allObjectiveCheckers, ref foundMatchingObjectiveChecker);
//                    //        break;
//                    //    }
//                    //}

//                    if (missionObjectiveChecker.missionObjectiveType.objectiveName == missionObjective.type.objectiveName)
//                    {
//                        Plugin.logger.Debug($"Found base: { missionObjective.type}");
//                        SetFoundObjectiveChecker(missionObjectiveChecker, missionObjective, activeObjectiveCheckers, allObjectiveCheckers);
//                        break;
//                    }
//                }
//            }

//            //foreach (MissionObjectiveChecker missionObjectiveChecker in allObjectiveCheckers)
//            //{
//            //    GameObject.Destroy(missionObjectiveChecker.gameObject);
//            //}

//            __instance.SetField("_activeMissionObjectiveCheckers", activeObjectiveCheckers.ToArray());
//            foreach (MissionObjectiveChecker misisonObjectiveChecker in __instance.activeMissionObjectiveCheckers)
//            {
//                misisonObjectiveChecker.statusDidChangeEvent += __instance.HandleMissionObjectiveCheckerStatusDidChange;
//            }

//            ___objectivesListDidChangeEvent?.Invoke();
//            return false;
//        }

//        private static void SetFoundObjectiveChecker(MissionObjectiveChecker missionObjectiveChecker, MissionObjective missionObjective,
//                                                     List<MissionObjectiveChecker> activeObjectiveCheckers, List<MissionObjectiveChecker> allObjectiveCheckers)
//        {
//            activeObjectiveCheckers.Add(missionObjectiveChecker);
//            missionObjectiveChecker.SetCheckedMissionObjective(missionObjective);
//            allObjectiveCheckers.Remove(missionObjectiveChecker);
//        }
//    }
//}
