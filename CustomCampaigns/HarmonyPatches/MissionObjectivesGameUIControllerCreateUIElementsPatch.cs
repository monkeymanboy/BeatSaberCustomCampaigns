using HarmonyLib;
using IPA.Utilities;
using System.Collections.Generic;

namespace CustomCampaigns.HarmonyPatches
{
    [HarmonyPatch(typeof(MissionObjectivesGameUIController), "CreateUIElements")]
    class MissionObjectivesGameUIControllerCreateUIElementsPatch
    {
        public static bool Prefix(MissionObjectiveCheckersManager ____missionObjectiveCheckersManager, List<MissionObjectiveGameUIView> ____missionObjectiveGameUIViews)
        {
            // Base game does not unsubscribe MissionObjectiveGameUIViews from events when being Destroyed - so we'll do it ourselves
            if (____missionObjectiveGameUIViews != null)
            {
                foreach (var missionObjectiveGameUIView in ____missionObjectiveGameUIViews)
                {
                    missionObjectiveGameUIView.GetField<MissionObjectiveChecker, MissionObjectiveGameUIView>("_missionObjectiveChecker").statusDidChangeEvent -= missionObjectiveGameUIView.HandleMissionObjectiveStatusDidChange;
                    missionObjectiveGameUIView.GetField<MissionObjectiveChecker, MissionObjectiveGameUIView>("_missionObjectiveChecker").checkedValueDidChangeEvent -= missionObjectiveGameUIView.HandleMissionObjectiveCheckedValueDidChange;
                }
            }

            return !Plugin.config.disableObjectiveUI;
        }
    }
}
