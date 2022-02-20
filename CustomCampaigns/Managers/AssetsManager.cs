﻿using CustomCampaigns.Campaign.Missions;
using CustomCampaigns.Utils;
using UnityEngine;
using Zenject;

namespace CustomCampaigns.Managers
{
    public class AssetsManager : IInitializable
    {
        public static Sprite ErrorIcon;
        public static Sprite FailOnSaberClashIcon;

        public static Sprite UnlockableSaberIcon;
        public static Sprite UnlockableAvatarIcon;
        public static Sprite UnlockablePlatformIcon;
        public static Sprite UnlockableNoteIcon;

        public static Sprite UnlockableSongIcon;

        public static Sprite ScoreSaberLogo;

        private const string RESOURCE_PREFIX = "CustomCampaigns.Resources";

        public void Initialize()
        {
            LoadImages();
        }

        public void LoadImages()
        {
            ErrorIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".ErrorIcon.png");
            FailOnSaberClashIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".FailOnSaberClashIcon.png");

            UnlockableSaberIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".UnlockableSaberIcon.png");
            UnlockableAvatarIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".UnlockableAvatarIcon.png");
            UnlockablePlatformIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".UnlockablePlatformIcon.png");
            UnlockableNoteIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".UnlockableNoteIcon.png");

            UnlockableSongIcon = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".UnlockableSongIcon.png");

            ScoreSaberLogo = SpriteUtils.LoadSprite(RESOURCE_PREFIX + ".ScoreSaberLogo.png");
        }

        public static Sprite GetUnlockableSprite(UnlockableItem.UnlockableType type)
        {
            switch (type)
            {
                case UnlockableItem.UnlockableType.SABER:
                    return UnlockableSaberIcon;
                case UnlockableItem.UnlockableType.AVATAR:
                    return UnlockableAvatarIcon;
                case UnlockableItem.UnlockableType.PLATFORM:
                    return UnlockablePlatformIcon;
                case UnlockableItem.UnlockableType.NOTE:
                    return UnlockableNoteIcon;
                default:
                    return ErrorIcon;
            }
        }
    }
}
