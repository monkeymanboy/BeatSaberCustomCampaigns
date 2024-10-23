using CustomCampaigns.Campaign.Missions;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionResultsViewController), "SetDataToUI")]
    public class MissionResultsViewControllerSetDataToUIPatch
    {
        // transpiler to remove original setting of song name
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int i = 0;
            int removalStartIndex = -1;
            int removalEndIndex = -1;

            var newInstructions = new List<CodeInstruction>(instructions);
            foreach (var instruction in instructions)
            {
                if (removalStartIndex != -1 && removalEndIndex == -1)
                {
                    if (instruction.opcode == OpCodes.Callvirt && instruction.operand.ToString() == "Void set_text(System.String)")
                    {
                        removalEndIndex = i + 2;
                        break;
                    }
                }
                else if (instruction.opcode == OpCodes.Ldfld && instruction.operand.ToString() == "TMPro.TextMeshProUGUI _songNameText")
                {
                    removalStartIndex = i;
                }
                i++;
            }

            newInstructions.RemoveRange(removalStartIndex, removalEndIndex - removalStartIndex);
            return newInstructions;
        }

        public static void Postfix(TextMeshProUGUI ____songNameText, MissionNode ____missionNode)
        {
            Plugin.logger.Debug("setdatatoui");
            var missionData = ____missionNode.missionData as CustomMissionDataSO;
            if (missionData != null)
            {
                ____songNameText.text = missionData.beatmapLevel.songName;
            }
        }
    }
}
