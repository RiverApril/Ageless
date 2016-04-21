using System;
using OpenTK;

namespace Ageless {
    public class Point2 {

        public static readonly Point2 Zero = new Point2(0, 0);

        public int X, Y;

        public Point2(int x, int y) {
            this.X = x;
            this.Y = y;
        }

        public override int GetHashCode() {
            return 31 * X + Y;
        }

        public override bool Equals(object obj) {
            return Equals(obj as Point2);
        }

        public bool Equals(Point2 obj) {
            return obj != null && obj.X == this.X && obj.Y == this.Y;
        }
    }
}