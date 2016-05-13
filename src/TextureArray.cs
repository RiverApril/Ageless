using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Ageless {
	public class TextureArray {

        public Vector3[,,,] coords;

        int size;

        public int id;
        public List<string> names = new List<string>();
        public string folder;

        public int scale;
        public bool subdivide;

        public TextureArray(int size, string folder, int scale, bool subdivide) {
			this.size = size;
            this.folder = folder;
            this.scale = scale;
            this.subdivide = subdivide;
        }

        public void add(string name) {
            names.Add(name);
        }

        internal void make() {
            id = GL.GenTexture();
            TryGL.Call(() => GL.BindTexture(TextureTarget.Texture2DArray, id));
            TryGL.Call(() => GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, size, size, names.Count));

            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat));
            TryGL.Call(() => GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat));


            if (subdivide) {
                coords = new Vector3[names.Count, scale, scale, 4];

                for (int index = 0; index < names.Count; index++) {
                    loadTexture(Game.dirTextures + folder + "/" + names[index] + ".png", index);
                    for (int i = 0; i < scale; i++) {
                        for (int j = 0; j < scale; j++) {
                            float x1 = i * (1.0f / scale);
                            float y1 = j * (1.0f / scale);
                            float x2 = (i + 1) * (1.0f / scale);
                            float y2 = (j + 1) * (1.0f / scale);
                            coords[index, i, j, 0] = new Vector3(x1, y1, index);
                            coords[index, i, j, 1] = new Vector3(x2, y1, index);
                            coords[index, i, j, 2] = new Vector3(x1, y2, index);
                            coords[index, i, j, 3] = new Vector3(x2, y2, index);
                        }
                    }
                }
            } else {
                coords = new Vector3[names.Count, 1, 1, 4];

                for (int index = 0; index < names.Count; index++) {
                    loadTexture(Game.dirTextures + folder + "/" + names[index] + ".png", index);
                    coords[index, 0, 0, 0] = new Vector3(0, 0, index);
                    coords[index, 0, 0, 1] = new Vector3(1, 0, index);
                    coords[index, 0, 0, 2] = new Vector3(0, 1, index);
                    coords[index, 0, 0, 3] = new Vector3(1, 1, index);
                }
            }
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