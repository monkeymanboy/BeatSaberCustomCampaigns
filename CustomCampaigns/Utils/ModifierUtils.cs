using CustomCampaigns.Managers;
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

        public static GameplayModifierParamsSO CreateUnlockableSongParam()
        {
            return CreateModifierParam(AssetsManager.UnlockableSongIcon, "Unlockable Song", "Unlock this song on completion");
        }
    }
}
