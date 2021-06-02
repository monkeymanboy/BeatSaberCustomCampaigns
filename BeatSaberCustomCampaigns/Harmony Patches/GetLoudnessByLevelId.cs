using DataModels.Levels;
using HarmonyLib;
using SongCore;
using System;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    // A bit of a hack since levelID will be null when loading a custom level
    // Probably a better way, but this works w/ no side effects so ¯\_(ツ)_/¯
    [HarmonyPatch(typeof(PerceivedLoudnessPerLevelModel), "GetLoudnessByLevelId",
        new Type[] { typeof(string) })]
    class GetLoudnessByLevelId
    {
        static bool Prefix(string levelId, ref float __result)
        {
            if (levelId == null)
                __result = -6f;
                return false;
            return true;
        }
    }
}
