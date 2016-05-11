using System;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Ageless {
    public class Prop {
        
        public Matrix4 modelMatrix;

        public Vector3 position = new Vector3();
        public Vector3 rotation = new Vector3();

		//absolute:
		public bool frameMade = false;
        public Vector3 frameMin;
        public Vector3 frameMax;

		public Model model;

		public bool solid;

        public List<Vector3> transformedPoints = new List<Vector3>();

        public int textureIndex;

        public Prop(Model model, bool solid) {
			this.model = model;
			this.solid = solid;
		}

        public void setupModelMatrix() {
            modelMatrix = Matrix4.CreateRotationY(rotation.Y) * Matrix4.CreateRotationX(rotation.X) * Matrix4.CreateRotationZ(rotation.Z) * Matrix4.CreateTranslation(position);
        }

		public void setupFrame(ref List<Vertex> verts) {
			frameMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			frameMax = new Vector3(-float.MaxValue, -float.MaxValue, -float.MaxValue);

            foreach (Vertex v in verts) {
				Vector3 tp = (Matrix4.CreateTranslation(v.Position) * modelMatrix).ExtractTranslation();
                transformedPoints.Add(tp);

				frameMin.X = Math.Min(frameMin.X, tp.X);
				frameMin.Y = Math.Min(frameMin.Y, tp.Y);
				frameMin.Z = Math.Min(frameMin.Z, tp.Z);
				
				frameMax.X = Math.Max(frameMax.X, tp.X);
				frameMax.Y = Math.Max(frameMax.Y, tp.Y);
				frameMax.Z = Math.Max(frameMax.Z, tp.Z);
				
			}
			
			frameMade = true;
        }

        public virtual void update(Game game) {
            if (!frameMade && model.vert != null) {
                setupFrame(ref model.vert);
            }
        }


        public virtual void draw(Game game) {
            game.matrixModel = modelMatrix;
            game.setModel();
            game.setTexture(textureIndex);
            model.drawRender();
        }


        public virtual void secondaryDraw(Game game) {

        }


    }
}