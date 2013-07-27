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

using Mono.Samples.GLTriangle20;

namespace Mono.Samples.GLTriangle20.Scene
{
    class Object : Node
    {
        private ObjMesh mesh = null;

        public Object()
            : base()
        {
        }

        public Object(Vector3 pos)
            : base(pos)
        {
        }

        public ObjMesh Mesh
        {
            get { return mesh; }
            set { mesh = value; }
        }
    }
}