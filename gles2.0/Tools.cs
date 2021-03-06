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
using System.IO;

namespace AndroidUI
{
    class Tools
    {
        static public float[] Matrix4toArray16(Matrix4 mat)
        {
            return new float[] {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };
        }

        static public TextReader GetFileReader(Context context, string filename)
        {
            StreamReader reader = null;
            try
            {
                using (var input = context.Assets.Open(filename))
                reader = new StreamReader(input);
                return reader;
            }
            catch { return null; }
        }
    }
}