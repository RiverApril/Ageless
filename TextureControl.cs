using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Drawing.Imaging;

namespace Ageless {
    class TextureControl {

        public static int terrain;

        public static Vector2[,] tex16x16Coords = new Vector2[16*16, 4];



        public static void loadTextures() {
            terrain = loadTexture("../../terrain.bmp");

            int k = 0;

            for (int y = 0; y < 16; y++) {
                for (int x = 0; x < 16; x++) {
                    float x1 = x * (1.0f / 16.0f);
                    float y1 = y * (1.0f / 16.0f);
                    float x2 = (x + 1) * (1.0f / 16.0f);
                    float y2 = (y + 1) * (1.0f / 16.0f);
                    tex16x16Coords[k, 0] = new Vector2(x1, y1);
                    tex16x16Coords[k, 1] = new Vector2(x2, y1);
                    tex16x16Coords[k, 2] = new Vector2(x1, y2);
                    tex16x16Coords[k, 3] = new Vector2(x2, y2);
                    k++;
                }
            }

        }

        public static int loadTexture(string path) {
            int id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            Bitmap bmp = new Bitmap(path);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

            bmp.UnlockBits(bmpData);

            return id;
        }

    }
}
