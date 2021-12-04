using CustomCampaigns.Managers;
using CustomCampaigns.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomCampaigns.Campaign.Missions
{
    public class UnlockableItem
    {
        public UnlockableType type;
        public string fileName;
        public string name;

        private const string UNLOCKABLES_PATH = "unlockables";
        private const string SABER_PATH = "CustomSabers";
        private const string AVATAR_PATH = "CustomAvatars";
        private const string PLATFORM_PATH = "CustomPlatforms";
        private const string NOTE_PATH = "CustomNotes";

        public GameplayModifierParamsSO GetModifierParam()
        {
            return ModifierUtils.CreateModifierParam(AssetsManager.GetUnlockableSprite(type),
                                                     $"Unlockable {type.ToString().First()}{type.ToString().ToLower().Substring(1)}",
                                                     $"Unlock a new {type.ToString().ToLower()} on completion");
        }

        public void UnlockItem(string campaignPath)
        {
            string sourcePath = Path.Combine(campaignPath, UNLOCKABLES_PATH, fileName);
            string destPath = Path.Combine(Environment.CurrentDirectory, GetFolder(), fileName);

            if (!File.Exists(destPath))
            {
                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath);
                }
                else
                {
                    Plugin.logger.Error($"No file exists for {fileName}");
                }
            }
        }

        private string GetFolder()
        {
            switch (type)
            {
                case UnlockableType.SABER:
                    return SABER_PATH;
                case UnlockableType.AVATAR:
                    return AVATAR_PATH;
                case UnlockableType.PLATFORM:
                    return PLATFORM_PATH;
                case UnlockableType.NOTE:
                    return NOTE_PATH;
                default:
                    return null;
            }
        }

        // Order matters for json deserialization
        public enum UnlockableType
        {
            SABER,
            AVATAR,
            PLATFORM,
            NOTE
        }
    }
}
