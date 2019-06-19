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
            missionObjectiveTypeSO.SetPrivateField("_objectiveValueFormater", ScriptableObject.CreateInstance<ObjectiveValueFormatterSO>());
            objective.SetPrivateField("_type", missionObjectiveTypeSO);
            objective.SetPrivateField("_referenceValueComparisonType", isMax ? ReferenceValueComparisonType.Max : ReferenceValueComparisonType.Min);
            objective.SetPrivateField("_referenceValue", count);
            return objective;
        }
        public static string GetObjectiveName(string type)
        {
            switch (type)
            {
                case "bombsHit":
                    return "Bombs";
                case "blocksMissed":
                    return "Miss";
                case "score":
                    return "OBJECTIVE_SCORE";
                case "saberDistance":
                    return "Hands Movement";
                case "perfectCuts":
                    return "Perfect Cuts";
                case "combo":
                    return "Combo";
                case "fullCombo":
                    return "Full Combo";
                case "badCuts":
                    return "OBJECTIVE_BAD_CUTS";
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
