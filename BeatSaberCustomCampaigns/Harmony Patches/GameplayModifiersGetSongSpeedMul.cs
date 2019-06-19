using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerDataModelSaveData.GameplayModifiers;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(GameplayModifiers), "songSpeedMul", MethodType.Getter)]
    class GameplayModifiersGetSongSpeedMul
    {
        static bool Prefix(ref float __result, GameplayModifiers __instance, SongSpeed ____songSpeed)
        {
            if (__instance is CustomGameplayModifiers)
            {
                switch (____songSpeed)
                {
                    case SongSpeed.Slower:
                        __result = 0.85f;
                        break;
                    case SongSpeed.Faster:
                        __result = 1.2f;
                        break;
                    default:
                        __result = ((CustomGameplayModifiers)__instance).challengeModifiers.speedMul;
                        break;
                }
                return false;
            }
            return true;
        }
    }
}
