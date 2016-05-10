using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace Ageless {

	public class Model : Renderable{

		public string name;
		public string path;

	
		public Model(string name, string path) {
			this.name = name;
			this.path = path;
			compileState = COMP_STATUS.NEEDS_TO_BE_MADE;
			keepVerts = true;
		}

		public override void makeRender() {
		
			Console.WriteLine("(Render) Making Model: {0}", name);

			vert = new List<Vertex>();
			ind = new List<uint>();
			uint nextI = 0;

			Vector2[] defaultUV = {new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1)};

			if (File.Exists(path)) {
				string[] lines = File.ReadAllLines(path);
				
				List<Vector3> vertexList = new List<Vector3>();
				List<Vector2> textureCoordList = new List<Vector2>();
				List<Vector3> normalList = new List<Vector3>();
				
				foreach (string line in lines) {
					if (line.StartsWith("v ", StringComparison.Ordinal)) { //vertex
						string[] split = line.Split(' ');

						float x = float.Parse(split[1]);
						float y = float.Parse(split[2]);
						float z = float.Parse(split[3]);

						vertexList.Add(new Vector3(x, y, z));
						
					} else if (line.StartsWith("vt ", StringComparison.Ordinal)) { //texture coords
						string[] split = line.Split(' ');

						float x = float.Parse(split[1]);
						float y = float.Parse(split[2]);

						textureCoordList.Add(new Vector2(x, y));

					} else if (line.StartsWith("vn ", StringComparison.Ordinal)) { //normals
						string[] split = line.Split(' ');

						float x = float.Parse(split[1]);
						float y = float.Parse(split[2]);
						float z = float.Parse(split[3]);

						normalList.Add(new Vector3(x, y, z).Normalized());

					} else if (line.StartsWith("f ", StringComparison.Ordinal)) { //faces
						string[] split1 = line.Split(' ');

						for (int i = 1; i <= 3;i++) {//1,2,3
							string[] split2 = split1[i].Split('/');

							int v = int.Parse(split2[0])-1;
							int t = split2[1].Length == 0 ? -1 : (int.Parse(split2[1])-1);
							int n = int.Parse(split2[2])-1;

							vert.Add(new Vertex(vertexList[v], t==-1?defaultUV[i-1]:textureCoordList[t], -normalList[n]));
							ind.Add(nextI);
							nextI++;
							
						}

					}
				}
			
				Console.WriteLine("(Render) Made Model: {0}", name);
				return;
			}
			
			Console.WriteLine("(Render) Failed to make Model: {0}", name);
		}
	}
}

