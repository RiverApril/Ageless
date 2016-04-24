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

		public static int UVIndex = 0;

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
			Console.WriteLine("Making Player");

			vert = new Dictionary<Vertex, uint>();
			ind = new List<uint>();
			uint nextI = 0;

			Vector4 color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
			Vector3 normal = new Vector3();
			Vector3 p1, p2, p3;
			Vector3 u, v;

			p1 = new Vector3(position.X - RENDER_HALF_SIZE, position.Y, position.Z - RENDER_HALF_SIZE);
			p2 = new Vector3(position.X + RENDER_HALF_SIZE, position.Y, position.Z - RENDER_HALF_SIZE);
			p3 = new Vector3(position.X - RENDER_HALF_SIZE, position.Y, position.Z + RENDER_HALF_SIZE);
			u = p2 - p1;
			v = p3 - p1;
			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);
			tryToAdd(p1, normal, color, TextureControl.tex16x16Coords[UVIndex, 0], ref vert, ref ind, ref nextI);
			tryToAdd(p2, normal, color, TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);
			tryToAdd(p3, normal, color, TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);

			p1 = new Vector3(position.X + RENDER_HALF_SIZE, position.Y, position.Z + RENDER_HALF_SIZE);
			p2 = new Vector3(position.X - RENDER_HALF_SIZE, position.Y, position.Z + RENDER_HALF_SIZE);
			p3 = new Vector3(position.X + RENDER_HALF_SIZE, position.Y, position.Z - RENDER_HALF_SIZE);
			u = p2 - p1;
			v = p3 - p1;
			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);
			tryToAdd(p1, normal, color, TextureControl.tex16x16Coords[UVIndex, 3], ref vert, ref ind, ref nextI);
			tryToAdd(p2, normal, color, TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);
			tryToAdd(p3, normal, color, TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);


			Console.WriteLine("Made Player");

			compileState = COMP_STATUS.READY_TO_COMPILE;
		}

    }
}
