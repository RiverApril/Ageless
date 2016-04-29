using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
	public class ActorPlayer : ActorCharacter {

		private float volocityY = 0;

		public Vector3 target = new Vector3();

		public ActorPlayer(RenderMaker renderMaker) : base(renderMaker) {
			
        }

        public override void update(Game game) {

            Vector2 diff = new Vector2();

			diff = target.Xz - position.Xz;

            /*if (game.keyboard.IsKeyDown(Key.Left)) {
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
            }*/


			float ch = game.loadedWorld.getFloorAtPosition(position.X, position.Y, position.Z);
			float nh;

			if(diff.Length > movementSpeed) {
				/*double angle = game.camAngle.Phi + Math.Atan2(diff.Y, diff.X);

				diff.X = (float)(Math.Cos(angle) * movementSpeed);
				diff.Y = (float)(Math.Sin(angle) * movementSpeed);*/

				diff.Normalize();
				diff *= movementSpeed;

				nh = game.loadedWorld.getFloorAtPosition(position.X + diff.X, position.Y + maxSlope, position.Z + diff.Y);

				if((nh - ch) / diff.Length <= maxSlope) {
					position.X += diff.X;
					position.Z += diff.Y;
				}
			} else {
				nh = ch;
			}

			if(position.Y > nh) {
				volocityY -= GRAVITY;
				if(position.Y + volocityY <= nh) {
					position.Y = nh;
					volocityY = 0;
				} else {
					position.Y += volocityY;
				}
			} else {
				position.Y = nh;
				volocityY = 0;
			}



			//Console.WriteLine("Player: {0}, {1}, {2}", position.X, position.Y, position.Z);

        }

    }
}
