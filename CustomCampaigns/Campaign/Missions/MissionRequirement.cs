using CustomCampaigns.CustomMissionObjectives.Formatters;
using IPA.Utilities;
using System.Collections.Generic;
using UnityEngine;
using static MissionObjective;

namespace CustomCampaigns.Campaign.Missions
{
    public class MissionRequirement
    {
        public bool isMax;
        public string type;
        public int count;

        public MissionObjective GetAsMissionObjective()
        {
            MissionObjective objective = new MissionObjective();
            MissionObjectiveTypeSO missionObjectiveTypeSO = ScriptableObject.CreateInstance<MissionObjectiveTypeSO>();

            missionObjectiveTypeSO.SetField("_objectiveName", GetObjectiveName(type));
            missionObjectiveTypeSO.SetField("_noConditionValue", GetNoCondition(type));
            missionObjectiveTypeSO.SetField("_objectiveValueFormater", GetObjectiveValueFormater(type));

            objective.SetField("_type", missionObjectiveTypeSO);
            objective.SetField("_referenceValueComparisonType", isMax ? ReferenceValueComparisonType.Max : ReferenceValueComparisonType.Min);
            objective.SetField("_referenceValue", count);
            return objective;
        }

        public static string GetObjectiveName(string type)
        {
            return objectiveNameDict.ContainsKey(type) ? objectiveNameDict[type] : "ERROR";
        }

        public static ObjectiveValueFormatterSO GetObjectiveValueFormater(string type)
        {
            switch (type)
            {
                case "saberDistance":
                    return ScriptableObject.CreateInstance<DistanceObjectiveValueFormatterSO>();
                case "score":
                    return ScriptableObject.CreateInstance<ScoreObjectiveValueFormatterSO>();
                case "headInWall":
                case "saberInWall":
                    return ScriptableObject.CreateInstance<MillisecondObjectiveValueFormatterSO>();
                case "accuracy":
                case "maintainAccuracy":
                    return ScriptableObject.CreateInstance<PercentageObjectiveValueFormatterSO>();
                default:
                    return ScriptableObject.CreateInstance<ObjectiveValueFormatterSO>();
            }
        }

        public static bool GetNoCondition(string type)
        {
            return type == "fullCombo";
        }

        static readonly IReadOnlyDictionary<string, string> objectiveNameDict = new Dictionary<string, string>
        {
            // Base Game Objectives
            {"blocksMissed", "OBJECTIVE_MISS"},
            {"score", "OBJECTIVE_SCORE"},
            {"saberDistance", "OBJECTIVE_HANDS_MOVEMENT"},
            {"combo", "OBJECTIVE_FULL_COMBO"},
            {"fullCombo", "OBJECTIVE_FULL_COMBO"},
            {"badCuts", "OBJECTIVE_BAD_CUTS"},

            // Custom Objectives
            {"accuracy", "Accuracy"},
            {"maintainAccuracy", "Maintain Acc"},
            {"perfectCuts", "Perfect Cuts"},
            {"spins", "Spins"},
            {"bombsHit", "Bombs"},
            {"headInWall", "Head In Wall"},
            {"saberInWall", "Saber In Wall"},
            {"wallHeadbutts", "Wall Headbutts"}
        };
    }
}
