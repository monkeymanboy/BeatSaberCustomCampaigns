using BeatSaberCustomCampaigns.Custom_Trackers;
using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MissionObjective;

namespace BeatSaberCustomCampaigns
{
    [Serializable]
    public class ChallengeRequirement
    {
        public bool isMax;
        public string type;
        public int count;

        public MissionObjective GetAsMissionObjective()
        {
            MissionObjective objective = new MissionObjective();
            MissionObjectiveTypeSO missionObjectiveTypeSO = ScriptableObject.CreateInstance<MissionObjectiveTypeSO>();
            missionObjectiveTypeSO.SetPrivateField("_objectiveName", GetObjectiveName(type));
            missionObjectiveTypeSO.SetPrivateField("_noConditionValue", GetNoCondition(type));
            missionObjectiveTypeSO.SetPrivateField("_objectiveValueFormater", GetObjectiveValueFormater(type));
            objective.SetPrivateField("_type", missionObjectiveTypeSO);
            objective.SetPrivateField("_referenceValueComparisonType", isMax ? ReferenceValueComparisonType.Max : ReferenceValueComparisonType.Min);
            objective.SetPrivateField("_referenceValue", count);
            return objective;
        }
        public static ObjectiveValueFormatterSO GetObjectiveValueFormater(string type)
        {
            switch (type)
            {
                case "saberDistance":
                    return ScriptableObject.CreateInstance<DistanceObjectiveValueFormatterSO>();
                case "score":
                    return ScriptableObject.CreateInstance<ScoreObjectiveValueFormatterSO>();
                case "headInWall": case "saberInWall":
                    return ScriptableObject.CreateInstance<MillisecondObjectiveValueFormatterSO>();
                case "accuracy": case "maintainAccuracy":
                    return ScriptableObject.CreateInstance<PercentageObjectiveValueFormatterSO>();
                default:
                    return ScriptableObject.CreateInstance<ObjectiveValueFormatterSO>();
            }
        }
        public static string GetObjectiveName(string type)
        {
            switch (type)
            {
                case "bombsHit":
                    return "Bombs";
                case "blocksMissed":
                    return "OBJECTIVE_MISS";
                case "score":
                    return "OBJECTIVE_SCORE";
                case "saberDistance":
                    return "OBJECTIVE_HANDS_MOVEMENT";
                case "perfectCuts":
                    return "Perfect Cuts";
                case "combo":
                    return "OBJECTIVE_COMBO";
                case "fullCombo":
                    return "OBJECTIVE_FULL_COMBO";
                case "badCuts":
                    return "OBJECTIVE_BAD_CUTS";
                case "headInWall":
                    return "Head In Wall";
                case "saberInWall":
                    return "Saber In Wall";
                case "wallHeadbutts":
                    return "Wall Headbutts";
                case "spins":
                    return "Spins";
                case "accuracy":
                    return "Accuracy";
                case "maintainAccuracy":
                    return "Maintain Acc";
            }
            return "ERROR";
        }
        public static bool GetNoCondition(string type)
        {
            switch (type)
            {
                case "fullCombo": return true;
                default:
                    return false;
            }
        }
    }
}
