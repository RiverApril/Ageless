using System;
using System.Collections.Generic;

namespace Ageless {
	public class HeadsUpDisplay {
		Game game;

		ShaderProgram shader;

		List<UiElement> uiElements = new List<UiElement>();
		
		public HeadsUpDisplay(Game game) {
			this.game = game;
		}

		public void onLoad() {
			shader = new ShaderProgram();
		}

		public void render(Game game) {
			
		}

	}
}

