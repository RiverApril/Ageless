using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Ageless {
	class Data {
	
		public static void loadMap(Map map) {
			//Nothing to load yet, will have a map setting file here later
		}

		public static void loadChunk(Chunk chunk) {

            Console.WriteLine("Loading Chunk: {0}, {1}", chunk.Location.X, chunk.Location.Y);

            string path;
			string letters = "abcdefghijklmnopqrstuvwxyz";
			bool loadedLetter = true;
			for (int i = 0; i < 26 && loadedLetter; i++) {
				loadedLetter = false;
				for (int fc = 0; fc < 2; fc++) {
					for (int st = 0; st < 2; st++) {
						path = Game.dirMaps + chunk.map.name + "/htmp.";
						path += chunk.Location.X.ToString();
						path += ".";
						path += chunk.Location.Y.ToString();
						path += ".";
						path += fc == 0 ? "f" : "c"; //floor, ceiling
						path += ".";
						path += st == 0 ? "s" : "d"; //solid, decorative
						path += ".";
						path += letters[i];
						path += ".png";

						try {


							if (File.Exists(path)) {

								Console.WriteLine("Try to load Heightmap: {0}", path);



								using (Image img = Image.FromFile(path)) {

									using (Bitmap bmp = new Bitmap(img)) {

										if (bmp.Width != Chunk.CHUNK_SIZE_X + 2 || bmp.Height != Chunk.CHUNK_SIZE_Z + 2) {
											throw new FormatException(String.Format("Image size not equal to {0}x{1}, is instead {2}x{3}", Chunk.CHUNK_SIZE_X + 2, Chunk.CHUNK_SIZE_Z + 2, bmp.Width, bmp.Height));
										}

										HeightMap htmp = new HeightMap(letters[i]);

										htmp.isFloor = fc == 0;
										htmp.isSolid = st == 0;

										htmp.min = float.MaxValue;
										htmp.max = float.MinValue;

										for (int x = 0; x < Chunk.CHUNK_SIZE_X + 2; x++) {
											for (int z = 0; z < Chunk.CHUNK_SIZE_Z + 2; z++) {
												Color c = bmp.GetPixel(x, z);
												if (x < Chunk.CHUNK_SIZE_X && z < Chunk.CHUNK_SIZE_Z) {
													htmp.tiles[x, z] = c.R;
												}
												htmp.heights[x, z] = c.B / chunk.resolution;
												htmp.min = Math.Min(htmp.heights[x, z], htmp.min);
												htmp.max = Math.Max(htmp.heights[x, z], htmp.max);
											}
										}

										chunk.terrain.Add(htmp);
										loadedLetter = true;
										Console.WriteLine("Loaded Heightmap: {0}", path);
									}
								}
							}


						} catch (OutOfMemoryException) {
							Console.WriteLine("Out of memory");
							return;
						}
					}
				}
			}

			path = Game.dirMaps + chunk.map.name + "/props.";
			path += chunk.Location.X.ToString();
			path += ".";
			path += chunk.Location.Y.ToString();
			path += ".txt";

			if (File.Exists(path)) {

				Console.WriteLine("Loading Prop File: {0}", path);

				Dictionary<string, string> modelTex = new Dictionary<string, string>();

				string[] lines = File.ReadAllLines(path);
				foreach (string line in lines) {
					if (line.StartsWith("set ", StringComparison.Ordinal)) {
						string[] split = line.Split(' ');

						if (split[1].Equals("tex") && split.Length == 4) {
							modelTex[split[2]] = split[3];
						}

					} else if (line.StartsWith("p", StringComparison.Ordinal)) {
						string[] split = line.Split(' ');

						string name = split[1];
						float x = float.Parse(split[2]);
						float z = float.Parse(split[4]);
						float y = 0;
						bool yset = false;
						foreach (HeightMap htmp in chunk.terrain) {
							if (htmp.letter == split[3][0]) {

								htmp.getHeightAtPosition(new Vector2d(x, z), out y);

								if (split[3].Substring(1).Length > 0) {
									y += float.Parse(split[3].Substring(1));
								}

								yset = true;
								break;
							}
						}
						if (!yset) {
							y = float.Parse(split[3]);
						}

						float xr = MathHelper.DegreesToRadians(float.Parse(split[5]));
						float yr = MathHelper.DegreesToRadians(float.Parse(split[6]));
						float zr = MathHelper.DegreesToRadians(float.Parse(split[7]));

						Prop p;
						if (split[0].Contains("i")) {
							p = new PropInteractable(ModelControl.getModel(name), split[0].Contains("s"));
						} else {
							p = new Prop(ModelControl.getModel(name), split[0].Contains("s"));
						}
						p.position = new Vector3(x + (Chunk.CHUNK_SIZE_X * chunk.Location.X), y, z + (Chunk.CHUNK_SIZE_Z * chunk.Location.Y));
						p.rotation = new Vector3(xr, yr, zr);
						p.setupModelMatrix();
						p.textureIndex = TextureControl.arrayProps.names.IndexOf(modelTex[p.model.name]);
						//p.setupFrame(); after model loads
						chunk.props.Add(p);

						Console.WriteLine("New prop at: {0}", p.position);

					}
				}

				Console.WriteLine("Loaded Prop File: {0}", path);
            }

            Console.WriteLine("Loaded Chunk: {0}, {1}", chunk.Location.X, chunk.Location.Y);

        }

		public static void saveMap(Map map) {
            foreach (Chunk chunk in map.loadedChunks.Values) {
                saveChunk(chunk);
            }
		}

        public static void saveChunk(Chunk chunk) {

            Console.WriteLine("Saving Chunk: {0}, {1}", chunk.Location.X, chunk.Location.Y);

            string path;
			foreach (HeightMap htmp in chunk.terrain) {
				path = Game.dirMaps + chunk.map.name + "/htmp.";
				path += chunk.Location.X.ToString();
				path += ".";
				path += chunk.Location.Y.ToString();
				path += ".";
				path += htmp.isFloor ? "f" : "c"; //floor, ceiling
				path += ".";
				path += htmp.isSolid ? "s" : "d"; //solid, decorative
				path += ".";
				path += htmp.letter;
				path += ".png";

				Bitmap bmp = new Bitmap(Chunk.CHUNK_SIZE_X + 2, Chunk.CHUNK_SIZE_Z + 2);

				for (int i = 0; i < Chunk.CHUNK_SIZE_X + 2; i++) {
					for (int j = 0; j < Chunk.CHUNK_SIZE_Z + 2; j++) {
						int t = 0;
						if (i < Chunk.CHUNK_SIZE_X && j < Chunk.CHUNK_SIZE_Z) {
							t = htmp.tiles[i, j];
						}
						bmp.SetPixel(i, j, Color.FromArgb(t, 0, (int)(htmp.heights[i, j] * chunk.resolution)));
					}
				}

                bmp.Save(path);
				
			}

            if (chunk.props.Count > 0) {

                path = Game.dirMaps + chunk.map.name + "/props.";
                path += chunk.Location.X.ToString();
                path += ".";
                path += chunk.Location.Y.ToString();
                path += ".txt";

                string text = string.Format("# Props for chunk {0}, {1}{2}", chunk.Location.X, chunk.Location.Y, Environment.NewLine);

                Dictionary<string, string> modelTex = new Dictionary<string, string>();

                foreach (Prop prop in chunk.props) {
                    if (!modelTex.ContainsKey(prop.model.name) || modelTex[prop.model.name] != TextureControl.arrayProps.names[prop.textureIndex]) {
                        modelTex[prop.model.name] = TextureControl.arrayProps.names[prop.textureIndex];
                        text += string.Format("{2}set tex {0} {1}{2}{2}", prop.model.name, modelTex[prop.model.name], Environment.NewLine);
                    }
                    text += "p";
                    if (prop.solid) {
                        text += "s";
                    }
                    if (prop is PropInteractable) {
                        text += "i";
                    }
                    float y = prop.position.Y;
                    string ys = string.Format("{0}", y);
                    foreach (HeightMap htmp in chunk.terrain) {
                        float h;
                        if (htmp.getHeightAtPosition(new Vector2d(prop.position.X, prop.position.Z), out h)) {
                            if (Math.Abs(h) <= Math.Abs(y)) {
                                y = prop.position.Y - h;
                                if (y == 0) {
                                    ys = string.Format("{0}", htmp.letter);
                                } else {
                                    ys = string.Format("{0}{1}{2}", htmp.letter, y < 0 ? "" : "+", y);
                                }
                            }
                        }
                    }
                    text += string.Format(" {0} {1} {2} {3} {4} {5} {6}{7}", prop.model.name, prop.position.X, ys, prop.position.Z, MathHelper.RadiansToDegrees(prop.rotation.X), MathHelper.RadiansToDegrees(prop.rotation.Y), MathHelper.RadiansToDegrees(prop.rotation.Z), Environment.NewLine);

                }

                File.WriteAllText(path, text);

            }

            Console.WriteLine("Saved Chunk: {0}, {1}", chunk.Location.X, chunk.Location.Y);

        }
		
	}
}