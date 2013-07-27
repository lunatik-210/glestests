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

namespace Mono.Samples.GLTriangle20.Scene
{
    class Light : Node
    {
        public Vector4 color;

        public Light() : base()
        {
            init(new Vector4(Vector3.Zero, 1.0f));
        }

        public Light(Vector3 pos)
            : base(pos)
        {
            init(new Vector4(Vector3.Zero, 1.0f));
        }

        public Light(Vector3 pos, Vector4 color)
            : base(pos)
        {
            init(color);
        }

        private void init(Vector4 color)
        {
            Color = color;
        }

        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}