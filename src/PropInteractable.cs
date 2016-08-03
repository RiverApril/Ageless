using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Ageless {
    class PropInteractable : Prop {

        public PropInteractable(Model model, bool solid) : base(model, solid) {

        }

        public override void draw(Game game) {
            base.draw(game);
        }

        public override void secondaryDraw(Game game) {
            base.secondaryDraw(game);
        }
    }
}
