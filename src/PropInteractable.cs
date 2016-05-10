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

        public bool canHighlight = true;
        public int highlightTimeout = 0;

        public PropInteractable(Model model, bool solid) : base(model, solid) {

        }

        public override void draw(Game game) {
            base.draw(game);
        }

        public override void secondaryDraw(Game game) {
            if (highlightTimeout > 0) {

                GL.Disable(EnableCap.DepthTest);
                game.setColor(game.highlightColor, true);
                game.additiveBlending();

                game.matrixModel = modelMatrix;
                game.setModel();
                model.drawRender();

                GL.Enable(EnableCap.DepthTest);
                game.resetColor();
                game.resetBlending();

                if (game.rayWasUpdated) {
                    highlightTimeout--;
                }
            }
        }
    }
}
