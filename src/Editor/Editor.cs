using System;
using OpenTK;
using OpenTK.Input;

namespace Ageless {
	public class Editor {
	
		public bool active;

		Game game;

		public Vector3 focusPosition;


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

        }
    }
}

