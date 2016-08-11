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

		public ActorPlayer() : base() {

            skeleton = new SkeletonHuman();

        }

        public override void update(Game game) {

            skeleton.updateMatrices();

            skeleton.animTick += 1;

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

			if(diff.LengthSquared > 1) {
                skeleton.animation = SkeletonHuman.ANIM_RUN;
				float next = -(float)Math.Atan2(diff.Y, diff.X);
				if(this.rotation.Y > Math.PI * 2) {
					this.rotation.Y -= (float)Math.PI * 2;
				}
				if(this.rotation.Y < 0) {
					this.rotation.Y += (float)Math.PI * 2;
				}
				if(Math.Abs(this.rotation.Y - next) > (float)Math.PI) {
					next += (float)Math.PI * 2;
				}
				this.rotation.Y += (next - this.rotation.Y) / 8;
			} else {
                skeleton.animation = SkeletonHuman.ANIM_STAND;
			}


			float ch = game.loadedMap.getFloorAtPosition(position.X, position.Y, position.Z);
			if (ch >= 0) { // Wait until chunks are loaded
				float nh;

				if (diff.LengthSquared > 1) {
                    /*double angle = game.camAngle.Phi + Math.Atan2(diff.Y, diff.X);

					diff.X = (float)(Math.Cos(angle) * movementSpeed);
					diff.Y = (float)(Math.Sin(angle) * movementSpeed);*/
                    if (diff.Length > movementSpeed) {
                        diff.Normalize();
                    }
                    Vector2 boundry = diff * radius;
                    diff *= movementSpeed;

                    nh = game.loadedMap.getFloorAtPosition(position.X + diff.X, position.Y + maxHeightChange, position.Z + diff.Y);

					Vector3 direction = new Vector3(boundry.X, nh - position.Y, boundry.Y);
                    float far = direction.Length;
					direction.Normalize();
                    Vector3 hp = new Vector3(position.X, position.Y + maxHeightChange, position.Z);
                    Prop prop; Vector3 hit;

                    bool isHit = game.findPropWithRay(ref hp, ref direction, out prop, out hit, far);

                    if (!isHit) {
						position.X += diff.X;
						position.Z += diff.Y;
                    }

				} else {
					nh = ch;
				}

				if (position.Y > nh) {
					volocityY -= GRAVITY;
					if (position.Y + volocityY <= nh) {
						position.Y = nh;
						volocityY = 0;
					} else {
						position.Y += volocityY;
					}
				} else {
					position.Y = nh;
					volocityY = 0;
				}

			}



			//Console.WriteLine("Player: {0}, {1}, {2}", position.X, position.Y, position.Z);

        }

    }
}
