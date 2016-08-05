using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
	public abstract class Actor {

		public static readonly float GRAVITY = 0.1f;

		public uint ID;

        protected float movementSpeed = 1.0f / 2.0f;
        public float maxHeightChange = 2.0f;
        protected float radius = 1.0f;

        public Vector3 position;


		public List<Bone> bones = new List<Bone>();


        public Actor() {

		}

		public abstract void update(Game game);

        public void draw(Game game) {
			Matrix4 pos = Matrix4.CreateTranslation(position);
			foreach(Bone bone in bones){
				game.matrixModel = bone.getTotalMatrix() * pos;
	            game.setModel();
				game.setTextureIndexOffset(bone.textureIndex);
				bone.model.drawRender();
			}
		}
    }
}
