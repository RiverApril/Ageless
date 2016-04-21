using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;

namespace Ageless {
    public class Chunk {

        public static readonly float GRID_HALF_SIZE = 0.5f;
        public static readonly float GRID_SIZE = GRID_HALF_SIZE*2;

        public static readonly uint CHUNK_SIZE_X = 128;
        public static readonly uint CHUNK_SIZE_Z = 128;

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

        public float[,] heightMap = new float[CHUNK_SIZE_X+1, CHUNK_SIZE_Z+1];

        public int[,] tileMap = new int[CHUNK_SIZE_X, CHUNK_SIZE_Z];

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
            if (false) { //Check if chunk file exists
                //Load chunk file
            } else {

                Random rand = new Random();

                for (uint x = 0; x < CHUNK_SIZE_X; x++) {
                    for (uint z = 0; z < CHUNK_SIZE_Z; z++) {
                        tileMap[x, z] = Tile.tileGrass.index;
                    }
                }

                for (uint x = 0; x <= CHUNK_SIZE_X; x++) {
                    for (uint z = 0; z <= CHUNK_SIZE_Z; z++) {
                        heightMap[x, z] = (float)-rand.NextDouble();
                    }
                }

            }
        }

        public void reload() {
            unload();
            load();
        }

        public void tryToAdd(Vector3 p, /*Vector3 normal,*/ Vector4 color, Vector2 UV, ref Dictionary<Vertex, ushort> vert, ref List<ushort> ind, ref ushort nextI) {
            Vertex v = new Vertex(p, /*normal,*/ color, UV);
            if (!vert.ContainsKey(v)) {
                vert.Add(v, nextI);
                nextI++;
            }
            ind.Add(vert[v]);
        }

        public Tile getTile(int x, int z) {
            return Tile.fromIndex(tileMap[x, z]);
        }

        public void compile() {

            Dictionary<Vertex, ushort> vert = new Dictionary<Vertex, ushort>();
            List<ushort> ind = new List<ushort>();
            ushort nextI = 0;

            Vector3 offset = (new Vector3(Location.X, 0, Location.Y) * new Vector3(CHUNK_SIZE_X, 0, CHUNK_SIZE_Z));

            Tile tile;
            Vector4 color;
            //Vector3 normal;
            Vector3 p1, p2, p3;
            //Vector3 u, v;

            for (int x = 0; x < CHUNK_SIZE_X; x++) {
                for (int z = 0; z < CHUNK_SIZE_Z; z++) {

                    tile = getTile(x, z);

                    if (tile.renderType == RenderType.Terrain) {
                        color = tile.color;

                        p1 = new Vector3(x - GRID_HALF_SIZE, heightMap[x, z], z - GRID_HALF_SIZE) + offset;
                        p2 = new Vector3(x + GRID_HALF_SIZE, heightMap[x + 1, z], z - GRID_HALF_SIZE) + offset;
                        p3 = new Vector3(x - GRID_HALF_SIZE, heightMap[x, z + 1], z + GRID_HALF_SIZE) + offset;
                        /*u = p2 - p1;
                        v = p3 - p1;
                        normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                        normal.Y = (u.Z * v.X) - (u.X * v.Z);
                        normal.Z = (u.X * v.Y) - (u.Y * v.X);*/
                        tryToAdd(p1, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 0], ref vert, ref ind, ref nextI);
                        tryToAdd(p2, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 1], ref vert, ref ind, ref nextI);
                        tryToAdd(p3, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 2], ref vert, ref ind, ref nextI);

                        p1 = new Vector3(x + GRID_HALF_SIZE, heightMap[x + 1, z + 1], z + GRID_HALF_SIZE) + offset;
                        p2 = new Vector3(x - GRID_HALF_SIZE, heightMap[x, z + 1], z + GRID_HALF_SIZE) + offset;
                        p3 = new Vector3(x + GRID_HALF_SIZE, heightMap[x + 1, z], z - GRID_HALF_SIZE) + offset;
                        /*u = p2 - p1;
                        v = p3 - p1;
                        normal.X = (u.Y * v.Z) - (u.Z * v.Y);
                        normal.Y = (u.Z * v.X) - (u.X * v.Z);
                        normal.Z = (u.X * v.Y) - (u.Y * v.X);*/
                        tryToAdd(p1, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 3], ref vert, ref ind, ref nextI);
                        tryToAdd(p2, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 2], ref vert, ref ind, ref nextI);
                        tryToAdd(p3, /*normal,*/ color, TextureControl.tex16x16Coords[tile.UVIndex, 1], ref vert, ref ind, ref nextI);
                    }
                }
            }

            Vertex[] vertices = new Vertex[vert.Count];
            vert.Keys.CopyTo(vertices, 0);
            ushort[] indecies = ind.ToArray();

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
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(short) * indecies.Length), indecies, BufferUsageHint.StaticDraw);

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

            /*a++;
            GL.EnableVertexAttribArray(a); //Normals
            GL.VertexAttribPointer(a, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToNormal);*/

            a++;
            GL.EnableVertexAttribArray(a); //Colors
            GL.VertexAttribPointer(a, 4, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToColor);

            a++;
            GL.EnableVertexAttribArray(a); //UV
            GL.VertexAttribPointer(a, 2, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToUV);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedShort, (IntPtr)null);

        }

    }
}
