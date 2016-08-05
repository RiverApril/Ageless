using System;
using System.Collections.Generic;
using OpenTK;

namespace Ageless {
	public class Bone {

		public Bone sourceBone = null;

		public Matrix4 matrix;
		public Model model;
		public int textureIndex;


		public Bone(Bone sourceBone, Model model, int textureIndex) {
			this.sourceBone = sourceBone;
			this.model = model;
			this.textureIndex = textureIndex;
		}

		public Matrix4 getTotalMatrix(){
			Bone up = sourceBone;
			Matrix4 mat = matrix;
			while(up != null){
				mat = mat * up.matrix;
				up = up.sourceBone;
			}
			return mat;
		}
	}
}

