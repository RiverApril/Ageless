using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;


namespace Ageless {
    public struct Vertex {
        public Vector3 Position;
        public Vector4 Color;
        public Vector2 UV;
        public Vector3 Normal;

        public static readonly int StrideToPosition = 0;
        public static readonly int StrideToColor = (3) * sizeof(float);
        public static readonly int StrideToUV = (3 + 4) * sizeof(float);
        public static readonly int StrideToNormal = (3 + 4 + 2) * sizeof(float);
        public static readonly int StrideToEnd = (3 + 4 + 2 + 3) * sizeof(float);

        public Vertex(Vector3 p, Vector4 c, Vector2 u, Vector3 n) {
            Position = p;
            Color = c;
            UV = u;
            Normal = n;
        }

        public bool Equals(Vertex obj) {
            return obj.Position == Position && obj.Color == Color && obj.UV == UV && obj.Normal == Normal;
        }


    }
}
