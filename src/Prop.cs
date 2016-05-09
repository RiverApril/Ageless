using System;
using OpenTK;
using System.Collections.Generic;

namespace Ageless {
    public class Prop {

        public Matrix4 modelMatrix;

        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3();

        public Vector3 frameTopLeft;
        public Vector3 frameBottomRight;

        public string name;

		public Model model;

		public bool solid;

		public Prop(Model model, bool solid) {
			this.model = model;
			this.solid = solid;
		}

        public void setupModelMatrix() {
            modelMatrix = Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
        }

		public void draw(Game game) {
			game.matrixModel = modelMatrix;
			game.setModel();
			
			model.drawRender();
		}


    }
}