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


		public int chunkRenderDistance = 2;


        public static float Square(float a) {
            return a * a;
        }

        public World(Game game) {
            this.game = game;
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
            actor.compileState = COMP_STATUS.NEEDS_TO_BE_MADE;
            actor.ID = nextActorID;
			loadedActors.Add(nextActorID, actor);
			nextActorID++;
		}

		public void revaluateChunks(){
			HashSet<Point2> visible = new HashSet<Point2>();
			Vector2 center = new Vector2(
                (game.player.position.X / Chunk.CHUNK_SIZE_X) - .5f, 
                (game.player.position.Z / Chunk.CHUNK_SIZE_Z) - .5f
                );
			int r = chunkRenderDistance * 2;
			for(int i=0; i<=r; i++){
				for(int j=0; j<=r; j++){
					visible.Add(new Point2((int)center.X + (i % 2 == 0 ? i/2 : -(i/2+1)), (int)center.Y + (j % 2 == 0 ? j/2 : -(j/2+1))));
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
			
			game.matrixModel = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
			game.setModel();

            foreach (KeyValuePair<Point2, Chunk> entry in loadedChunks) {
                entry.Value.drawRender();
            }
        }

		public void drawActors(Game game) {
			foreach (KeyValuePair<uint, Actor> entry in loadedActors){

				game.matrixModel = Matrix4.CreateTranslation(entry.Value.position);
				game.setModel();

				entry.Value.drawRender();
			}
		}

        public float getFloorAtPosition(float x, float y, float z) {
            x /= Chunk.GRID_SIZE;
            z /= Chunk.GRID_SIZE;
            x += 0.5f;
            z += 0.5f;
            Point2 c = new Point2((int)Math.Floor(x / Chunk.CHUNK_SIZE_X), (int)Math.Floor(z / Chunk.CHUNK_SIZE_Z));
            Vector2d p = new Vector2d(x - (c.X * Chunk.CHUNK_SIZE_X), z - (c.Y * Chunk.CHUNK_SIZE_Z));

            //Console.WriteLine("Chunk = {0}, {1}", c.X, c.Y);
            //Console.WriteLine("Pos = {0}, {1}", p.X, p.Y);

            if (loadedChunks.ContainsKey(c)) {

                Chunk chunk = loadedChunks[c];

                List<float> heights = new List<float>();

                foreach (HeightMap htmp in chunk.terrain) {
                    if (htmp.isSolid) {
                        int x1 = (int)Math.Floor(p.X);
                        int x2 = (int)Math.Floor(p.X + 1);
                        int y1 = (int)Math.Floor(p.Y);
                        int y2 = (int)Math.Floor(p.Y + 1);
                        //f1-f2
                        //| / |
                        //f3-f4
                        if (htmp.getTile(x1, y1).solid) {
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