using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using CustomCampaigns.Campaign.Missions;

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

        private const string RESOURCE_PREFIX = "CustomCampaigns.Resources";

        public void Initialize()
        {
            ErrorIcon = LoadSprite(RESOURCE_PREFIX + ".ErrorIcon.png");
            FailOnSaberClashIcon = LoadSprite(RESOURCE_PREFIX + ".FailOnSaberClashIcon.png");

            UnlockableSaberIcon = LoadSprite(RESOURCE_PREFIX + ".UnlockableSaberIcon.png");
            UnlockableAvatarIcon = LoadSprite(RESOURCE_PREFIX + ".UnlockableAvatarIcon.png");
            UnlockablePlatformIcon = LoadSprite(RESOURCE_PREFIX + ".UnlockablePlatformIcon.png");
            UnlockableNoteIcon = LoadSprite(RESOURCE_PREFIX + ".UnlockableNoteIcon.png");

            UnlockableSongIcon = LoadSprite(RESOURCE_PREFIX + ".UnlockableSongIcon.png");
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

        private Sprite LoadSprite(string resourcePath, float pixelsPerUnit = 100f)
        {
            byte[] data = GetResource(Assembly.GetCallingAssembly(), resourcePath);
            return LoadSpriteRaw(data, pixelsPerUnit);
        }

        private Sprite LoadSpriteRaw(byte[] data, float pixelsPerUnit = 100f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(data), pixelsPerUnit);
        }

        private Sprite LoadSpriteFromTexture(Texture2D texture, float pixelsPerUnit)
        {
            if (texture != null)
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), pixelsPerUnit);
            }

            return null;
        }

        private Texture2D LoadTextureRaw(byte[] data)
        {
            if (data.Count() > 0)
            {
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(data))
                {
                    return texture;
                }
            }

            return null;
        }

        private byte[] GetResource(Assembly assembly, string resourceName)
        {
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            byte[] data = new byte[(int) stream.Length];
            stream.Read(data, 0, (int) stream.Length);
            return data;
        }
    }
}
