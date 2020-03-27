using BS_Utils.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns.Harmony_Patches
{
    [HarmonyPatch(typeof(GameplayModifiersModelSO), "GetModifierParams",
        new Type[] { typeof(GameplayModifiers) })]
    class GameplayModifiersModelSOGetModifierParams
    {
        static void Postfix(GameplayModifiers gameplayModifiers, GameplayModifierParamsSO ____fasterSong, GameplayModifierParamsSO ____slowerSong, ref List<GameplayModifierParamsSO> __result)
        {
            if(gameplayModifiers is CustomGameplayModifiers)
            {
                ChallengeModifiers challenge = ((CustomGameplayModifiers)gameplayModifiers).challengeModifiers;
                if(challenge.songSpeed==GameplayModifiers.SongSpeed.Normal && challenge.speedMul != 1)
                {
                    GameplayModifierParamsSO speedParamsSo = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
                    speedParamsSo.SetPrivateField("_modifierNameLocalizationKey", "Song Speed - " + (int)(challenge.speedMul * 100) + "%");
                    if (challenge.speedMul < 1)
                    {
                        speedParamsSo.SetPrivateField("_descriptionLocalizationKey", "Song will play slower.");
                        speedParamsSo.SetPrivateField("_icon", ____slowerSong.icon);
                    } else
                    {
                        speedParamsSo.SetPrivateField("_descriptionLocalizationKey", "Song will play faster.");
                        speedParamsSo.SetPrivateField("_icon", ____fasterSong.icon);
                    }
                    __result.Add(speedParamsSo);
                }
                if (gameplayModifiers.failOnSaberClash)
                    __result.Add(APITools.CreateModifierParam(Assets.FailOnClashIcon, "Fail On Saber Clash", "If your sabers touch, you fail."));
            }
        }
    }
}
