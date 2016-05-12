using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Ageless {
    public class TextureAtlas {

        public Vector2[,] coords;

        private int size;
        private int splitSize;

        public int id;
        public List<string> names = new List<string>();
        private string folder;

        public TextureAtlas(int size, int splitSize, string folder) {
            this.size = size;
            this.splitSize = splitSize;
            this.folder = folder;
        }

        public void add(string name) {
            names.Add(name);
        }

        public void make() {
            id = GL.GenTexture();
            TryGL.Call(() => GL.BindTexture(TextureTarget.Texture2DArray, id));
            TryGL.Call(() => GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, size, size, 1));

            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder));

            for (int index = 0; index < names.Count; index++) {
                loadTexture(Game.dirTextures + folder + "/" + names[index] + ".png", index);
            }

            coords = new Vector2[splitSize * splitSize, 4];

            int k = 0;

            float x1;
            float y1;
            float x2;
            float y2;

            for (int y = 0; y < splitSize; y++) {
                for (int x = 0; x < splitSize; x++) {
                    x1 = (x * (1.0f / splitSize));
                    y1 = (y * (1.0f / splitSize));
                    x2 = ((x + 1) * (1.0f / splitSize));
                    y2 = ((y + 1) * (1.0f / splitSize));
                    coords[k, 0] = new Vector2(x1, y1);
                    coords[k, 1] = new Vector2(x2, y1);
                    coords[k, 2] = new Vector2(x1, y2);
                    coords[k, 3] = new Vector2(x2, y2);
                    k++;
                }
            }

        }

        public void loadTexture(string path, int index) {
            if (File.Exists(path)) {

                using (Bitmap bmp = new Bitmap(path)) {

                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    TryGL.Call(() => GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, (index % splitSize) * (size / splitSize), (index / splitSize) * (size / splitSize), 0, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0));

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