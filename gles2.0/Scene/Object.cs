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

using OpenTK;

using AndroidUI;
using OpenTK.Graphics.ES20;

namespace AndroidUI.Scene
{
    class Object
    {
        private ObjMesh mesh = null;
        private Transformation position = null;
        private int id = 3;

        public Object()
        {
            mesh = null;
            position = null;
        }

        public ObjMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }

        public Transformation Position
        {
            get { return position; }
            set { position = value; }
        }

        public void Render(Shader shader)
        {
            GL.Uniform1(GL.GetUniformLocation(shader.Program, "uObjectIndex"), 0.5f);
            Mesh.Render(shader);
        }
    }
}