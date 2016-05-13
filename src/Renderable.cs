
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ageless {

    public enum COMP_STATUS {
        DO_NOT_RENDER, NEEDS_TO_BE_MADE, CURRENTLY_MAKING, NEEDS_TO_BE_COMPILED, READY_TO_RENDER, NEEDS_TO_BE_REMOVED
    }

    public abstract class Renderable {

        private uint[] VBOIDs;
        private int elementCount = 0;
        public COMP_STATUS compileState = COMP_STATUS.DO_NOT_RENDER;
        

		public List<Vertex> vert = null;
		public List<uint> ind = null;

		public bool keepVerts = false;

		public Thread makeThread = null;

        public Renderable() {
			makeThread = new Thread(privateMakeRender);
        }

        protected void cleanupRender() {
            compileState = COMP_STATUS.DO_NOT_RENDER;
            vert = null;
			ind = null;
			if (VBOIDs != null) {
				GL.DeleteBuffers(2, VBOIDs);
				VBOIDs = null;
			}
        }

        abstract public void makeRender();

		public void addVert(ref Vector3 p, ref Vector3 normal, ref Vector3 UV, ref List<Vertex> vert, ref List<uint> ind, ref uint nextI) {
			Vertex v = new Vertex(p, UV, normal);
			vert.Add(v);
			ind.Add(nextI);
			nextI++;
		}

		public void addInd(ref uint nextI, int offset){
			ind.Add((uint)(nextI + offset));
		}

		//HORRIBLY SLOW:
		/*public void tryToAdd(ref Vector3 p, ref Vector3 normal, ref Vector2 UV, ref Dictionary<Vertex, uint> vert, ref List<uint> ind, ref uint nextI) {
			Vertex v = new Vertex(p, UV, normal);
			if (!vert.ContainsKey(v)) {
				vert.Add(v, nextI);
				nextI++;
			}
			ind.Add(vert[v]);
		}*/

		public void compileRender() {

			//Console.WriteLine("(Render) Array creation");

			Vertex[] vertices = vert.ToArray();
            uint[] indecies = ind.ToArray();

            elementCount = ind.Count;

			Console.Out.WriteLine("(Render) Vert Count = " + vert.Count);
			Console.Out.WriteLine("(Render) Ind Count = " + ind.Count);


			//Console.WriteLine("(Render) Opengl Chunk compilation calls");

			if(VBOIDs == null) {
				VBOIDs = new uint[2];
				TryGL.Call(() => GL.GenBuffers(2, VBOIDs));
				Console.WriteLine("(Render) New Buffers: [{0}, {1}]", VBOIDs[0], VBOIDs[1]);
			}

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.StrideToEnd * vertices.Length), vertices, BufferUsageHint.StaticDraw);
			//GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * indecies.Length), indecies, BufferUsageHint.StaticDraw);
			//GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

			//Console.WriteLine("(Render) done");

			if (!keepVerts) {
				vert = null;
                ind = null;
            }

        }

		private void privateMakeRender() {
			makeRender();
			compileState = COMP_STATUS.NEEDS_TO_BE_COMPILED;
		}


        public void drawRender() {
            //Console.Out.WriteLine("Draw chunk: " + Location.X + ", " + Location.Y);

            switch (compileState) {
                case COMP_STATUS.NEEDS_TO_BE_MADE: {
					compileState = COMP_STATUS.CURRENTLY_MAKING;
					makeThread.Start();
					return;
				}
                case COMP_STATUS.NEEDS_TO_BE_COMPILED: {
                    compileRender();
                    compileState = COMP_STATUS.READY_TO_RENDER;
                    return;
                }
                case COMP_STATUS.READY_TO_RENDER: {

                    /*if (this as Chunk != null) {
						Console.Out.WriteLine("Draw chunk: {0}, {1}  VBO:[{2}, {3}] elCount={4}", (this as Chunk).Location.X, (this as Chunk).Location.Y, VBOIDs[0], VBOIDs[1], elementCount);
                    }else if (this as Actor != null) {
						Console.Out.WriteLine("Draw actor: {0}  VBO:[{1}, {2}] elCount={3}", (this as Actor).ID, VBOIDs[0], VBOIDs[1], elementCount);
					}*/

					TryGL.Call(() => GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]));
					TryGL.Call(() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]));

                    TryGL.Call(() => GL.EnableVertexAttribArray(0)); //Positions
                    TryGL.Call(() => GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToPosition));

                    TryGL.Call(() => GL.EnableVertexAttribArray(1)); //UV
                    TryGL.Call(() => GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToUV));

                    TryGL.Call(() => GL.EnableVertexAttribArray(2)); //Normals
                    TryGL.Call(() => GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToNormal));

					TryGL.Call(() => GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, (IntPtr)null));
					//GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
					//GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

                    return;
                }
                case COMP_STATUS.NEEDS_TO_BE_REMOVED: {
                    cleanupRender();
                    return;
                }
            }

        }

    }
}
