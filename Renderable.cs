using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {

    public enum COMP_STATUS {
        NO_RENDER, READY_TO_MAKE, MAKING, READY_TO_COMPILE, READY_TO_RENDER
    }

    public abstract class Renderable {

        protected RenderMaker renderMaker;

        protected uint[] VBOIDs = new uint[2];
        protected int elementCount = 0;
        protected COMP_STATUS compileState = COMP_STATUS.NO_RENDER;
        

        protected Dictionary<Vertex, uint> vert = null;
        protected List<uint> ind = null;

        public Renderable(RenderMaker renderMaker) {
            this.renderMaker = renderMaker;
        }

        protected void cleanupRender() {
            compileState = COMP_STATUS.NO_RENDER;
            vert = null;
            ind = null;
        }

        abstract public void makeRender();

        public void compileRender() {

            Console.WriteLine("Array creation");

            Vertex[] vertices = new Vertex[vert.Count];
            vert.Keys.CopyTo(vertices, 0);
            uint[] indecies = ind.ToArray();

            elementCount = ind.Count;

            Console.Out.WriteLine("Vert Count = " + vert.Count);
            Console.Out.WriteLine("Ind Count = " + ind.Count);


            Console.WriteLine("Opengl Chunk compilation calls");

            if (VBOIDs[0] != 0) {
                GL.DeleteBuffers(2, VBOIDs);
            }
            VBOIDs = new uint[2];
            GL.GenBuffers(2, VBOIDs);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.StrideToEnd * vertices.Length), vertices, BufferUsageHint.StaticDraw);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(sizeof(uint) * indecies.Length), indecies, BufferUsageHint.StaticDraw);

            Console.WriteLine("done");

            vert = null;
            ind = null;

            compileState = COMP_STATUS.READY_TO_RENDER;

        }


        public void drawRender() {
            //Console.Out.WriteLine("Draw chunk: " + Location.X + ", " + Location.Y);

            switch (compileState) {
                case COMP_STATUS.READY_TO_MAKE: {
                    renderMaker.list.Add(this);
                    compileState = COMP_STATUS.MAKING;
                    return;
                }
                case COMP_STATUS.READY_TO_COMPILE: {
                    compileRender();
                    return;
                }
                case COMP_STATUS.READY_TO_RENDER: {

                    GL.EnableVertexAttribArray(0); //Positions
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToPosition);

                    GL.EnableVertexAttribArray(1); //Colors
                    GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToColor);

                    GL.EnableVertexAttribArray(2); //UV
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToUV);

                    GL.EnableVertexAttribArray(3); //Normals
                    GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, Vertex.StrideToEnd, Vertex.StrideToNormal);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, VBOIDs[0]);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIDs[1]);
                    GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, (IntPtr)null);
                    return;
                }
            }

        }

    }
}
