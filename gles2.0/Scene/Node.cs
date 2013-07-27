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
    class Node
    {
        protected Vector3 pos;
        protected Transformation transform;

        public Node()
        {
            Pos = Vector3.Zero;
            Transform = null;
        }

        public Node(Vector3 pos)
        {
            Pos = pos;
        }

        public virtual Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public virtual Transformation Transform
        {
            get { return transform; }
            set { transform = value; }
        }
    }
}