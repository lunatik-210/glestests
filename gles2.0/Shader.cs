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

namespace AndroidUI
{
    class Shader
    {
        private int programHandle;

        public Shader(string vertexShaderCode, string fragmentShaderCode)
        {
            programHandle = createProgram(vertexShaderCode, fragmentShaderCode);
        }

        public int Program
        {
            get { return programHandle; }
        }

        public void Use()
        {
            GL.UseProgram(programHandle);
        }

        private int createProgram(string vertexShaderCode, string fragmentShaderCode)
        {
            int vertexShaderHandle = CreateShader(All.VertexShader, vertexShaderCode);
            int fragmentShaderHandle = CreateShader(All.FragmentShader, fragmentShaderCode);

            int programHandle = GL.CreateProgram();

            if (programHandle == 0)
                throw new InvalidOperationException("Unable to create program");

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
            return programHandle;
        }

        private int CreateShader(All type, string source)
        {
            int shader = GL.CreateShader(type);
            if (shader == 0)
                throw new InvalidOperationException("Unable to create shader");

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
                throw new InvalidOperationException("Unable to compile shader of type : " + type.ToString());
            }
            return shader;
        }

    }
}