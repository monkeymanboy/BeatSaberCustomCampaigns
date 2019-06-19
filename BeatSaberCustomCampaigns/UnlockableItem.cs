using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;

namespace BeatSaberCustomCampaigns
{
    public class UnlockableItem
    {
        public UnlockableType type;
        public string fileName;
        public string name;

        public GameplayModifierParamsSO GetModifierParam()
        {
            GameplayModifierParamsSO modifierParam = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
            modifierParam.SetPrivateField("_modifierName", "Unlockable " + type.ToString().ToCharArray()[0] + type.ToString().ToLower().Substring(1));
            modifierParam.SetPrivateField("_hintText", "Unlock a new " + type.ToString().ToLower() + " on completion");
            modifierParam.SetPrivateField("_icon", Assets.UnlockableIcons[(int)type]);
            return modifierParam;
        }
        public void UnlockItem(string activeCampaignPath)
        {
            string fromPath = activeCampaignPath + "/unlockables/" + fileName;
            string toPath = Environment.CurrentDirectory.Replace('\\', '/') + "/" + GetFolder() + "/" + fileName;
            if(!File.Exists(toPath))File.Copy(fromPath, toPath);
        }
        public string GetFolder()
        {
            switch (type)
            {
                case UnlockableType.SABER:
                    return "CustomSabers";
                case UnlockableType.AVATAR:
                    return "CustomAvatars";
                case UnlockableType.PLATFORM:
                    return "CustomPlatforms";
                case UnlockableType.NOTE:
                    return "CustomNotes";
            }
            return null;
        }
    }
    public enum UnlockableType
    {
        SABER,AVATAR,PLATFORM,NOTE
    }
}
