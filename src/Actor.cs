using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
	public abstract class Actor : Renderable {

		public static readonly float GRAVITY = 0.1f;

		public uint ID;

        protected float movementSpeed = 1.0f / 2.0f;
        public float maxSlope = 1.0f;

        public Vector3 position;

        public Actor() : base() {

		}

		public abstract void update(Game game);

        public void draw(Game game) {
            game.matrixModel = Matrix4.CreateTranslation(position);
            game.setModel();
            game.setTextureIndex(0);

            drawRender();
        }
    }
}
