using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Ageless {
    public class Chunk : Renderable{

		public static readonly int CHUNK_SIZE_X = 128;
		public static readonly int CHUNK_SIZE_Z = 128;

        public static readonly float GRID_HALF_SIZE = 0.5f;
        public static readonly float GRID_SIZE = GRID_HALF_SIZE*2;

		public float resolution = 4;

        public Map map;

        public Point2 Location;

        public List<HeightMap> terrain = new List<HeightMap>();

        public List<Prop> props = new List<Prop>();

        public Chunk(Map map, Point2 location) : base() {
            this.map = map;
            Location = location;
        }

        public void unload() {
            compileState = COMP_STATUS.NEEDS_TO_BE_REMOVED;
            Console.WriteLine("Unloaded Chunk: {0}, {1}", Location.X, Location.Y);
        }

        public void load() {

			Data.loadChunk(this);

            if (terrain.Count > 0) {
                compileState = COMP_STATUS.NEEDS_TO_BE_MADE;
            }

        }

		public Vector3 calcNorm(HeightMap htmp, int x, int z) {
            Vector3 sum = Vector3.Zero;

            Vector3 side1 = new Vector3(x + 1, htmp.heights[x, z], z) - new Vector3(x, htmp.heights[x, z], z);
            Vector3 side2 = new Vector3(x, htmp.heights[x, z], z + 1) - new Vector3(x + 1, htmp.heights[x, z], z);
            sum += Vector3.Cross(side1, side2);

            side1 = new Vector3(x + 1, htmp.heights[x, z], z) - new Vector3(x, htmp.heights[x, z], z);
            side2 = new Vector3(x, htmp.heights[x, z], z - 1) - new Vector3(x + 1, htmp.heights[x, z], z);
            sum += Vector3.Cross(side1, side2);

            side1 = new Vector3(x + 1, htmp.heights[x, z], z - 1) - new Vector3(x, htmp.heights[x, z], z);
            side2 = new Vector3(x, htmp.heights[x, z], z - 1) - new Vector3(x + 1, htmp.heights[x, z], z - 1);
            sum += Vector3.Cross(side1, side2);

            side1 = new Vector3(x - 1, htmp.heights[x, z], z) - new Vector3(x, htmp.heights[x, z], z);
            side2 = new Vector3(x, htmp.heights[x, z], z - 1) - new Vector3(x - 1, htmp.heights[x, z], z);
            sum += Vector3.Cross(side1, side2);

            side1 = new Vector3(x - 1, htmp.heights[x, z], z) - new Vector3(x, htmp.heights[x, z], z);
            side2 = new Vector3(x, htmp.heights[x, z], z - 1) - new Vector3(x - 1, htmp.heights[x, z], z);
            sum += Vector3.Cross(side1, side2);

            side1 = new Vector3(x, htmp.heights[x, z], z + 1) - new Vector3(x, htmp.heights[x, z], z);
            side2 = new Vector3(x - 1, htmp.heights[x, z], z - 1) - new Vector3(x, htmp.heights[x, z], z + 1);
            sum += Vector3.Cross(side1, side2);

            sum.Normalize();
            return sum;
        }

        public override void makeRender() {

			Console.WriteLine("(Render) Making Chunk {0}, {1}", Location.X, Location.Y);

			vert = new List<Vertex>();
            ind = new List<uint>();
            uint nextI = 0;

            Vector3 offset = (new Vector3(Location.X, 0, Location.Y) * new Vector3(CHUNK_SIZE_X, 0, CHUNK_SIZE_Z));

            Tile tile;
            Vector3 n1 = new Vector3(), n2 = new Vector3(), n3 = new Vector3();
            Vector3 p1 = new Vector3(), p2 = new Vector3(), p3 = new Vector3();
            Vector3 t = new Vector3();

            int htmpi = 0;

			foreach (HeightMap htmp in terrain) {

				htmpi++;

				Console.WriteLine("(Render) Making Heightmap {0} of {1}", htmpi, terrain.Count);

                for (int x = 1; x <= CHUNK_SIZE_X; x++) {
                    for (int z = 1; z <= CHUNK_SIZE_Z; z++) {

                        tile = htmp.getTile(x, z);

                        if (tile.renderType == RenderType.Terrain) {
							
                            p1.X = x - GRID_HALF_SIZE + offset.X - 1;   p1.Y = htmp.heights[x, z] + offset.Y;       p1.Z = z - GRID_HALF_SIZE + offset.Z - 1;
							p2.X = x + GRID_HALF_SIZE + offset.X - 1;   p2.Y = htmp.heights[x + 1, z] + offset.Y;   p2.Z = z - GRID_HALF_SIZE + offset.Z - 1;
							p3.X = x - GRID_HALF_SIZE + offset.X - 1;   p3.Y = htmp.heights[x, z + 1] + offset.Y;   p3.Z = z + GRID_HALF_SIZE + offset.Z - 1;

                            //u.X = p2.X - p1.X;   u.Y = p2.Y - p1.Y;   u.Z = p2.Z - p1.Z;
                            //v.X = p3.X - p1.X;   v.Y = p3.Y - p1.Y;   v.Z = p3.Z - p1.Z;

                            //normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                            //normal.Y = (u.Z * v.X) - (u.X * v.Z);
                            //normal.Z = (u.X * v.Y) - (u.Y * v.X);

                            n1 = calcNorm(htmp, x, z);
                            n2 = calcNorm(htmp, x + 1, z);
                            n3 = calcNorm(htmp, x, z + 1);

                            addVert(ref p1, ref n1, ref TextureControl.arrayTerrain.coords[tile.UVIndex, x % TextureControl.arrayTerrain.scale, z % TextureControl.arrayTerrain.scale, 0], ref vert, ref ind, ref nextI);
							addVert(ref p3, ref n3, ref TextureControl.arrayTerrain.coords[tile.UVIndex, x % TextureControl.arrayTerrain.scale, z % TextureControl.arrayTerrain.scale, 2], ref vert, ref ind, ref nextI);
							addVert(ref p2, ref n2, ref TextureControl.arrayTerrain.coords[tile.UVIndex, x % TextureControl.arrayTerrain.scale, z % TextureControl.arrayTerrain.scale, 1], ref vert, ref ind, ref nextI);


                            p1.X = x + GRID_HALF_SIZE + offset.X - 1;   p1.Y = htmp.heights[x + 1, z + 1] + offset.Y;   p1.Z = z + GRID_HALF_SIZE + offset.Z - 1;
							//p2.X = x - GRID_HALF_SIZE + offset.X - 1;   p2.Y = htmp.heights[x, z + 1] + offset.Y;       p2.Z = z + GRID_HALF_SIZE + offset.Z - 1;
							//p3.X = x + GRID_HALF_SIZE + offset.X - 1;   p3.Y = htmp.heights[x + 1, z] + offset.Y;       p3.Z = z - GRID_HALF_SIZE + offset.Z - 1;

                            //u.X = p2.X - p1.X;   u.Y = p2.Y - p1.Y;   u.Z = p2.Z - p1.Z;
                            //v.X = p3.X - p1.X;   v.Y = p3.Y - p1.Y;   v.Z = p3.Z - p1.Z;

                            //normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                            //normal.Y = (u.Z * v.X) - (u.X * v.Z);
                            //normal.Z = (u.X * v.Y) - (u.Y * v.X);

                            n1 = calcNorm(htmp, x + 1, z + 1);

                            addVert(ref p1, ref n1, ref TextureControl.arrayTerrain.coords[tile.UVIndex, x % TextureControl.arrayTerrain.scale, z % TextureControl.arrayTerrain.scale, 3], ref vert, ref ind, ref nextI);
							addInd(ref nextI, -2);
							addInd(ref nextI, -3);
							//tryToAdd(ref p2, ref normal, ref TextureControl.tex16x16Coords[tile.UVIndex, 2], ref vert, ref ind, ref nextI);
							//tryToAdd(ref p3, ref normal, ref TextureControl.tex16x16Coords[tile.UVIndex, 1], ref vert, ref ind, ref nextI);
                            
                        }
                    }
                }
            }

			Console.WriteLine("(Render) Made Chunk {0}, {1}", Location.X, Location.Y);

        }

        public void update(Game game) {
            foreach (Prop prop in props) {
                prop.update(game);
            }
        }

		public void drawProps(Game game) {
			foreach (Prop prop in props) {
				prop.draw(game);
            }
            foreach (Prop prop in props) {
                prop.secondaryDraw(game);
            }
        }

		public float getFloorAtPosition(Vector2 p, float y) {
            List<float> heights = new List<float>();

            foreach (HeightMap htmp in terrain) {
				float o;
				if (htmp.getHeightAtPosition(p, out o)) {
					heights.Add(o);
				}
            }
            
            Vector3 origin = new Vector3(p.X, Game.FAR, p.Y);
            Vector3 direction = new Vector3(0, -1, 0);

            foreach (Prop prop in props) {
                if (prop.solid && Game.intercectAABBRay(prop.frameMin, prop.frameMax, ref origin, ref direction)) {
                    float d = 0;

                    for (int i = 0; i < prop.model.ind.Count; i += 3) {
                        if (Game.intercectTriangleRay(prop.transformedPoints[(int)prop.model.ind[i]], prop.transformedPoints[(int)prop.model.ind[i + 1]], prop.transformedPoints[(int)prop.model.ind[i + 2]], ref origin, ref direction, out d)) {
                            heights.Add((origin + (direction * d)).Y);
                        }
                    }
                }
            }

            float h = 0;

            /*if (heights.Count >= 2) {
                Console.WriteLine("Height: {0}", y);
                foreach (float f in heights) {
                    Console.WriteLine(f);
                }
            }*/

            foreach (float f in heights) {
                if (f > h && f <= y) {
                    h = f;
                }
            }

            return h;
		}


    }
}
