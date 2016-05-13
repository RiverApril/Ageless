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

		public static TextureArray arrayProps = new TextureArray(256, "props", 4, false);
		public static TextureArray arrayActors = new TextureArray(16, "actors", 1, false);
        public static TextureArray arrayTerrain = new TextureArray(256, "terrain", 16, true);
        public static TextureArray arrayGUI = new TextureArray(256, "gui", 16, true);

        public static List<string> textureArrayNames = new List<string>();

        public static void loadTextures() {

            arrayProps.add("stone");
            arrayActors.add("default");

            arrayTerrain.add("grass");
            arrayTerrain.add("dirt");

            arrayGUI.add("hud");

            arrayProps.make();
            arrayActors.make();

            arrayTerrain.make();
            arrayGUI.make();

        }

    }
}