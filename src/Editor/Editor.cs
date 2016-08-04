using System;
using OpenTK;
using OpenTK.Input;

namespace Ageless {
	public class Editor {
	
		public bool active;

		Game game;

		public Vector3 focusPosition;

        public Prop selectedProp;


        public Editor(Game game) {
			this.game = game;
		}

        public void onInputDown(KeyboardKeyEventArgs ke, MouseButtonEventArgs me) {

            if (game.settings.editorBindFocusOnPlayer.test(ke, me)) {
                focusPosition = game.player.position;

            } else if (game.settings.editorBindSave.test(ke, me)) {
                Data.saveMap(game.loadedMap);

            } else if (game.settings.editorBindLoad.test(ke, me)) {
                game.loadedMap.unloadAllChunks();
                Data.loadMap(game.loadedMap);

            }

            if (selectedProp != null) {
                if (ke != null) {
                    bool changed = false;
                    Vector3 propMove = new Vector3();
                    if (game.settings.editorBindPropMoveForward.test(ke, me)) {
                        propMove.Z += -1;
                    }
                    if (game.settings.editorBindPropMoveBackward.test(ke, me)) {
                        propMove.Z += 1;
                    }
                    if (game.settings.editorBindPropMoveLeft.test(ke, me)) {
                        propMove.X += -1;
                    }
                    if (game.settings.editorBindPropMoveRight.test(ke, me)) {
                        propMove.X += 1;
                    }
                    if (game.settings.editorBindPropMoveUp.test(ke, me)) {
                        propMove.Y += 1;
                    }
                    if (game.settings.editorBindPropMoveDown.test(ke, me)) {
                        propMove.Y += -1;
                    }
                    if (ke.Shift) {
                        propMove *= 0.125f;
                    }
                    if (ke.Alt) {
                        selectedProp.rotation += propMove * (float)(Math.PI / 90f);
                        changed = true;
                    } else {
                        selectedProp.position += propMove;
                        changed = true;
                    }
                    if (game.settings.editorBindPropReset.test(ke, me)) {
                        selectedProp.position = Vector3.Zero;
                        selectedProp.rotation = Vector3.Zero;
                        changed = true;
                    }
                    if (game.settings.editorBindPropMoveToMouse.test(ke, me)) {
                        game.findTerrainWithRay(ref game.lookOrigin, ref game.lookDirection, out selectedProp.position);
                        changed = true;
                    }
                    if (changed) {
                        selectedProp.frameMade = false;
                        selectedProp.setupModelMatrix();
                    }
                }
            }

            if (game.settings.editorBindSelectProp.test(ke, me)) {
                Vector3 hit;
                game.findPropWithRay(ref game.lookOrigin, ref game.lookDirection, out selectedProp, out hit);
            }

        }

        public void update() {
            if (selectedProp != null) {
                selectedProp.highlightTimeout = 2;
            }
        }
    }
}

