using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Ageless {
	public class TextureArray {

        public Vector2[] coords;

        int size;

        public int id;
        public List<string> names = new List<string>();
        public string folder;

        public TextureArray(int size, string folder) {
			this.size = size;
            this.folder = folder;
        }

        public void add(string name) {
            names.Add(name);
        }

        internal void make() {
            id = GL.GenTexture();
            TryGL.Call(() => GL.BindTexture(TextureTarget.Texture2DArray, id));
            TryGL.Call(() => GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, size, size, names.Count));

            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder));

            for (int index = 0; index < names.Count; index++) {
                loadTexture(Game.dirTextures + folder + "/" + names[index] + ".png", index);
            }
            
            coords = new Vector2[4];

            coords[0] = new Vector2(0, 0);
            coords[1] = new Vector2(1, 0);
            coords[2] = new Vector2(0, 1);
            coords[3] = new Vector2(1, 1);
        }

        public void loadTexture(string path, int index) {
            if (File.Exists(path)) {

                using (Bitmap bmp = new Bitmap(path)) {

                    BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    
                    TryGL.Call(() => GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, index, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0));

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