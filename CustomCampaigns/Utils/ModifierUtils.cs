using IPA.Utilities;
using System;
using UnityEngine;

namespace CustomCampaigns.Utils
{
    public static class ModifierUtils
    {
        public static GameplayModifierParamsSO CreateModifierParam(Sprite icon, string title, string description)
        {
            GameplayModifierParamsSO gameplayModifierParamsSO = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();

            gameplayModifierParamsSO.SetField("_icon", icon);
            gameplayModifierParamsSO.SetField("_modifierNameLocalizationKey", title);
            gameplayModifierParamsSO.SetField("_descriptionLocalizationKey", description);

            return gameplayModifierParamsSO;
        }
    }
}
