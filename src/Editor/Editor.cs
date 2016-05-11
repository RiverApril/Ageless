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

		public void onKeyDown(object sender, KeyboardKeyEventArgs e) {
			switch (e.Key) {
                case Key.P: {
					focusPosition = game.player.position;
                    break;
                }
            }
		}
	}
}

