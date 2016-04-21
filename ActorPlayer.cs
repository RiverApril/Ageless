using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
    public class ActorPlayer : Actor {

        public void update(Game game) {

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

        }


        public void render(Game game) {



        }

    }
}
