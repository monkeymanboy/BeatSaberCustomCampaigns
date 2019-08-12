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
            return APITools.CreateModifierParam(Assets.UnlockableIcons[(int)type], "Unlockable " + type.ToString().ToCharArray()[0] + type.ToString().ToLower().Substring(1), "Unlock a new " + type.ToString().ToLower() + " on completion");
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
