using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace Ageless {
    public class World {

        public static readonly float SQRT2f = (float)Math.Sqrt(2);

        public Dictionary<Point2, Chunk> LoadedChunks = new Dictionary<Point2, Chunk>();

        public Game game;

        public static float Square(float a) {
            return a * a;
        }

        public World(Game game) {
            this.game = game;
        }

        public void loadChunk(Point2 location) {
            if (LoadedChunks.ContainsKey(location)) {
                LoadedChunks[location].reload();
            } else {
                Chunk chunk = new Chunk(this, location);
                chunk.load();
                LoadedChunks.Add(location, chunk);
            }
        }

        public void unloadChunk(Point2 location) {
            if (LoadedChunks.ContainsKey(location)) {
                LoadedChunks[location].unload();
                LoadedChunks.Remove(location);
            }
        }

        public void drawChunks() {

            foreach (KeyValuePair<Point2, Chunk> entry in LoadedChunks) {
                entry.Value.draw();
            }
        }

        public float getHeightAtPosition(float x, float z) {
            x /= Chunk.GRID_SIZE;
            z /= Chunk.GRID_SIZE;
            x += 0.5f;
            z += 0.5f;
            Point2 c = new Point2((int)Math.Floor(x / Chunk.CHUNK_SIZE_X), (int)Math.Floor(z / Chunk.CHUNK_SIZE_Z));
            Vector2d p = new Vector2d(x - (c.X * Chunk.CHUNK_SIZE_X), z - (c.Y * Chunk.CHUNK_SIZE_Z));

            //Console.WriteLine("Chunk = {0}, {1}", c.X, c.Y);
            //Console.WriteLine("Pos = {0}, {1}", p.X, p.Y);

            if (LoadedChunks.ContainsKey(c)) {
                int x1 = (int)Math.Floor(p.X);
                int x2 = (int)Math.Floor(p.X+1);
                int y1 = (int)Math.Floor(p.Y);
                int y2 = (int)Math.Floor(p.Y+1);
                //f1-f2
                //| / |
                //f3-f4
                float f1 = LoadedChunks[c].heightMap[x1, y1];
                float f2 = LoadedChunks[c].heightMap[x2, y1];
                float f3 = LoadedChunks[c].heightMap[x1, y2];
                float f4 = LoadedChunks[c].heightMap[x2, y2];
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
                    return f;
                } else {//f2, f3, f4
                    dx = 1 - dx;
                    dy = 1 - dy;
                    dd = 1 - (dx + dy);
                    //Console.WriteLine("dx: {0}, dy: {1}\n", dx, dy);
                    float f = (f4 * dd) + (f2 * dy) + (f3 * dx);
                    //Console.WriteLine("f = {0}\n", f);
                    return f;
                }

            }

            return 0;

        }
    }
}