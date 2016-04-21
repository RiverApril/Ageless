using OpenTK;
using System;
using System.Collections.Generic;

namespace Ageless {
    public class Tile {

        public static Dictionary<int, Tile> tileList = new Dictionary<int, Tile>();

        public static Tile tileAir = new Tile(0, "Void", RenderType.None, -1, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        public static Tile tileGrass = new Tile(1, "Grass", RenderType.Terrain, 0, new Vector4(1.0f, 1.0f, 1.0f, 1.0f));

        public static Tile fromIndex(int i) {
            return tileList[i];
        }

        public int index { get; private set; }
        public string name { get; private set; }
        public Vector4 color { get; private set; }
        public int UVIndex { get; private set; }
        public RenderType renderType { get; private set; }

        public Tile(int index, string name, RenderType renderType, int UVIndex, Vector4 color) {
            this.index = index;
            this.name = name;
            this.renderType = renderType;
            this.color = color;
            this.UVIndex = UVIndex;
            tileList.Add(index, this);
        }
    }
}
