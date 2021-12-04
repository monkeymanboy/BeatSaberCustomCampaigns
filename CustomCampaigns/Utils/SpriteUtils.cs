using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomCampaigns.Utils
{
    public static class SpriteUtils
    {
        public static Sprite LoadSprite(string resourcePath, float pixelsPerUnit = 100f)
        {
            byte[] data = GetResource(Assembly.GetCallingAssembly(), resourcePath);
            return LoadSpriteRaw(data, pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromExternalAssembly(Assembly assembly, string resourcePath, float pixelsPerUnit = 100f)
        {
            byte[] data = GetResource(assembly, resourcePath);
            return LoadSpriteRaw(data, pixelsPerUnit);
        }

        public static Sprite LoadSpriteRaw(byte[] data, float pixelsPerUnit = 100f)
        {
            return LoadSpriteFromTexture(LoadTextureRaw(data), pixelsPerUnit);
        }

        public static Sprite LoadSpriteFromTexture(Texture2D texture, float pixelsPerUnit)
        {
            if (texture != null)
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), pixelsPerUnit);
            }

            return null;
        }

        public static Texture2D LoadTextureRaw(byte[] data)
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

        public static byte[] GetResource(Assembly assembly, string resourceName)
        {
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            byte[] data = new byte[(int)stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }
    }
}
