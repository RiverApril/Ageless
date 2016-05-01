using System;
using OpenTK;
using System.Collections.Generic;

namespace Ageless {
    public class Prop : Renderable{

        public Matrix4 modelMatrix;

        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3();

        public Vector3 frameTopLeft;
        public Vector3 frameBottomRight;

        public string name;

        public void setupModelMatrix() {
            modelMatrix = Matrix4.CreateTranslation(position) * Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationZ(rotation.Z);
        }

        public override void makeRender() {
            Console.WriteLine("(Render) Making Prop {0}", name);

            vert = new List<Vertex>();
            ind = new List<uint>();
            uint nextI = 0;

            Tile tile;
            Vector3 normal = new Vector3();
            Vector3 p1 = new Vector3(), p2 = new Vector3(), p3 = new Vector3();
            Vector3 u = new Vector3(), v = new Vector3();



            Console.WriteLine("(Render) Made Prop {0}", name);
        }


    }
}