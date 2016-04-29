using OpenTK;
using System;
using System.Collections.Generic;

namespace Ageless {

	public enum RenderType {
		None, Terrain
	}

    public class Tile {

        public static Dictionary<byte, Tile> tileList = new Dictionary<byte, Tile>();

        public static Tile tileVoid = new Tile(0, "Void", RenderType.None, -1, false);
        public static Tile tileGrass = new Tile(1, "Grass", RenderType.Terrain, 0);

        public static Tile fromIndex(byte i) {
            return tileList[i];
        }

        public byte index { get; private set; }
        public string name { get; private set; }
        public bool solid { get; private set; }
        public int UVIndex { get; private set; }
        public RenderType renderType { get; private set; }

        public Tile(byte index, string name, RenderType renderType, int UVIndex, bool solid = true) {
            this.index = index;
            this.name = name;
            this.renderType = renderType;
            this.UVIndex = UVIndex;
            this.solid = solid;
            tileList.Add(index, this);
        }
    }
}
