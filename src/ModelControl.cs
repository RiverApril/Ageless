using System;
using System.Collections.Generic;

namespace Ageless {
	class ModelControl {

		private static Dictionary<string, Model> models = new Dictionary<string, Model>();

		private static Model addModel(string name) {
			Model m = new Model(name, Game.dirModels+name+".obj");
			models.Add(name, m);
			return m;
		}

		public static Model getModel(string name) {
			if (models.ContainsKey(name)) {
				return models[name];
			} else {
				return addModel(name);
			}
		}
	}
}