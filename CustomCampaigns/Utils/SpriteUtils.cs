using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomCampaigns.Utils
{
    public static class SpriteUtils
    {
        private const int MAX_IMAGE_SIZE = 256;

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
            return ReadStream(stream);
        }

        public static async Task<Sprite> LoadSpriteFromFile(string fileLocation, bool allowDownscaling = true, float pixelsPerUnit = 100f)
        {
            Stream stream;

            if (allowDownscaling)
            {
                stream = await Task.Run(() => DownscaleImage(fileLocation));
            }
            else
            {
                stream = new FileStream(fileLocation, FileMode.Open);
            }

            byte[] data;
            if (stream != null)
            {
                if (stream is MemoryStream memoryStream)
                {
                    data = memoryStream.ToArray();
                }
                else
                {
                    data = await ReadStreamAsync(stream);
                }
                
            }
            else
            {
                return null;
            }

            return LoadSpriteRaw(data, pixelsPerUnit);
        }

        public static byte[] ReadStream(Stream stream)
        {
            byte[] data = new byte[(int) stream.Length];
            stream.Read(data, 0, (int) stream.Length);
            return data;
        }

        public static async Task<byte[]> ReadStreamAsync(Stream stream)
        {
            byte[] data = new byte[(int) stream.Length];
            await stream.ReadAsync(data, 0, (int) stream.Length);
            return data;
        }

        public static Stream DownscaleImage(string fileLocation)
        { 
            Image originalImage = Image.FromFile(fileLocation);

            if (originalImage.Width <= MAX_IMAGE_SIZE && originalImage.Height <= MAX_IMAGE_SIZE)
            {
                var memoryStream = new MemoryStream();
                originalImage.Save(fileLocation, ImageFormat.Png);
                return memoryStream;
            }

            return DownscaleImageInternal(originalImage);
        }

        public static Stream DownscaleImage(Stream originalStream)
        {
            Image originalImage = Image.FromStream(originalStream);

            MemoryStream ms = new MemoryStream();
            originalImage.Save(ms, ImageFormat.Png);
            return ms;

            if (originalImage.Width <= MAX_IMAGE_SIZE && originalImage.Height <= MAX_IMAGE_SIZE)
            {
                return originalStream;
            }

            return DownscaleImageInternal(originalImage);
        }

        private static Stream DownscaleImageInternal(Image image)
        {
            Bitmap downscaledImage = new Bitmap(MAX_IMAGE_SIZE, MAX_IMAGE_SIZE);
            DrawResizedImage(image, ref downscaledImage);

            MemoryStream memoryStream = new MemoryStream();
            downscaledImage.Save(memoryStream, ImageFormat.Png);
            return memoryStream;
        }

        private static void DrawResizedImage(Image originalImage, ref Bitmap newImage)
        {
            newImage.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);
            var graphics = System.Drawing.Graphics.FromImage(newImage);

            var wrapMode = new ImageAttributes();
            wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);

            Rectangle resizedRectangle = new Rectangle(0, 0, MAX_IMAGE_SIZE, MAX_IMAGE_SIZE);
            graphics.DrawImage(originalImage, resizedRectangle, 0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, wrapMode);
        }
    }
}
