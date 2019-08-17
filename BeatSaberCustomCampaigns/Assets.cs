using CustomUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatSaberCustomCampaigns
{
    public class Assets
    {
        public static Sprite ButtonIcon;
        public static Sprite ErrorIcon;
        public static Sprite FailOnClashIcon;
        public static Sprite UnlockableSongIcon;
        public static Sprite[] UnlockableIcons;
        public static void Init()
        {
            ButtonIcon = UIUtilities.LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.ButtonIcon.png");
            ErrorIcon = UIUtilities.LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.ErrorIcon.png");
            FailOnClashIcon = UIUtilities.LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.FailOnClashIcon.png");
            UnlockableSongIcon = UIUtilities.LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.UnlockableSongIcon.png");
            UnlockableIcons = new Sprite[Enum.GetNames(typeof(UnlockableType)).Length];
            for(int i = 0; i < UnlockableIcons.Length; i++)
                UnlockableIcons[i] = UIUtilities.LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.UnlockableIcon_" + i + ".png");
        }
    }
}
