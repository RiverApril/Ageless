using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace Ageless {
    class TextureControl {

        public const int texSize = 256;

        public static int textureID;

        public static Vector2[,] tex16x16Coords = new Vector2[16*16, 4];

        public static List<string> textures = new List<string>();

        public static int texAtlas;
        public static int texStone;

        public static void loadTextures() {

            textures.Add("atlas"); texAtlas = textures.Count - 1;
            textures.Add("stone"); texStone = textures.Count - 1;

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, textureID);
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, texSize, texSize, textures.Count);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            for (int index = 0; index < textures.Count; index++) {
                loadTexture(Game.dirTextures + textures[index] + ".png", index);
            }


            int k = 0;

			float x1;
			float y1;
			float x2;
			float y2;

            //float pixelSize = 1.0f / 256.0f;

            for (int y = 0; y < 16; y++) {
                for (int x = 0; x < 16; x++) {
                    x1 = (x * (1.0f / 16.0f));// + (pixelSize * 0.5f);
                    y1 = (y * (1.0f / 16.0f));// + (pixelSize * 0.5f);
                    x2 = ((x + 1) * (1.0f / 16.0f));// + (pixelSize * 0.5f);
                    y2 = ((y + 1) * (1.0f / 16.0f));// + (pixelSize * 0.5f);
                    tex16x16Coords[k, 0] = new Vector2(x1, y1);
                    tex16x16Coords[k, 1] = new Vector2(x2, y1);
                    tex16x16Coords[k, 2] = new Vector2(x1, y2);
                    tex16x16Coords[k, 3] = new Vector2(x2, y2);
					//Console.WriteLine("Tex Coord: {0}, {1}, {2}, {3}", tex16x16Coords[k, 0]*16, tex16x16Coords[k, 1]*16, tex16x16Coords[k, 2]*16, tex16x16Coords[k, 3]*16);
					k++;
                }
            }

        }

        public static void loadTexture(string path, int index) {
            if (File.Exists(path)) {

                using (Bitmap bmp = new Bitmap(path)) {

                    bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    //GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
                    //glTexSubImage3D(GL_TEXTURE_2D_ARRAY, 0, 0, 0, 0, width, height, layerCount, GL_RGBA, GL_UNSIGNED_BYTE, texels);
                    GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, index, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);

                    bmp.UnlockBits(bmpData);

                    bmp.Dispose();
                    Console.WriteLine("Texture loaded: {0}", path);
                }
            } else {
                Console.WriteLine("{0} does not exist.", path);
            }

        }

    }
}
