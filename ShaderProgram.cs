using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;



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
            throw new NotImplementedException();
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

            int a = 0;

            TryGL.Call(() => GL.BindAttribLocation(ProgramID, a, "vertexPosition"));a++;
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, a, "vertexColor")); a++;
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, a, "vertexUV")); a++;
            TryGL.Call(() => GL.BindAttribLocation(ProgramID, a, "vertexNormal"));a++;

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

        public void SetUniform(string name, float x) {
            if (ProgramID > 0) {
                int varID = GetUniformID(name);
                if (varID != -1) {
                    TryGL.Call(() => GL.Uniform1(varID, x));
                }
            }
        }

        public void SetUniform(string name, float x, float y) {
            if (ProgramID > 0) {
                int varID = GetUniformID(name);
                if (varID != -1) {
                    TryGL.Call(() => GL.Uniform2(varID, x, y));
                }
            }
        }

        public void SetUniform(string name, float x, float y, float z) {
            if (ProgramID > 0) {
                int varID = GetUniformID(name);
                if (varID != -1) {
                    TryGL.Call(() => GL.Uniform3(varID, x, y, z));
                }
            }
        }

        public void SetUniform(string name, float x, float y, float z, float w) {
            if (ProgramID > 0) {
                int varID = GetUniformID(name);
                if (varID != -1) {
                    TryGL.Call(() => GL.Uniform4(varID, x, y, z, w));
                }
            }
        }

        public void SetUniform(string name, Matrix4 matrix) {
            if (ProgramID > 0) {
                int varID = GetUniformID(name);
                if (varID != -1) {
                    GL.UniformMatrix4(varID, false, ref matrix);
                    TryGL.CheckError();
                }
            }
        }

        public void SetUniform(string name, Vector2 v) {
            SetUniform(name, v.X, v.Y);
        }

        public void SetUniform(string name, Vector3 v) {
            SetUniform(name, v.X, v.Y, v.Z);
        }
    }
}
