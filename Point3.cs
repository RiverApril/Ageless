using System;
using OpenTK;

namespace Ageless {
    public class Point3 {

        public static readonly Point3 Zero = new Point3(0, 0, 0);

        public int X, Y, Z;

        public Point3(int x, int y, int z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3 makeVector3() {
            return new Vector3(X, Y, Z);
        }
    }
}