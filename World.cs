using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Threading;

namespace Ageless {
    public class World {

        public static readonly float SQRT2f = (float)Math.Sqrt(2);

        public Dictionary<Point2, Chunk> LoadedChunks = new Dictionary<Point2, Chunk>();

        public Game game;
        private Thread thread;

        public List<Chunk> compileList = new List<Chunk>();

        public static float Square(float a) {
            return a * a;
        }

        public World(Game game) {
            this.game = game;
            thread = new Thread(compileChunks);
            thread.Start();
        }

        private void compileChunks() {
            while (true) {
                while (compileList.Count > 0) {
                    compileList[0].makeChunkVertices();
                    compileList.RemoveAt(0);
                }
            }
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

        

        public float getFloorAtPosition(float x, float y, float z) {
            x /= Chunk.GRID_SIZE;
            z /= Chunk.GRID_SIZE;
            x += 0.5f;
            z += 0.5f;
            Point2 c = new Point2((int)Math.Floor(x / HeightMap.CHUNK_SIZE_X), (int)Math.Floor(z / HeightMap.CHUNK_SIZE_Z));
            Vector2d p = new Vector2d(x - (c.X * HeightMap.CHUNK_SIZE_X), z - (c.Y * HeightMap.CHUNK_SIZE_Z));

            //Console.WriteLine("Chunk = {0}, {1}", c.X, c.Y);
            //Console.WriteLine("Pos = {0}, {1}", p.X, p.Y);

            if (LoadedChunks.ContainsKey(c)) {

                Chunk chunk = LoadedChunks[c];

                List<float> heights = new List<float>();

                foreach (HeightMap htmp in chunk.terrain) {
                    int x1 = (int)Math.Floor(p.X);
                    int x2 = (int)Math.Floor(p.X + 1);
                    int y1 = (int)Math.Floor(p.Y);
                    int y2 = (int)Math.Floor(p.Y + 1);
                    //f1-f2
                    //| / |
                    //f3-f4
                    float f1 = htmp.heights[x1, y1];
                    float f2 = htmp.heights[x2, y1];
                    float f3 = htmp.heights[x1, y2];
                    float f4 = htmp.heights[x2, y2];
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
                        heights.Add(f);
                    } else {//f2, f3, f4
                        dx = 1 - dx;
                        dy = 1 - dy;
                        dd = 1 - (dx + dy);
                        //Console.WriteLine("dx: {0}, dy: {1}\n", dx, dy);
                        float f = (f4 * dd) + (f2 * dy) + (f3 * dx);
                        //Console.WriteLine("f = {0}\n", f);
                        heights.Add(f);
                    }
                }

                float h = 0;

                foreach (float f in heights) {
                    if (f > h && f <= y) {
                        h = f;
                    }
                }

                return h;

            }

            return 0;

        }
    }
}