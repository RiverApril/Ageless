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

		Bone torso;
		Bone legUpperLeft;
		Bone legUpperRight;
		Bone legLowerLeft;
		Bone legLowerRight;
		Bone armUpperLeft;
		Bone armUpperRight;
		Bone armLowerLeft;
		Bone armLowerRight;

		int animTick = 0;

		const int ANIM_STAND = 0;
		const int ANIM_RUN = 1;

		int animation = ANIM_STAND;

		public ActorPlayer() : base() {
			torso = new Bone(null, ModelControl.getModel("limb"), 0);

			legUpperLeft = new Bone(torso, ModelControl.getModel("limb"), 0);
			legLowerLeft = new Bone(legUpperLeft, ModelControl.getModel("limb"), 0);
			legUpperRight = new Bone(torso, ModelControl.getModel("limb"), 0);
			legLowerRight = new Bone(legUpperRight, ModelControl.getModel("limb"), 0);

			armUpperLeft = new Bone(torso, ModelControl.getModel("limb"), 0);
			armLowerLeft = new Bone(armUpperLeft, ModelControl.getModel("limb"), 0);
			armUpperRight = new Bone(torso, ModelControl.getModel("limb"), 0);
			armLowerRight = new Bone(armUpperRight, ModelControl.getModel("limb"), 0);

			bones.Add(torso);
			bones.Add(legUpperLeft);
			bones.Add(legLowerLeft);
			bones.Add(legUpperRight);
			bones.Add(legLowerRight);
			bones.Add(armUpperLeft);
			bones.Add(armLowerLeft);
			bones.Add(armUpperRight);
			bones.Add(armLowerRight);
		}

		//Torso
		public float animTorsoRZ(){
			if(animation == ANIM_RUN){
				return (float)(Math.PI * -0.05);
			}else{
				return 0;
			}
		}

		//Legs
		public float animUpperLegRZ(bool left){
			if(animation == ANIM_RUN) {
				return (float)(Math.PI * (left ? 1 : -1) * Math.Cos(animTick / 7.0f) * -0.2f + Math.PI);
			}else{
				return (float)(Math.PI);
			}
		}
		public float animLowerLegRZ(bool left){
			if(animation == ANIM_RUN){
				return (float)(Math.PI * (left ? 1 : -1) * Math.Sin(animTick / 7.0) * -0.1 + Math.PI * -0.2);
			}else{
				return 0;
			}
		}

		//Arms
		public float animUpperArmRZ(bool left){
			if(animation == ANIM_RUN) {
				return (float)(Math.PI * (left ? 1 : -1) * Math.Sin(animTick / 7.0) * -0.2 + Math.PI);
			}else{
				return (float)(Math.PI);
			}
		}
		public float animLowerArmRZ(bool left){
			if(animation == ANIM_RUN){
				return (float)(Math.PI * (left ? 1 : -1) * Math.Cos(animTick / 7.0) * -0.1 + Math.PI * 0.2);
			}else{
				return 0;
			}
		}


        public override void update(Game game) {

			animTick += 1;

			torso.matrix         = Matrix4.CreateRotationZ(animTorsoRZ())         * Matrix4.CreateTranslation(0, 3, 0);

			legUpperLeft.matrix  = Matrix4.CreateRotationZ(animUpperLegRZ(true))  * Matrix4.CreateTranslation(0, 0   , 0.5f);
			legLowerLeft.matrix  = Matrix4.CreateRotationZ(animLowerLegRZ(true))  * Matrix4.CreateTranslation(0, 1.5f, 0);
			legUpperRight.matrix = Matrix4.CreateRotationZ(animUpperLegRZ(false)) * Matrix4.CreateTranslation(0, 0   ,-0.5f);
			legLowerRight.matrix = Matrix4.CreateRotationZ(animLowerLegRZ(false)) * Matrix4.CreateTranslation(0, 1.5f, 0);

			armUpperLeft.matrix  = Matrix4.CreateRotationZ(animUpperArmRZ(true))  * Matrix4.CreateTranslation(0, 1.5f, 0.75f);
			armLowerLeft.matrix  = Matrix4.CreateRotationZ(animLowerArmRZ(true))  * Matrix4.CreateTranslation(0, 1.5f, 0);
			armUpperRight.matrix = Matrix4.CreateRotationZ(animUpperArmRZ(false)) * Matrix4.CreateTranslation(0, 1.5f,-0.75f);
			armLowerRight.matrix = Matrix4.CreateRotationZ(animLowerArmRZ(false)) * Matrix4.CreateTranslation(0, 1.5f, 0);

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
				animation = ANIM_RUN;
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
				animation = ANIM_STAND;
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
