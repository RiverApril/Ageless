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

        GameWindow gameWindow;

        double UPS, RPS;

        public ShaderProgram shader;

        public Matrix4 projection;
        public Matrix4 model;
        //public Matrix4 normal;

        public World loadedWorld;

        public Random rand = new Random();

        private Vector3 camPos = new Vector3();
        private Angle2 camAngle = new Angle2();
        private Vector3 focusPos = new Vector3();
        private float focusSpeed = 1.0f / 8.0f;
        private float focusDistance = 20.0f;
        private float camSpeed = 1.0f/8.0f;
        private float camRotSpeed = (float)(Math.PI / 180);
        private int camRegisterBorder = 10;

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

            Version version = new Version(GL.GetString(StringName.Version).Substring(0, 3));
            Version target = new Version(4, 0);
            if (version < target) {
                throw new NotSupportedException(String.Format("OpenGL {0} is required. Current version is {1}.", target, version));
            }

            TryGL.Call(() => GL.ClearColor(0.1f, 0.0f, 0.1f, 1.0f));
            TryGL.Call(() => GL.Enable(EnableCap.DepthTest));
            TryGL.Call(() => GL.CullFace(CullFaceMode.Front));
            TryGL.Call(() => GL.Enable(EnableCap.CullFace));


            shader = new ShaderProgram();
            shader.CompileProgram(File.ReadAllText("../../vertex.glsl"), File.ReadAllText("../../fragment.glsl"));

            TextureControl.loadTextures();

            loadedWorld = new World(this);
            loadedWorld.loadChunk(new Point2(0, 0));
            //loadedWorld.loadChunk(new Point2(0, 1));
            //loadedWorld.loadChunk(new Point2(1, 0));
            //loadedWorld.loadChunk(new Point2(0, -1));
            //loadedWorld.loadChunk(new Point2(-1, 0));


            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, gameWindow.Width / (float)gameWindow.Height, 0.01f, 1024.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            model = Matrix4.Identity;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref model);
        }

        void onUnload(object sender, EventArgs e) {
            Console.Out.WriteLine("onUnload");

        }

        void onResize(object sender, EventArgs e) {
            GL.Viewport(0, 0, gameWindow.Width, gameWindow.Height);

        }

        void onUpdateFrame(object sender, FrameEventArgs e) {
            UPS = 1.0 / e.Time;

            gameWindow.Title = string.Format("UPS: {0:F}  RPS: {1:F}", UPS, RPS);
        }

        void onRenderFrame(object sender, FrameEventArgs e) {
            RPS = 1.0 / e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.use();

            KeyboardState k = Keyboard.GetState();

            float dx = 0;
            float dz = 0;

            focusPos.X += dx;
            focusPos.Z += dz;
            focusPos.Y = loadedWorld.getHeightAtPosition(focusPos.X, focusPos.Z);

            int mx = gameWindow.Mouse.X;
            int my = gameWindow.Mouse.Y;


            if (mx <= camRegisterBorder || k.IsKeyDown(Key.A)) {
                camAngle.Phi += camRotSpeed;
            } else if (mx >= gameWindow.Width - camRegisterBorder || k.IsKeyDown(Key.D)) {
                camAngle.Phi -= camRotSpeed;
            }

            if (my <= camRegisterBorder || k.IsKeyDown(Key.W)) {
                camAngle.Theta += camRotSpeed;
            } else if (my >= gameWindow.Height - camRegisterBorder || k.IsKeyDown(Key.S)) {
                camAngle.Theta -= camRotSpeed;
            }

            if (camAngle.Theta < 0) {
                camAngle.Theta = 0;
            } else if (camAngle.Theta > Math.PI / 2) {
                camAngle.Theta = (float)Math.PI / 2;
            }

            camPos.X = focusPos.X + -(float)(Math.Cos(camAngle.Theta) * Math.Sin(camAngle.Phi) * focusDistance);
            camPos.Y = focusPos.Y + (float)(Math.Sin(camAngle.Theta) * focusDistance);
            camPos.Z = focusPos.Z + (float)(Math.Cos(camAngle.Theta) * Math.Cos(camAngle.Phi) * focusDistance);

            //lastMouse = m;

            model = Matrix4.CreateTranslation(0.0f, -2.0f, 0.0f);
            model *= Matrix4.CreateTranslation(-camPos.X, -camPos.Y, -camPos.Z);
            model *= Matrix4.CreateRotationY(camAngle.Phi);
            model *= Matrix4.CreateRotationX(camAngle.Theta);
            //model *= Matrix4.CreateRotationZ(camAngle.Z);

            GL.BindTexture(TextureTarget.Texture2D, TextureControl.terrain);
            GL.ActiveTexture(TextureUnit.Texture0);

            //normal = Matrix4.Transpose(Matrix4.Invert(model));

            GL.UniformMatrix4(shader.GetUniformID("ProjectionMatrix"), false, ref projection);
            GL.UniformMatrix4(shader.GetUniformID("ModelMatrix"), false, ref model);
            //GL.UniformMatrix4(shader.GetUniformID("NormalMatrix"), false, ref normal);
            GL.Uniform1(shader.GetUniformID("Texture"), 0);

            loadedWorld.drawChunks();

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
