using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
    public class ActorPlayer : Actor {

		public static readonly float RENDER_HALF_SIZE = 0.5f;

		public static int UVIndex = 1;

		public ActorPlayer(RenderMaker renderMaker) : base(renderMaker) {

        }

        public override void update(Game game) {

            Vector2 diff = new Vector2();

            if (game.keyboard.IsKeyDown(Key.Left)) {
                diff.X -= 1;
            }
            if (game.keyboard.IsKeyDown(Key.Right)) {
                diff.X += 1;
            }
            if (game.keyboard.IsKeyDown(Key.Up)) {
                diff.Y -= 1;
            }
            if (game.keyboard.IsKeyDown(Key.Down)) {
                diff.Y += 1;
            }

            if (diff.LengthSquared != 0) {
                diff.Normalize();
                diff *= movementSpeed;

                float maxSlope = 1.0f;

                float ch = game.loadedWorld.getFloorAtPosition(position.X, position.Y, position.Z); ;
                float nh = game.loadedWorld.getFloorAtPosition(position.X + diff.X, position.Y + maxSlope, position.Z + diff.Y);

                if ((nh - ch) / diff.Length <= maxSlope) {
                    position.X += diff.X;
                    position.Z += diff.Y;
                    position.Y = nh;
                }

            }

			//Console.WriteLine("Player: {0}, {1}, {2}", position.X, position.Y, position.Z);

        }

		public override void makeRender() {
			if(compileState != COMP_STATUS.MAKING){
				Console.Error.WriteLine("ERROR!!");
			}
			Console.WriteLine("(Render) Making Player");

			vert = new List<Vertex>();
			ind = new List<uint>();
			uint nextI = 0;

			Vector3 normal = new Vector3();
			Vector3 p1 = new Vector3(), p2 = new Vector3(), p3 = new Vector3();
			Vector3 u = new Vector3(), v = new Vector3();

			p1.X = - RENDER_HALF_SIZE;   p1.Y = 0;   p1.Z = - RENDER_HALF_SIZE;
			p2.X =   RENDER_HALF_SIZE;   p2.Y = 0;   p2.Z = - RENDER_HALF_SIZE;
			p3.X = - RENDER_HALF_SIZE;   p3.Y = 0;   p3.Z =   RENDER_HALF_SIZE;

			u.X = p2.X - p1.X;   u.Y = p2.Y - p1.Y;   u.Z = p2.Z - p1.Z;
			v.X = p3.X - p1.X;   v.Y = p3.Y - p1.Y;   v.Z = p3.Z - p1.Z;

			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);

			addVert(ref p1, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 0], ref vert, ref ind, ref nextI);
			addVert(ref p2, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);
			addVert(ref p3, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);



			p1.X =   RENDER_HALF_SIZE;   p1.Y = 0;   p1.Z =   RENDER_HALF_SIZE;
			p2.X = - RENDER_HALF_SIZE;   p2.Y = 0;   p2.Z =   RENDER_HALF_SIZE;
			p3.X =   RENDER_HALF_SIZE;   p3.Y = 0;   p3.Z = - RENDER_HALF_SIZE;

			u.X = p2.X - p1.X;   u.Y = p2.Y - p1.Y;   u.Z = p2.Z - p1.Z;
			v.X = p3.X - p1.X;   v.Y = p3.Y - p1.Y;   v.Z = p3.Z - p1.Z;

			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);

			addVert(ref p1, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 3], ref vert, ref ind, ref nextI);
			addInd(ref nextI, -2);
			addInd(ref nextI, -3);
			//tryToAdd(ref p2, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);
			//tryToAdd(ref p3, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);


			Console.WriteLine("(Render) Made Player");

			compileState = COMP_STATUS.READY_TO_COMPILE;
		}

    }
}
