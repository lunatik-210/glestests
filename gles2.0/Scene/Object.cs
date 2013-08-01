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
    }
}