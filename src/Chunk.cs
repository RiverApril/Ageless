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

        public static readonly float GRID_HALF_SIZE = 0.5f;
        public static readonly float GRID_SIZE = GRID_HALF_SIZE*2;

        /*Vector3[] cubePoints = new Vector3[] {
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
        };*/

        public World world;

        public Point2 Location;

        public List<HeightMap> terrain = new List<HeightMap>();

        public bool markForRemoval = false;

        public Chunk(World world, Point2 location) : base(world.chunkMaker) {
            this.world = world;
            Location = location;
        }

        public void unload() {
            cleanupRender();
        }

        public void load() {
            
            float resolution = 0x10;
            string letters = "abcdefghijklmnopqrstuvwxyz";
            bool loadedLetter = true;
            for (int i=0;i<26 && loadedLetter; i++) {
                loadedLetter = false;
                for (int fc = 0; fc < 2; fc++) {
					string path = Game.dirMap+"htmp_";
                    path += Location.X >= 0 ? "p" : "n";
                    path += Math.Abs(Location.X).ToString("00");
                    path += "_";
                    path += Location.Y >= 0 ? "p" : "n";
                    path += Math.Abs(Location.Y).ToString("00");
                    path += "_";
                    path += fc == 0 ? "f" : "c";
                    path += "_";
                    path += letters[i];
                    path += ".png";

                    try {


						if(File.Exists(path)){

	                        Console.WriteLine("Try to load: {0}", path);



							using(Image img = Image.FromFile(path)){

								using(Bitmap bmp = new Bitmap(img)){

			                        if (bmp.Width != HeightMap.CHUNK_SIZE_X + 1 || bmp.Height != HeightMap.CHUNK_SIZE_Z + 1) {
			                            throw new FormatException(String.Format("Image size not equal to {0}x{1}, is instead {2}x{3}", HeightMap.CHUNK_SIZE_X + 1, HeightMap.CHUNK_SIZE_Z + 1, bmp.Width, bmp.Height));
			                        }

			                        HeightMap htmp = new HeightMap();

			                        htmp.isFloor = fc == 0;
			                        htmp.isCeiling = fc == 1;

			                        for (int x = 0; x <= HeightMap.CHUNK_SIZE_X; x++) {
			                            for (int z = 0; z <= HeightMap.CHUNK_SIZE_Z; z++) {
			                                Color c = bmp.GetPixel(x, z);
			                                if (x < HeightMap.CHUNK_SIZE_X && z < HeightMap.CHUNK_SIZE_Z) {
			                                    htmp.tiles[x, z] = c.R;
			                                }
			                                htmp.heights[x, z] = (((int)c.G) | (((int)c.B) << 0x100)) / resolution;
			                            }
			                        }

			                        terrain.Add(htmp);
			                        loadedLetter = true;
			                        Console.WriteLine("Success");

									bmp.Dispose();
								}
							}
						}


					} catch (FileNotFoundException) {
						Console.WriteLine("File not found");
                        continue;
					}
                }
            }

            compileState = COMP_STATUS.READY_TO_MAKE;

        }

        public void reload() {
            unload();
            load();
        }

        public void tryToAdd(Vector3 p, Vector3 normal, Vector4 color, Vector2 UV, ref Dictionary<Vertex, uint> vert, ref List<uint> ind, ref uint nextI) {
            Vertex v = new Vertex(p, color, UV, normal);
            if (!vert.ContainsKey(v)) {
                vert.Add(v, nextI);
                nextI++;
            }
            ind.Add(vert[v]);
        }

        public override void makeRender() {
			Console.WriteLine("Making Chunk {0}, {1}", Location.X, Location.Y);

            vert = new Dictionary<Vertex, uint>();
            ind = new List<uint>();
            uint nextI = 0;

            Vector3 offset = (new Vector3(Location.X, 0, Location.Y) * new Vector3(HeightMap.CHUNK_SIZE_X, 0, HeightMap.CHUNK_SIZE_Z));

            Tile tile;
            Vector4 color;
            Vector3 normal;
            Vector3 p1, p2, p3;
            Vector3 u, v;

			Console.WriteLine("Heightmap count: {0}", terrain.Count);

            foreach (HeightMap htmp in terrain) {
                for (int x = 0; x < HeightMap.CHUNK_SIZE_X; x++) {
                    for (int z = 0; z < HeightMap.CHUNK_SIZE_Z; z++) {

                        tile = htmp.getTile(x, z);

                        if (tile.renderType == RenderType.Terrain) {
                            color = tile.color;

                            if (htmp.isFloor) {

                                p1 = new Vector3(x - GRID_HALF_SIZE, htmp.heights[x, z], z - GRID_HALF_SIZE) + offset;
                                p2 = new Vector3(x + GRID_HALF_SIZE, htmp.heights[x + 1, z], z - GRID_HALF_SIZE) + offset;
                                p3 = new Vector3(x - GRID_HALF_SIZE, htmp.heights[x, z + 1], z + GRID_HALF_SIZE) + offset;
                                u = p2 - p1;
                                v = p3 - p1;
                                normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                                normal.Y = (u.Z * v.X) - (u.X * v.Z);
                                normal.Z = (u.X * v.Y) - (u.Y * v.X);
                                tryToAdd(p1, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 0], ref vert, ref ind, ref nextI);
                                tryToAdd(p2, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 1], ref vert, ref ind, ref nextI);
                                tryToAdd(p3, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 2], ref vert, ref ind, ref nextI);

                                p1 = new Vector3(x + GRID_HALF_SIZE, htmp.heights[x + 1, z + 1], z + GRID_HALF_SIZE) + offset;
                                p2 = new Vector3(x - GRID_HALF_SIZE, htmp.heights[x, z + 1], z + GRID_HALF_SIZE) + offset;
                                p3 = new Vector3(x + GRID_HALF_SIZE, htmp.heights[x + 1, z], z - GRID_HALF_SIZE) + offset;
                                u = p2 - p1;
                                v = p3 - p1;
                                normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                                normal.Y = (u.Z * v.X) - (u.X * v.Z);
                                normal.Z = (u.X * v.Y) - (u.Y * v.X);
                                tryToAdd(p1, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 3], ref vert, ref ind, ref nextI);
                                tryToAdd(p2, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 2], ref vert, ref ind, ref nextI);
                                tryToAdd(p3, normal, color, TextureControl.tex16x16Coords[tile.UVIndex, 1], ref vert, ref ind, ref nextI);
                            }

                            if (htmp.isCeiling) {
                                //TODO
                            }
                        }
                    }
                }
            }

			Console.WriteLine("Made Chunk {0}, {1}", Location.X, Location.Y);

            compileState = COMP_STATUS.READY_TO_COMPILE;

        }

    }
}
