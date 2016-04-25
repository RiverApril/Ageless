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
        
        public Dictionary<Point2, Chunk> loadedChunks = new Dictionary<Point2, Chunk>();
        public Dictionary<uint, Actor> loadedActors = new Dictionary<uint, Actor>();

		public uint nextActorID = 0;

        public Game game;

        public RenderMaker chunkMaker = new RenderMaker();
        public RenderMaker actorMaker = new RenderMaker();


		public int chunkRenderDistance = 1;


        public static float Square(float a) {
            return a * a;
        }

        public World(Game game) {
            this.game = game;
            chunkMaker.initRenderMaker();
            actorMaker.initRenderMaker();
        }

        public void loadChunk(Point2 location) {
            if (!loadedChunks.ContainsKey(location)) {
                Chunk chunk = new Chunk(this, location);
                chunk.load();
                loadedChunks.Add(location, chunk);
            }
        }

        public void unloadChunk(Point2 location) {
            if (loadedChunks.ContainsKey(location)) {
                loadedChunks[location].unload();
                loadedChunks.Remove(location);
            }
        }

        public void newActor(Actor actor) {
            actor.compileState = COMP_STATUS.READY_TO_MAKE;
            actor.ID = nextActorID;
			loadedActors.Add(nextActorID, actor);
			nextActorID++;
		}

		public void revaluateChunks(){
			HashSet<Point2> visible = new HashSet<Point2>();
			Point2 center = new Point2(((int)Math.Round(game.player.position.X)) / (int)HeightMap.CHUNK_SIZE_X, ((int)Math.Round(game.player.position.Z)) / (int)HeightMap.CHUNK_SIZE_Z);
			int r = chunkRenderDistance * 2;
			for(int i=0; i<=r; i++){
				for(int j=0; j<=r; j++){
					visible.Add(new Point2(center.X + (i % 2 == 0 ? i/2 : -(i/2+1)), center.Y + (j % 2 == 0 ? j/2 : -(j/2+1))));
					//Console.WriteLine("{0}, {1}", x, y);
				}
			}
			//Console.WriteLine("");
			HashSet<Point2> chunksToUnload = new HashSet<Point2>();
			foreach(KeyValuePair<Point2, Chunk> entry in loadedChunks){
				if(!visible.Contains(entry.Key)){
					chunksToUnload.Add(entry.Key);
				}
			}
			foreach(Point2 entry in chunksToUnload){
				unloadChunk(entry);
			}
			foreach (Point2 entry in visible) {
				loadChunk(entry);
			}
		}

		public void update(Game game) {
			revaluateChunks(); //TODO make this happen less often

			foreach(KeyValuePair<uint, Actor> entry in loadedActors){
				entry.Value.update(game);
			}
        }

		public void drawChunks(Game game) {
            foreach (KeyValuePair<Point2, Chunk> entry in loadedChunks) {
                entry.Value.drawRender();
            }
        }

		public void drawActors(Game game) {

			foreach (KeyValuePair<uint, Actor> entry in loadedActors){

				game.matrixModel = Matrix4.CreateTranslation(entry.Value.position);
				game.setWMP();
				entry.Value.drawRender();
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

            if (loadedChunks.ContainsKey(c)) {

                Chunk chunk = loadedChunks[c];

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