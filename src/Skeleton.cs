using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Ageless {
    public abstract class Skeleton {


        public List<Bone> bones = new List<Bone>();

        public abstract void updateMatrices();
        
        public int animTick = 0;
        public int animation = 0;

        public void draw(Game game, Matrix4 pos) {
            foreach (Bone bone in bones) {
                game.matrixModel = bone.getTotalMatrix() * pos;
                game.setModel();
                game.setTextureIndexOffset(bone.textureIndex);
                bone.model.drawRender();
            }
        }

    }
}
