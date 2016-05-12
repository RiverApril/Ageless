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

		public static TextureArray arrayProps = new TextureArray(256, "props");
		public static TextureArray arrayActors = new TextureArray(16, "actors");
        public static TextureAtlas arrayTerrain = new TextureAtlas(256, 16, "terrain");
        public static TextureAtlas arrayGUI = new TextureAtlas(256, 16, "gui");

        public static List<string> textureArrayNames = new List<string>();

        public static void loadTextures() {

            arrayProps.add("stone");
            arrayActors.add("default");

            arrayTerrain.add("grass");
            arrayTerrain.add("dirt");
            arrayTerrain.add("water");
            arrayTerrain.add("cobble");
            arrayGUI.add("hud");

            arrayProps.make();
            arrayActors.make();

            arrayTerrain.make();
            arrayGUI.make();

        }

    }
}