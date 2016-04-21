using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.IO;

namespace Ageless {
    public class Chunk {

        public static readonly float GRID_HALF_SIZE = 0.5f;
        public static readonly float GRID_SIZE = GRID_HALF_SIZE*2;

        Vector3[] cubePoints = new Vector3[] {
            new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f),
        };

        public World world;

        public Point2 Location;

        public List<HeightMap> terrain = new List<HeightMap>();

        public uint[] VBOIDs = new uint[2];
        int elementCount = 0;

        bool dirty;

        public Chunk(World world, Point2 location) {
            this.world = world;
            Location = location;
            dirty = true;
        }

        public void unload() {
            //nothing yet
        }

        public void load() {

            /*HeightMap map = new HeightMap();

            Random rand = new Random();

            for (uint x = 0; x < HeightMap.CHUNK_SIZE_X; x++) {
                for (uint z = 0; z < HeightMap.CHUNK_SIZE_Z; z++) {
                    map.tiles[x, z] = Tile.tileGrass.index;
                }
            }

            for (uint x = 0; x <= HeightMap.CHUNK_SIZE_X; x++) {
                for (uint z = 0; z <= HeightMap.CHUNK_SIZE_Z; z++) {
                    map.heights[x, z] = (float)-rand.NextDouble();
                }
            }

            terrain.Add(map);*/

                    
            float resolution = 0x10;
            string letters = "abcdefghijklmnopqrstuvwxyz";
            bool loadedLetter = true;
            for (int i=0;i<26 && loadedLetter; i++) {
                loadedLetter = false;
                for (int fc = 0; fc < 2; fc++) {
                    string s = "../../htmp_";
                    s += Location.X >= 0 ? "p" : "n";
                    s += Location.X.ToString("00");
                    s += "_";
                    s += Location.Y >= 0 ? "p" : "n";
                    s += Location.Y.ToString("00");
                    s += "_";
                    s += fc == 0 ? "f" : "c";
                    s += "_";
                    s += letters[i];
                    s += ".bmp";

                    try {

                        Console.WriteLine("Try to load: {0}", s);

                        Bitmap bmp = new Bitmap(s);

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


                    } catch (FileNotFoundException) {
                        continue;
                    } catch (ArgumentException) {
                        continue;
                    }
                }
            }
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

        public void compile() {

            Console.WriteLine("Compiling Chunk");

            Dictionary<Vertex, uint> vert = new Dictionary<Vertex, uint>();
            List<uint> ind = new List<uint>();
            uint nextI = 0;

            Vector3 offset = (new Vector3(Location.X, 0, Location.Y) * new Vector3(HeightMap.CHUNK_SIZE_X, 0, HeightMap.CHUNK_SIZE_Z));

            Tile tile;
            Vector4 color;
            Vector3 normal;
            Vector3 p1, p2, p3;
            Vector3 u, v;

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

            Vertex[] vertices = new Vertex[vert.Count];
            vert.Keys.CopyTo(vertices, 0);
            uint[] indecies = ind.ToArray();

            elementCount = ind.Count;

            Console.Out.WriteLine("Vert Count = " + vert.Count);
            Console.Out.WriteLine("Ind Count = " + ind.Count);

            if (VBOIDs[0] != 0) {
                GL.DeleteBuffers(2, VBOIDs);
            }
            VBOIDs = new uint[2];
            GL.GenBuffers(2, VBOIDs);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.StrideToEnd * vertices.Length), vertices, BufferUsageHint.StaticDraw);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * indecies.Length), indecies, BufferUsageHint.StaticDraw);

        }

        public void draw() {
            //Console.Out.WriteLine("Draw chunk: " + Location.X + ", " + Location.Y);

            if (dirty) {
                dirty = false;
                compile();
            }


            int a = 0;

            GL.EnableVertexAttribArray(a); //Positions
            GL.VertexAttribPointer(a, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToPosition);

            a++;
            GL.EnableVertexAttribArray(a); //Colors
            GL.VertexAttribPointer(a, 4, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToColor);

            a++;
            GL.EnableVertexAttribArray(a); //UV
            GL.VertexAttribPointer(a, 2, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToUV);

            a++;
            GL.EnableVertexAttribArray(a); //Normals
            GL.VertexAttribPointer(a, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToNormal);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, (IntPtr)null);

        }

    }
}
