using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            ButtonIcon = LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.ButtonIcon.png");
            ErrorIcon = LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.ErrorIcon.png");
            FailOnClashIcon = LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.FailOnClashIcon.png");
            UnlockableSongIcon = LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.UnlockableSongIcon.png");
            UnlockableIcons = new Sprite[Enum.GetNames(typeof(UnlockableType)).Length];
            for(int i = 0; i < UnlockableIcons.Length; i++)
                UnlockableIcons[i] = LoadSpriteFromResources("BeatSaberCustomCampaigns.Resources.UnlockableIcon_" + i + ".png");
        }

        public static Texture2D LoadTextureRaw(byte[] file)
        {
            if (file.Count() > 0)
            {
                Texture2D Tex2D = new Texture2D(2, 2);
                if (Tex2D.LoadImage(file))
                    return Tex2D;
            }
            return null;
        }
        public static Texture2D LoadTextureFromResources(string resourcePath)
        {
            return LoadTextureRaw(GetResource(Assembly.GetCallingAssembly(), resourcePath));
        }

        public static Sprite LoadSpriteRaw(byte[] image, float PixelsPerUnit = 100.0f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(image), PixelsPerUnit);
        }

        public static Sprite LoadSpriteFromTexture(Texture2D SpriteTexture, float PixelsPerUnit = 100.0f)
        {
            if (SpriteTexture)
                return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);
            return null;
        }

        public static Sprite LoadSpriteFromResources(string resourcePath, float PixelsPerUnit = 100.0f)
        {
            return LoadSpriteRaw(GetResource(Assembly.GetCallingAssembly(), resourcePath), PixelsPerUnit);
        }

        public static byte[] GetResource(Assembly asm, string ResourceName)
        {
            System.IO.Stream stream = asm.GetManifestResourceStream(ResourceName);
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }
    }
}
