using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using OpenTK.Graphics.ES20;
using Android.Util;
using OpenTK;

namespace AndroidUI
{
    class Shader
    {
        private int programHandle;

        private int vertexShaderHandle;

        private int fragmentShaderHandle;

        public Shader()
        {
            Init();
        }

        public Shader(string vertexShaderCode, string fragmentShaderCode)
        {
            Init();
            createProgram(vertexShaderCode, fragmentShaderCode);
        }

        public int Program
        {
            get { return programHandle; }
        }

        public void Bind()
        {
            GL.UseProgram(programHandle);
        }

        public void Unbind()
        {
            GL.UseProgram(0);
        }

        public void createProgram(string vertexShaderCode, string fragmentShaderCode)
        {
            CompileShader(vertexShaderHandle, vertexShaderCode);
            CompileShader(fragmentShaderHandle, fragmentShaderCode);

            GL.AttachShader(programHandle, vertexShaderHandle);
            GL.AttachShader(programHandle, fragmentShaderHandle);

            GL.LinkProgram(programHandle);

            int linked = 0;
            GL.GetProgram(programHandle, All.LinkStatus, ref linked);
            if (linked == 0)
            {
                // link failed
                int length = 0;
                GL.GetProgram(programHandle, All.InfoLogLength, ref length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetProgramInfoLog(programHandle, length, ref length, log);
                    Log.Debug("GL2", "Couldn't link program: " + log.ToString());
                }

                GL.DeleteProgram(programHandle);
                throw new InvalidOperationException("Unable to link program");
            }
        }

        public void SetUniform(String name, int x)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x);
        }

        public void SetUniform(int location, int x)
        {
            GL.Uniform1(location, x);
        }

        public void SetUniform(String name, float x)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x);
        }

        public void SetUniform(int location, float x)
        {
            GL.Uniform1(location, x);
        }

        public void SetUniform(String name, int x, int y)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y);
        }

        public void SetUniform(int location, int x, int y)
        {
            GL.Uniform2(location, x, y);
        }

        public void SetUniform(String name, float x, float y)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y);
        }

        public void SetUniform(int location, float x, float y)
        {
            GL.Uniform2(location, x, y);
        }

        public void SetUniform(String name, int x, int y, int z)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y, z);
        }

        public void SetUniform(int location, int x, int y, int z)
        {
            GL.Uniform3(location, x, y, z);
        }

        public void SetUniform(String name, float x, float y, float z)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y, z);
        }

        public void SetUniform(int location, float x, float y, float z)
        {
            GL.Uniform3(location, x, y, z);
        }

        public void SetUniform(String name, int x, int y, int z, int w)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y, z, w);
        }

        public void SetUniform(int location, int x, int y, int z, int w)
        {
            GL.Uniform4(location, x, y, z, w);
        }

        public void SetUniform(String name, float x, float y, float z, float w)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, x, y, z, w);
        }

        public void SetUniform(int location, float x, float y, float z, float w)
        {
            GL.Uniform4(location, x, y, z, w);
        }

        public void SetUniform(String name, Vector2 vec)
        {
            SetUniform(name, vec.X, vec.Y);
        }

        public void SetUniform(String name, Vector3 vec)
        {
            SetUniform(name, vec.X, vec.Y, vec.Z);
        }

        public void SetUniform(String name, Vector4 vec)
        {
            SetUniform(name, vec.X, vec.Y, vec.Z, vec.W);
        }

        public void SetUniform(String name, Matrix4 mat)
        {
            int location = GL.GetUniformLocation(Program, name);
            if (location < 0)
                return;
            SetUniform(location, mat);
        }

        public void SetUniform(int location, Matrix4 mat)
        {
            GL.UniformMatrix4(location, 1, false, Tools.Matrix4toArray16(mat));
        }

        private void Init()
        {
            vertexShaderHandle = GL.CreateShader(All.VertexShader);

            fragmentShaderHandle = GL.CreateShader(All.FragmentShader);

            if (vertexShaderHandle == 0 || fragmentShaderHandle == 0)
                throw new InvalidOperationException("Unable to create shader");

            programHandle = GL.CreateProgram();

            if (programHandle == 0)
                throw new InvalidOperationException("Unable to create program");
        }

        private void CompileShader(int shader, string source)
        {
            int length = 0;
            GL.ShaderSource(shader, 1, new string[] { source }, (int[])null);
            GL.CompileShader(shader);

            int compiled = 0;
            GL.GetShader(shader, All.CompileStatus, ref compiled);
            if (compiled == 0)
            {
                length = 0;
                GL.GetShader(shader, All.InfoLogLength, ref length);
                if (length > 0)
                {
                    var log = new StringBuilder(length);
                    GL.GetShaderInfoLog(shader, length, ref length, log);
                    Log.Debug("GL2", "Couldn't compile shader: " + log.ToString());
                }

                GL.DeleteShader(shader);
                throw new InvalidOperationException("Unable to compile shader");
            }
        }

    }
}