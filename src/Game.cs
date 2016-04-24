using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Drawing.Imaging;

namespace Ageless {

	public class Game {

		public static readonly string dir = "../../";
		public static readonly string dirMap = dir+"map/";
		public static readonly string dirTex = dir+"tex/";
		public static readonly string dirSdr = dir+"sdr/";

        public static bool exiting = false;

        GameWindow gameWindow;

        double UPS, RPS;

        public ShaderProgram shader;

        public Matrix4 matrixProjection;
        public Matrix4 matrixWorld;
        public Matrix4 matrixModel;

		public Matrix4 matrixWMP;
        public Matrix3 matrixNormal;

        public World loadedWorld;

        public Random rand = new Random();

        private Vector3 camPos = new Vector3();
        private Angle2 camAngle = new Angle2();
        private Vector3 focusPos = new Vector3();
        private float focusDistance = 20.0f;
        private float camRotSpeed = (float)(Math.PI / 180);

        public ActorPlayer player;

        Vector3 lightPosition = new Vector3(0, 0, 0);
        Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);

        public KeyboardState keyboard;

        public Game() {

            gameWindow = new GameWindow();

            gameWindow.Load += onLoad;
            gameWindow.Unload += onUnload;
            gameWindow.Resize += onResize;
            gameWindow.UpdateFrame += onUpdateFrame;
            gameWindow.RenderFrame += onRenderFrame;
            gameWindow.KeyDown += onKeyDown;
            gameWindow.KeyUp += onKeyUp;

            gameWindow.Run(60.0);
        }

        void onLoad(object sender, EventArgs e) {
            Console.Out.WriteLine("onLoad");

            gameWindow.VSync = VSyncMode.On;

			Console.WriteLine("OpenGL version: {0}", GL.GetString(StringName.Version));
			//Console.WriteLine("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
			int glslVersion = int.Parse(GL.GetString(StringName.ShadingLanguageVersion).Replace(".", ""));

            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(2, 0);
            if (version < target) {
                throw new NotSupportedException(String.Format("OpenGL {0} is required. Current version is {1}.", target, version));
            }

            TryGL.Call(() => GL.ClearColor(0.1f, 0.0f, 0.1f, 1.0f));
            TryGL.Call(() => GL.Enable(EnableCap.DepthTest));
            //TryGL.Call(() => GL.CullFace(CullFaceMode.Front));
            //TryGL.Call(() => GL.Enable(EnableCap.CullFace));


            shader = new ShaderProgram();
			Console.WriteLine("Current GLSL version: {0}", glslVersion);
			while(!File.Exists(dirSdr+"vertex."+glslVersion+".glsl")){
				glslVersion--;
				if(glslVersion <= 0){
					Console.WriteLine("Shader with required version not found.");
					throw new NotSupportedException(String.Format("Please create shader files with version {0} or lower.", GL.GetString(StringName.ShadingLanguageVersion)));
				}
			}
			Console.WriteLine("Using shader with GLSL version: {0}", glslVersion);
			shader.CompileProgram(File.ReadAllText(dirSdr+"vertex."+glslVersion+".glsl"), File.ReadAllText(dirSdr+"fragment."+glslVersion+".glsl"));

            TextureControl.loadTextures();

            loadedWorld = new World(this);

			player = new ActorPlayer(loadedWorld.actorMaker);
			loadedWorld.newActor(player);

            matrixProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, gameWindow.Width / (float)gameWindow.Height, 0.01f, 1024.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref matrixProjection);

            matrixModel = Matrix4.Identity;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref matrixModel);

            matrixWorld = Matrix4.Identity;

            player.position.X = 64;
            player.position.Y = 100;
			player.position.Z = 64;

        }

        void onUnload(object sender, EventArgs e) {
            Console.Out.WriteLine("onUnload");
			exiting = true;
			loadedWorld.actorMaker.thread.Abort();
			loadedWorld.chunkMaker.thread.Abort();
        }

        void onResize(object sender, EventArgs e) {
            GL.Viewport(0, 0, gameWindow.Width, gameWindow.Height);

        }

        void onUpdateFrame(object sender, FrameEventArgs e) {
            UPS = 1.0 / e.Time;
            keyboard = Keyboard.GetState();

            gameWindow.Title = string.Format("UPS: {0:F}  RPS: {1:F}", UPS, RPS);

			loadedWorld.update(this);
            
            //int mx = gameWindow.Mouse.X;
            //int my = gameWindow.Mouse.Y;


            if (/*mx <= camRegisterBorder ||*/ keyboard.IsKeyDown(Key.A)) {
                camAngle.Phi += camRotSpeed;
            } else if (/*mx >= gameWindow.Width - camRegisterBorder ||*/ keyboard.IsKeyDown(Key.D)) {
                camAngle.Phi -= camRotSpeed;
            }

            if (/*my <= camRegisterBorder ||*/ keyboard.IsKeyDown(Key.W)) {
                camAngle.Theta += camRotSpeed;
            } else if (/*my >= gameWindow.Height - camRegisterBorder ||*/ keyboard.IsKeyDown(Key.S)) {
                camAngle.Theta -= camRotSpeed;
            }

            if (camAngle.Theta < 0) {
                camAngle.Theta = 0;
			} else if (camAngle.Theta > Math.PI / 2) {
                camAngle.Theta = (float)Math.PI / 2;
            }

            lightPosition.X = player.position.X;
            lightPosition.Y = player.position.Y-128;
            lightPosition.Z = player.position.Z;
        }

        void onRenderFrame(object sender, FrameEventArgs e) {
            RPS = 1.0 / e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.use();

            focusPos = player.position;

            camPos.X = focusPos.X - (float)(Math.Cos(camAngle.Theta) * Math.Sin(camAngle.Phi) * focusDistance);
            camPos.Y = focusPos.Y + (float)(Math.Sin(camAngle.Theta) * focusDistance);
            camPos.Z = focusPos.Z + (float)(Math.Cos(camAngle.Theta) * Math.Cos(camAngle.Phi) * focusDistance);

            //lastMouse = m;

            matrixWorld = Matrix4.CreateTranslation(0.0f, -2.0f, 0.0f);
            matrixWorld *= Matrix4.CreateTranslation(-camPos.X, -camPos.Y, -camPos.Z);
            matrixWorld *= Matrix4.CreateRotationY(camAngle.Phi);
            matrixWorld *= Matrix4.CreateRotationX(camAngle.Theta);
            //model *= Matrix4.CreateRotationZ(camAngle.Z);

			matrixWMP = matrixWorld * matrixModel * matrixProjection;
            matrixNormal = new Matrix3(Matrix4.Transpose(Matrix4.Invert(matrixModel)));

            GL.UniformMatrix4(shader.GetUniformID("WMPMatrix"), false, ref matrixWMP);
            GL.UniformMatrix4(shader.GetUniformID("ModelMatrix"), false, ref matrixModel);
            GL.UniformMatrix3(shader.GetUniformID("NormalMatrix"), false, ref matrixNormal);
            GL.Uniform3(shader.GetUniformID("light.position"), lightPosition);
            GL.Uniform3(shader.GetUniformID("light.color"), lightColor);


			GL.BindTexture(TextureTarget.Texture2D, TextureControl.terrain);
			GL.ActiveTexture(TextureUnit.Texture0);

			GL.Uniform1(shader.GetUniformID("Texture"), 0);
			loadedWorld.drawChunks();

			GL.BindTexture(TextureTarget.Texture2D, TextureControl.actors);
			GL.ActiveTexture(TextureUnit.Texture1);

			GL.Uniform1(shader.GetUniformID("Texture"), 1);
			loadedWorld.drawActors();

            gameWindow.SwapBuffers();
        }

        void onKeyDown(object sender, KeyboardKeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape: {
                    gameWindow.Exit();
                    break;
                }
            }
        }

        void onKeyUp(object sender, KeyboardKeyEventArgs e) {

        }
    }
}
