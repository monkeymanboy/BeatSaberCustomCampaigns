using CustomCampaigns.Campaign;
using HarmonyLib;
using static GameplayModifiers;

namespace CustomCampaigns.HarmonyPatches
{

    [HarmonyPatch(typeof(GameplayModifiers), "songSpeedMul", MethodType.Getter)]
    public class GameplayModifiersSongSpeedMulGetterPatch
    {
        public static bool Prefix(ref float __result, GameplayModifiers __instance, SongSpeed ____songSpeed)
        {
            CustomGameplayModifiers customGameplayModifiers = __instance as CustomGameplayModifiers;
            if (customGameplayModifiers != null)
            {
                switch (____songSpeed)
                {
                    case SongSpeed.Slower:
                        __result = 0.85f;
                        break;
                    case SongSpeed.Faster:
                        __result = 1.2f;
                        break;
                    case SongSpeed.SuperFast:
                        __result = 1.5f;
                        break;
                    default:
                        __result = customGameplayModifiers.missionModifiers.speedMul;
                        break;
                }

                return false;
            }
            return true;
        }
    }
}
