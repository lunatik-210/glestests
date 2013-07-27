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

namespace AndroidUI.Scene
{
    class Camera : Node
    {
        public Matrix4 view;

        public Camera(Vector3 pos)
            : base(pos)
        {
            calcViewMatrix();
        }

        public override Vector3 Pos
        {
            set { pos = value; calcViewMatrix(); }
            get { return pos; }
        }

        public Matrix4 View
        {
            get { return view; }
        }

        private void calcViewMatrix()
        {
            view = Matrix4.LookAt(Pos, Vector3.Zero, Vector3.UnitY);
        }

    }
}