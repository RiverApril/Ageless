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
    public class Map {

        public static readonly float SQRT2f = (float)Math.Sqrt(2);
        
        public Dictionary<Point2, Chunk> loadedChunks = new Dictionary<Point2, Chunk>();
        public Dictionary<uint, Actor> loadedActors = new Dictionary<uint, Actor>();

		public uint nextActorID = 0;

        public Game game;

		public string name;
        
        public bool chunksLocked = false;


		public int chunkRenderDistance = 2;

		private bool unloadAll = false;


        public static float Square(float a) {
            return a * a;
        }

        public Map(Game game, string name) {
            this.game = game;
			this.name = name;
			Data.loadMap(this);
        }

		public void lockChunks() {
			chunksLocked = true;
		}

		public void unlockChunks() {
			chunksLocked = false;
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

		internal void unloadAllChunks() {
			unloadAll = true;
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
			if (!chunksLocked) {
				revaluateChunks(); //TODO make this happen less often

				if (unloadAll) {
					while (loadedChunks.Count > 0) {
						var e = loadedChunks.Keys.GetEnumerator();
						e.MoveNext();
						unloadChunk(e.Current);
					}
					unloadAll = false;
				}
			}

			foreach(Actor actor in loadedActors.Values){
				actor.update(game);
            }

            foreach (Chunk chunk in loadedChunks.Values) {
                chunk.update(game);
            }
        }

		public void drawChunks(Game game) {
			
			game.matrixModel = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
			game.setModel();

            game.setTexture(TextureControl.arrayTerrain.id);

            foreach (Chunk chunk in loadedChunks.Values) {
                chunk.drawRender();
            }

            game.setTexture(TextureControl.arrayProps.id);

            foreach (Chunk chunk in loadedChunks.Values) {
				chunk.drawProps(game);
			}
        }

		public void drawActors(Game game) {

            game.setTexture(TextureControl.arrayActors.id);

            foreach (Actor actor in loadedActors.Values){
                actor.draw(game);
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

				return loadedChunks[c].getFloorAtPosition(p, y);

            }

            return -1;

        }

}
}