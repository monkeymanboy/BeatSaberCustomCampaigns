using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace BeatSaberCustomCampaigns
{
    public class APITools
    {
        public static string LeaderboardServer = "https://bsaber.com";
        public static string Username { get { return GetUserInfo.GetUserName(); } }
        public static string UserID { get { return GetUserInfo.GetUserID() + ""; } }
        public static BeatmapLevelSO stubLevel
        {
            get
            {
                if (_stubLevel == null)
                {
                    _stubLevel = ScriptableObject.CreateInstance<BeatmapLevelSO>();
                    _stubLevel.SetPrivateField("_songName", "Stub song");
                    _stubLevel.SetPrivateField("_songSubName", "Don't worry about it");
                    /*
                    BeatmapCharacteristicSO[] characteristics = new BeatmapCharacteristicSO[1];
                    characteristics[0] = ScriptableObject.CreateInstance<BeatmapCharacteristicSO>();
                    characteristics[0].SetPrivateField("_characteristicName", characteristic);
                    errorSO.SetBeatmapCharacteristics(characteristics);
                    errorSO.SetPrivateField("_audioClip", SongLoader.TemporaryAudioClip);
                    errorSO.SetPrivateField("_previewStartTime", 0);
                    errorSO.SetPrivateField("_previewDuration", 0);*/

                }
                return _stubLevel;
            }
        }
        private static BeatmapLevelSO _stubLevel;

        public static string GetHash(string usedString)
        {
            var md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(usedString + "%");
            byte[] hash = md5.ComputeHash(bytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string EncodePath(string path)
        {
            path = Uri.EscapeDataString(path);
            path = path.Replace("%2F", "/");
            path = path.Replace("%3A", ":");
            return path;
        }

        public static GameplayModifierParamsSO CreateModifierParam(Sprite icon, string title, string desc)
        {
            GameplayModifierParamsSO param = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
            param.SetPrivateField("_modifierNameLocalizationKey", title);
            param.SetPrivateField("_descriptionLocalizationKey", desc);
            param.SetPrivateField("_icon", icon);
            return param;
        }
    }
}
