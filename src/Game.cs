using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Threading;

namespace Ageless {

    public class Game {

        public static readonly float FLOAT_EPSILON = 0.0001f;

        public static readonly string dir = "../../";
        public static readonly string dirMaps = dir + "maps/";
        public static readonly string dirTextures = dir + "textures/";
        public static readonly string dirShaders = dir + "shaders/";
        public static readonly string dirModels = dir + "models/";

        public const float NEAR = 0.1f;
        public const float FAR = 1024.0f;

        public static bool exiting = false;

        public GameWindow gameWindow;

        double FPS, UPS;

        public ShaderProgram shader;

        public Matrix4 matrixProjection;
        public Matrix4 matrixCamera;
        public Matrix4 matrixModel;

        public Matrix4 matrixView;
        public Matrix3 matrixNormal;

        public Map loadedMap;

        public Random rand = new Random();

        public Vector3 camPos = new Vector3();
        public Angle2 camAngle = new Angle2();
        public Vector3 focusPos = new Vector3();
        public float focusDistance = 20.0f;

        public Settings settings;

        public ActorPlayer player;

        Vector3 lightPosition = new Vector3(0, 0, 0);
        Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);

        public KeyboardState keyboard;

		private Point2 mousePosition = new Point2(-1, -1);

        private bool mouseLeftClicked = false;
        private bool mouseRightClicked = false;

        public bool rayShouldUpdate = false;
        public bool rayWasUpdated = false;

        private Thread heavyThread;

		private Editor editor;

        Vector3 lookOrigin, lookDirection;

        public Vector4 highlightColor = new Vector4(0, 1, 0, .5f);

        public Game() {

			editor = new Editor(this);

			heavyThread = new Thread(heavy);

            settings = new Settings(dir + "settings.txt");
            settings.load();

            gameWindow = new GameWindow(settings.windowWidth, settings.windowHeight, GraphicsMode.Default, "Ageless",
        GameWindowFlags.Default, DisplayDevice.Default, 3, 2, GraphicsContextFlags.ForwardCompatible);

            gameWindow.Load += onLoad;
            gameWindow.Unload += onUnload;
            gameWindow.Resize += onResize;
            gameWindow.UpdateFrame += onUpdateFrame;
            gameWindow.RenderFrame += onRenderFrame;
            gameWindow.KeyDown += onKeyDown;
            gameWindow.KeyUp += onKeyUp;
            gameWindow.MouseDown += onMouseDown;
            gameWindow.MouseMove += onMouseMove;

            gameWindow.Run(60.0);
        }

        void onLoad(object sender, EventArgs e) {
            Console.Out.WriteLine("onLoad");

            gameWindow.VSync = VSyncMode.On;

            Console.WriteLine("OpenGL version: {0}", GL.GetString(StringName.Version));
            //Console.WriteLine("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
            string glslVersionS = GL.GetString(StringName.ShadingLanguageVersion);
            if (glslVersionS.Contains(" ")) {
                glslVersionS = glslVersionS.Substring(0, glslVersionS.IndexOf(" ", StringComparison.CurrentCulture));
            }
            int glslVersion = int.Parse(glslVersionS.Replace(".", ""));

            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(2, 0);
            if (version < target) {
                throw new NotSupportedException(String.Format("OpenGL {0} is required. Current version is {1}.", target, version));
            }

            TryGL.Call(() => GL.ClearColor(0.2f, 0.0f, 0.2f, 1.0f));

            TryGL.Call(() => GL.Enable(EnableCap.DepthTest));
            
            TryGL.Call(() => GL.Enable(EnableCap.CullFace));
            TryGL.Call(() => GL.CullFace(CullFaceMode.Back));

            TryGL.Call(() => GL.Enable(EnableCap.Blend));
            resetBlending();
            TryGL.Call(() => GL.BlendEquation(BlendEquationMode.FuncAdd));
            
            int VAO = GL.GenVertexArray();
			GL.BindVertexArray(VAO);
	

            shader = new ShaderProgram();
            Console.WriteLine("Current GLSL version: {0}", glslVersion);

            while (!File.Exists(dirShaders + "vertex." + glslVersion + ".glsl")) {
                glslVersion--;
                if (glslVersion <= 0) {
                    Console.WriteLine("Shader with required version not found.");
                    throw new NotSupportedException(String.Format("Please create shader files with version {0} or lower.", GL.GetString(StringName.ShadingLanguageVersion)));
                }
            }
            Console.WriteLine("Using shader with GLSL version: {0}", glslVersion);
            shader.LoadAndCompileProrgam(dirShaders + "vertex." + glslVersion + ".glsl", dirShaders + "fragment." + glslVersion + ".glsl");

            TextureControl.loadTextures();

            heavyThread.Start();

            loadedMap = new Map(this, "debugMap");

            player = new ActorPlayer();
            loadedMap.newActor(player);
            //loadedMap.newActor(new ActorPlayer(loadedMap.actorMaker));
            //loadedMap.newActor(new ActorPlayer(loadedMap.actorMaker));

            matrixProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, (float)gameWindow.Width / (float)gameWindow.Height, NEAR, FAR);
            

            matrixModel = Matrix4.Identity;
            
            matrixCamera = Matrix4.Identity;

            player.position.X = 64;
            player.position.Y = 128;
            player.position.Z = 64;

            player.target = player.position;

        }

        void onUnload(object sender, EventArgs e) {
            Console.Out.WriteLine("onUnload");
            exiting = true;
        }

        void onResize(object sender, EventArgs e) {
            TryGL.Call(() => GL.Viewport(0, 0, gameWindow.Width, gameWindow.Height));

            settings.windowWidth.value = gameWindow.Width;
            settings.windowHeight.value = gameWindow.Height;

            settings.save();
        }

        void onUpdateFrame(object sender, FrameEventArgs e) {
        	UPS = 1.0 / e.Time;
            keyboard = Keyboard.GetState();

            gameWindow.Title = string.Format("UPS: {0:F}, FPS: {0:F}", UPS, FPS);

            loadedMap.update(this);


            if (keyboard.IsKeyDown(Key.A)) {
                camAngle.Phi += settings.cameraScrollSpeed * (settings.invertCameraX ? -1 : 1);
                rayShouldUpdate = true;
            }
			if (keyboard.IsKeyDown(Key.D)) {
                camAngle.Phi -= settings.cameraScrollSpeed * (settings.invertCameraX ? -1 : 1);
                rayShouldUpdate = true;
            }

            if (keyboard.IsKeyDown(Key.W)) {
                camAngle.Theta += settings.cameraScrollSpeed * (settings.invertCameraY ? -1 : 1);
                rayShouldUpdate = true;
            }
			if (keyboard.IsKeyDown(Key.S)) {
                camAngle.Theta -= settings.cameraScrollSpeed * (settings.invertCameraY ? -1 : 1);
                rayShouldUpdate = true;
            }

			if (keyboard.IsKeyDown(Key.Plus)) {
				focusDistance = Math.Max(NEAR, focusDistance / settings.cameraZoomSpeed);
                rayShouldUpdate = true;
            }
			if (keyboard.IsKeyDown(Key.Minus)) {
				focusDistance = Math.Min(FAR, focusDistance * settings.cameraZoomSpeed);
                rayShouldUpdate = true;
            }

            if (camAngle.Theta < 0) {
                camAngle.Theta = 0;
            } else if (camAngle.Theta > Math.PI / 2) {
                camAngle.Theta = (float)Math.PI / 2;
            }

			if (editor.active) {
				focusPos = editor.focusPosition;
			} else {
				focusPos = player.position;
			}

            camPos.X = focusPos.X - (float)(Math.Cos(camAngle.Theta) * Math.Sin(camAngle.Phi) * focusDistance);
            camPos.Y = focusPos.Y + (float)(Math.Sin(camAngle.Theta) * focusDistance);
            camPos.Z = focusPos.Z + (float)(Math.Cos(camAngle.Theta) * Math.Cos(camAngle.Phi) * focusDistance);

            lightPosition.X = - 20000;
            lightPosition.Y = - 80000;
            lightPosition.Z = - 20000;
        }

        void onRenderFrame(object sender, FrameEventArgs e) {
            FPS = 1.0 / e.Time;

            TryGL.Call(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            shader.use();

            GL.Uniform3(shader.GetUniformID("light.position"), lightPosition);
            GL.Uniform3(shader.GetUniformID("light.color"), lightColor);

            //matrixCamera = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            matrixCamera = Matrix4.CreateTranslation(-camPos.X, -camPos.Y, -camPos.Z);
            matrixCamera *= Matrix4.CreateRotationY(camAngle.Phi);
            matrixCamera *= Matrix4.CreateRotationX(camAngle.Theta);

            resetColor();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2DArray, TextureControl.textureID);
            GL.Uniform1(shader.GetUniformID("Texture"), 0);

            loadedMap.drawActors(this);
            loadedMap.drawChunks(this);


            gameWindow.SwapBuffers();

            rayWasUpdated = false;
        }

        public void setModel() {
            GL.UniformMatrix4(shader.GetUniformID("ModelMatrix"), false, ref matrixModel);

            matrixView = matrixModel * matrixCamera * matrixProjection;
            GL.UniformMatrix4(shader.GetUniformID("ViewMatrix"), false, ref matrixView);

            matrixNormal = new Matrix3(Matrix4.Transpose(Matrix4.Invert(matrixModel)));
            GL.UniformMatrix3(shader.GetUniformID("NormalMatrix"), false, ref matrixNormal);
        }

        public void setColor(Vector4 color, bool replace) {
            GL.Uniform4(shader.GetUniformID("color"), color);
            GL.Uniform1(shader.GetUniformID("replaceColor"), replace?1:0);
        }

        public void resetColor() {
            GL.Uniform4(shader.GetUniformID("color"), Vector4.One);
            GL.Uniform1(shader.GetUniformID("replaceColor"), 0);
        }
        
        public void resetBlending() {
            TryGL.Call(() => GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha));
        }

        public void additiveBlending() {
            TryGL.Call(() => GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One));
        }

        public void setTexture(int texIndex) {
            GL.Uniform1(shader.GetUniformID("textureIndex"), texIndex);
        }

        bool intercectAABBRay(Vector3 min, Vector3 max, ref Vector3 origin, ref Vector3 direction) {
            float tmin = 0, tmax = 0;
            for (int a = 0; a < 3; a++) {
                float invD = 1.0f / direction[a];
                float t0 = (min[a] - origin[a]) * invD;
                float t1 = (max[a] - origin[a]) * invD;
                if (invD < 0.0f) {
                    float tmp = t0;
                    t0 = t1;
                    t1 = tmp;
                }
                tmin = a==0 ? t0 : (t0 > tmin ? t0 : tmin);
                tmax = a==0 ? t1 : (t1 < tmax ? t1 : tmax);
                if (tmax <= tmin)
                    return false;
            }
            return true;
        }

        bool intercectTriangleRay(Vector3 V1, Vector3 V2, Vector3 V3, ref Vector3 O, ref Vector3 D, out float outt) {
            Vector3 e1, e2;  //Edge1, Edge2
            Vector3 P, Q, T;
            float det, inv_det, u, v;
            float t;
            outt = 0;

            //Find vectors for two edges sharing V1
            e1 = V2 - V1;
            e2 = V3 - V1;
            //Begin calculating determinant - also used to calculate u parameter
            P = Vector3.Cross(D, e2);
            //if determinant is near zero, ray lies in plane of triangle
            det = Vector3.Dot(e1, P);
            //NOT CULLING
            if (det > -FLOAT_EPSILON && det < FLOAT_EPSILON) return false;
            inv_det = 1.0f / det;

            //calculate distance from V1 to ray origin
            T = O - V1;

            //Calculate u parameter and test bound
            u = Vector3.Dot(T, P) * inv_det;
            //The intersection lies outside of the triangle
            if (u < 0.0f || u > 1.0f) return false;

            //Prepare to test v parameter
            Q = Vector3.Cross(T, e1);

            //Calculate V parameter and test bound
            v = Vector3.Dot(D, Q) * inv_det;
            //The intersection lies outside of the triangle
            if (v < 0.0f || u + v > 1.0f) return false;

            t = Vector3.Dot(e2, Q) * inv_det;

            if (t > FLOAT_EPSILON) { //ray intersection
                outt = t;
                return true;
            }

            // No hit, no win
            return false;
        }

		void getRayFromWindowPoint(ref Point2 point, out Vector3 origin, out Vector3 direction) {
			float aspect = (float)gameWindow.Width / (float)gameWindow.Height;
            float normX = (point.X - (gameWindow.Width / 2.0f)) / (gameWindow.Width / 2.0f);
            float normY = (gameWindow.Height - point.Y - (gameWindow.Height / 2.0f)) / (gameWindow.Height / 2.0f);

            //Console.WriteLine("norm {0}, {1}", normX, normY);
            //return;

            float rx = NEAR * aspect * normX;
            float ry = NEAR * normY;

            Vector4 rayPoint = new Vector4(0, 0, 0, 1);
            Vector4 rayVector = new Vector4(rx, ry, -NEAR, 0);

            Matrix4 mat = matrixCamera.Inverted();

            origin = Vector4.Transform(rayPoint, mat).Xyz;
            direction = Vector4.Transform(rayVector, mat).Xyz.Normalized();
		}

        bool findTerrainWithRay(ref Vector3 origin, ref Vector3 direction, out Vector3 outLocation, float far = FAR) {
            

            Vector3 p1 = new Vector3();
            Vector3 p2 = new Vector3();
            Vector3 p3 = new Vector3();

            /*for(float i = 0; i < 20; i += 1) {
                ActorCharacter p = new ActorCharacter(loadedMap.actorMaker);
                p.position = origin + (direction * i);
                loadedMap.newActor(p);
                Console.WriteLine(p.position);
            }*/

            float closest = far;

            float t;

			loadedMap.lockChunks();

            foreach (Chunk c in loadedMap.loadedChunks.Values) {
                foreach (HeightMap h in c.terrain) {
                    if (h.isSolid) {

                        Vector3 offset = new Vector3(c.Location.X * Chunk.CHUNK_SIZE_X, 0, c.Location.Y * Chunk.CHUNK_SIZE_Z);

                        if (intercectAABBRay(new Vector3(offset.X, h.min, offset.Z), new Vector3(offset.X + Chunk.CHUNK_SIZE_X, h.max, offset.Z + Chunk.CHUNK_SIZE_Z), ref origin, ref direction)) {

                            for (int x = 0; x < Chunk.CHUNK_SIZE_X; x++) {
                                for (int z = 0; z < Chunk.CHUNK_SIZE_Z; z++) {
                                    p1.X = x - Chunk.GRID_HALF_SIZE + offset.X; p1.Y = h.heights[x, z] + offset.Y; p1.Z = z - Chunk.GRID_HALF_SIZE + offset.Z;
                                    p2.X = x + Chunk.GRID_HALF_SIZE + offset.X; p2.Y = h.heights[x + 1, z] + offset.Y; p2.Z = z - Chunk.GRID_HALF_SIZE + offset.Z;
                                    p3.X = x - Chunk.GRID_HALF_SIZE + offset.X; p3.Y = h.heights[x, z + 1] + offset.Y; p3.Z = z + Chunk.GRID_HALF_SIZE + offset.Z;

                                    if (intercectTriangleRay(p1, p2, p3, ref origin, ref direction, out t)) {
                                        closest = Math.Min(closest, t);
                                    } else {
                                        p1.X = x + Chunk.GRID_HALF_SIZE + offset.X; p1.Y = h.heights[x + 1, z + 1] + offset.Y; p1.Z = z + Chunk.GRID_HALF_SIZE + offset.Z;
                                        if (intercectTriangleRay(p3, p2, p1, ref origin, ref direction, out t)) {
                                            closest = Math.Min(closest, t);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }

			loadedMap.unlockChunks();
			
            outLocation = origin + (direction * closest);//new Vector3(close.X, loadedMap.getFloorAtPosition(close.X, close.Y + player.maxSlope, close.Z), close.Z);
            return closest < far;
        }

		bool findPropWithRay(ref Vector3 origin, ref Vector3 direction, out Prop closestProp, float far = FAR) {

			closestProp = null;
			float close = far;
		
			foreach (Chunk c in loadedMap.loadedChunks.Values) {
				foreach (Prop p in c.props) {
					if (intercectAABBRay(p.frameMin, p.frameMax, ref origin, ref direction)) {
                        bool hit = false;
                        float d = FAR;

                        for (int i = 0; i < p.model.ind.Count; i+=3) {
                            if (intercectTriangleRay(p.transformedPoints[(int)p.model.ind[i]], p.transformedPoints[(int)p.model.ind[i+1]], p.transformedPoints[(int)p.model.ind[i+2]], ref origin, ref direction, out d)) {
                                hit = true;
                                break;
                            }
                        }
                        if (hit) {
                            if (d <= close) {
                                close = d;
                                closestProp = p;
                            }
                        }
					}
				}
			}
			
			return close <= far;
		}
        void onKeyDown(object sender, KeyboardKeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape: {
                    gameWindow.Exit();
                    break;
                }
				case Key.U: {
					loadedMap.unloadAllChunks();
					break;
				}
				case Key.Tab: {
					editor.active = !editor.active;
					break;
				}
            }

			if (editor.active) {
				editor.onKeyDown(sender, e);
			}
			
        }

        void onKeyUp(object sender, KeyboardKeyEventArgs e) {

        }

        void onMouseDown(object sender, MouseButtonEventArgs e) {
            switch (e.Button) {
                case MouseButton.Left: {
                    mouseLeftClicked = true;
                    rayShouldUpdate = true;
                    break;
                }
                case MouseButton.Right: {
                    mouseRightClicked = true;
                    rayShouldUpdate = true;
                    break;
                }
            }
        }

        void onMouseMove(object sender, MouseMoveEventArgs e) {
            mousePosition.X = e.X;
            mousePosition.Y = e.Y;
            rayShouldUpdate = true;
        }

        private void heavy() {

            while (!exiting) {

                if (rayShouldUpdate) {
                    rayWasUpdated = true;
                    rayShouldUpdate = false;

                    getRayFromWindowPoint(ref mousePosition, out lookOrigin, out lookDirection);

                    Vector3 terrain;
                    bool hitTerrain = findTerrainWithRay(ref lookOrigin, ref lookDirection, out terrain);

                    Prop p;
                    if (findPropWithRay(ref lookOrigin, ref lookDirection, out p, hitTerrain ? (lookOrigin - terrain).Length : FAR)) {
                        //Console.WriteLine("Found prop with model: {0}", p.model.name);
                        if (p is PropInteractable) {
                            if ((p as PropInteractable).canHighlight) {
                                (p as PropInteractable).highlightTimeout = 2;
                            }
                        }
                    }

                    if (mouseLeftClicked) {
                        mouseLeftClicked = false;
                        if (hitTerrain) {
                            player.target = terrain;
                        }
                    }
                }
				
			}
		}

	}
}
