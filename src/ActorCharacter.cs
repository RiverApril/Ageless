﻿using System;
using OpenTK;
using System.Collections.Generic;

namespace Ageless {
	public class ActorCharacter : Actor{

		public static readonly float RENDER_HALF_SIZE = 0.5f;

		public static int UVIndex = 0;

		public ActorCharacter(RenderMaker renderMaker) : base(renderMaker) {

		}

		public override void update(Game game) {
			//position.Y = game.loadedWorld.getFloorAtPosition(position.X, position.Y, position.Z);
		}

		public override void makeRender() {
			if(compileState != COMP_STATUS.MAKING) {
				Console.Error.WriteLine("ERROR!!");
			}
			Console.WriteLine("(Render) Making ActorCharacter");

			vert = new List<Vertex>();
			ind = new List<uint>();
			uint nextI = 0;

			Vector3 normal = new Vector3();
			Vector3 p1 = new Vector3(), p2 = new Vector3(), p3 = new Vector3();
			Vector3 u = new Vector3(), v = new Vector3();

			p1.X = -RENDER_HALF_SIZE; p1.Y = 0; p1.Z = -RENDER_HALF_SIZE;
			p2.X = RENDER_HALF_SIZE; p2.Y = 0; p2.Z = -RENDER_HALF_SIZE;
			p3.X = -RENDER_HALF_SIZE; p3.Y = 0; p3.Z = RENDER_HALF_SIZE;

			u.X = p2.X - p1.X; u.Y = p2.Y - p1.Y; u.Z = p2.Z - p1.Z;
			v.X = p3.X - p1.X; v.Y = p3.Y - p1.Y; v.Z = p3.Z - p1.Z;

			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);

			addVert(ref p1, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 0], ref vert, ref ind, ref nextI);
			addVert(ref p2, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);
			addVert(ref p3, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);



			p1.X = RENDER_HALF_SIZE; p1.Y = 0; p1.Z = RENDER_HALF_SIZE;
			p2.X = -RENDER_HALF_SIZE; p2.Y = 0; p2.Z = RENDER_HALF_SIZE;
			p3.X = RENDER_HALF_SIZE; p3.Y = 0; p3.Z = -RENDER_HALF_SIZE;

			u.X = p2.X - p1.X; u.Y = p2.Y - p1.Y; u.Z = p2.Z - p1.Z;
			v.X = p3.X - p1.X; v.Y = p3.Y - p1.Y; v.Z = p3.Z - p1.Z;

			normal.X = (u.Y * v.Z) - (u.Z * v.Y);
			normal.Y = (u.Z * v.X) - (u.X * v.Z);
			normal.Z = (u.X * v.Y) - (u.Y * v.X);

			addVert(ref p1, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 3], ref vert, ref ind, ref nextI);
			addInd(ref nextI, -2);
			addInd(ref nextI, -3);
			//tryToAdd(ref p2, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 2], ref vert, ref ind, ref nextI);
			//tryToAdd(ref p3, ref normal, ref TextureControl.tex16x16Coords[UVIndex, 1], ref vert, ref ind, ref nextI);


			Console.WriteLine("(Render) Made ActorCharacter");

			compileState = COMP_STATUS.READY_TO_COMPILE;
		}
	}
}
