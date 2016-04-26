using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using System.IO;



namespace Ageless {

    public class ShaderProgram {

        public int ProgramID = 0;
        public Dictionary<string, int> variableIDs = new Dictionary<string, int>();

        public ShaderProgram() {

        }

        public void LoadAndCompileProrgam(string vertPath, string fragPath) {
            CompileProgram(LoadShader(vertPath), LoadShader(fragPath));
        }

        private string LoadShader(string path) {
			return File.ReadAllText(path);
        }

        private void CompileShader(string code, ShaderType type) {
            int shaderID = GL.CreateShader(type);

            string info = "";
            int statusCode = -1;

            TryGL.Call(() => GL.ShaderSource(shaderID, code));
            TryGL.Call(() => GL.CompileShader(shaderID));
            TryGL.Call(() => GL.GetShaderInfoLog(shaderID, out info));
            TryGL.Call(() => GL.GetShader(shaderID, ShaderParameter.CompileStatus, out statusCode));

            if (statusCode != 1) {
                Console.WriteLine("Failed to compile shader source, type: " + type.ToString());
                Console.WriteLine(info);
                Console.WriteLine("Status Code: " + statusCode);
                TryGL.Call(() => GL.DeleteShader(shaderID));
                TryGL.Call(() => GL.DeleteProgram(ProgramID));
                ProgramID = 0;
                throw new Exception("Failed to compile shader source, type: "+type.ToString());
            }

            TryGL.Call(() => GL.AttachShader(ProgramID, shaderID));
            TryGL.Call(() => GL.DeleteShader(shaderID));
        }

        public void CompileProgram(string vertCode, string fragCode) {
            if (ProgramID > 0) {
                TryGL.Call(() => GL.DeleteProgram(ProgramID));
            }
            variableIDs.Clear();
            ProgramID = GL.CreateProgram();


            CompileShader(vertCode, ShaderType.VertexShader);
            CompileShader(fragCode, ShaderType.FragmentShader);

            string info = "";
            int statusCode = -1;
			
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, 0, "vertexPosition"));
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, 1, "vertexUV"));
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, 2, "vertexNormal"));

            TryGL.Call(() => GL.LinkProgram(ProgramID));
            TryGL.Call(() => GL.GetProgramInfoLog(ProgramID, out info));
            TryGL.Call(() => GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out statusCode));

            if (statusCode != 1) {
                Console.WriteLine("Failed to link shader program.");
                Console.WriteLine(info);
                Console.WriteLine("Status Code: " + statusCode);
                TryGL.Call(() => GL.DeleteProgram(ProgramID));
                ProgramID = 0;
                throw new Exception("Failed to link shader program");
            }
        }

        public void use(){
            TryGL.Call(() => GL.UseProgram(ProgramID));
        }

        public int GetUniformID(string name) {
            if (!variableIDs.ContainsKey(name)) {

                int varID = GL.GetUniformLocation(ProgramID, name);

                if (varID != -1) {
                    variableIDs.Add(name, varID);
                } else {
                    Console.WriteLine("Failed to find variable in shader: " + name);
                }

            }
            return variableIDs[name];
        }
    }
}
