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
    class Light : Node
    {
        private Vector4 color;
        private Vector3 attenuation;

        public Light() : base()
        {
            init(new Vector4(Vector3.Zero, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
        }

        public Light(Vector3 pos)
            : base(pos)
        {
            init(new Vector4(Vector3.Zero, 1.0f), new Vector3(0.0f, 0.0f, 0.0f));
        }

        public Light(Vector3 pos, Vector4 color)
            : base(pos)
        {
            init(color, new Vector3(0.0f, 0.0f, 0.0f));
        }

        public Light(Vector3 pos, Vector4 color, Vector3 att)
            : base(pos)
        {
            init(color, att);
        }

        private void init(Vector4 color, Vector3 att)
        {
            Color = color;
            Attenuation = att;
        }

        public Vector4 Color
        {
            get { return color; }
            set { color = value; }
        }

        public Vector3 Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; }
        }
    }
}