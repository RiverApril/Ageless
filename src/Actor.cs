using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ageless {
	public abstract class Actor : Renderable {

		public uint ID;

        protected float movementSpeed = 1.0f / 8.0f;
        protected float maxSlope = 1.0f;

        public Vector3 position;

        public Actor(RenderMaker renderMaker) : base(renderMaker) {

		}

		public abstract void update(Game game);
    }
}
