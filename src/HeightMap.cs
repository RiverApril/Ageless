using System;
using System.Collections.Generic;
using OpenTK;

namespace Ageless {
    public class HeightMap {
        
        public float[,] heights = new float[Chunk.CHUNK_SIZE_X + 2, Chunk.CHUNK_SIZE_Z + 2];

        public byte[,] tiles = new byte[Chunk.CHUNK_SIZE_X + 2, Chunk.CHUNK_SIZE_Z + 2];

		public float min, max;

        public bool isFloor = true;
        public bool isSolid = true;
        
		public char letter;

		public HeightMap(char letter) {
			this.letter = letter;
		}

		public Tile getTile(int x, int z) {
            return Tile.fromIndex(tiles[x, z]);
        }

        public byte getIndex(int x, int z) {
            return tiles[x, z];
        }

		public bool getHeightAtPosition(Vector2 p, out float height) {
            int x1 = (int)Math.Floor(p.X);
            int x2 = (int)Math.Floor(p.X + 1);
            int y1 = (int)Math.Floor(p.Y);
            int y2 = (int)Math.Floor(p.Y + 1);
            //f1-f2
            //| / |
            //f3-f4
            if (getTile(x1, y1).renderType != RenderType.None) {
                float f1 = heights[x1, y1];
                float f2 = heights[x2, y1];
                float f3 = heights[x1, y2];
                float f4 = heights[x2, y2];
                //Console.WriteLine("{0}, {1}, {2}, {3}", f1, f2, f3, f4);

                //Console.WriteLine("x1: {0}, y1: {1}, x2: {2}, y2: {3}", x1, y1, x2, y2);
                //Console.WriteLine("f1: {0}, f2: {1}, f3: {2}, f4: {3}", f1, f2, f3, f4);

                float dx = (float)(p.X - x1);
                float dy = (float)(p.Y - y1);
                float dd = 1 - (dx + dy);
                //Console.WriteLine("dx: {0}, dy: {1}\n", dx, dy);


                if (dx + dy <= 1f) {//f1, f2, f3
                    float f = (f1 * dd) + (f2 * dx) + (f3 * dy);
					//Console.WriteLine("f = {0}\n", f);
					height = f;
					return true;
                } else {//f2, f3, f4
                    dx = 1 - dx;
                    dy = 1 - dy;
                    dd = 1 - (dx + dy);
                    //Console.WriteLine("dx: {0}, dy: {1}\n", dx, dy);
                    float f = (f4 * dd) + (f2 * dy) + (f3 * dx);
					//Console.WriteLine("f = {0}\n", f);
					height = f;
					return true;
                }
            }
			height = 0;
			return false;
		}
	}
}