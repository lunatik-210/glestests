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
using OpenTK.Graphics.ES20;

namespace Mono.Samples.GLTriangle20.Scene
{
    class Scene
    {
        private Camera camera = null;
        private List<Light> lights = null;
        private List<Object> objects = null;
        private int width;
        private int height;

        public Scene() 
        {
            lights = new List<Light>();
            objects = new List<Object>();
        }

        public void init(int shader)
        {
            Matrix4 view = Matrix4.LookAt(camera.Pos, Vector3.Zero, Vector3.UnitY);

            GL.UseProgram(shader);

            int u_ViewMatrix_Handle = GL.GetUniformLocation(shader, "uView");
            GL.UniformMatrix4(u_ViewMatrix_Handle, 1, false, Matrix4toArray16(view));

            int handle = GL.GetUniformLocation(shader, "u_lightPosition");
            GL.Uniform3(handle, lights[0].Pos.X, lights[0].Pos.Y, lights[0].Pos.Z);

            handle = GL.GetUniformLocation(shader, "u_lightColor");
            GL.Uniform4(handle, lights[0].Color.X, lights[0].Color.Y, lights[0].Color.Z, lights[0].Color.W);

            handle = GL.GetUniformLocation(shader, "u_camera");
            GL.Uniform3(handle, camera.Pos.X, camera.Pos.Y, camera.Pos.Z);
        }

        private float[] Matrix4toArray16(Matrix4 mat)
        {
            return new float[] {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };
        }

        public void render( int shader )
        {
            float ratio = (float)Width / Height;
            float k = 0.065f;
            float left = -k * ratio;
            float right = k * ratio;
            float bottom = -k;
            float top = k;
            float near = 0.1f;
            float far = 100.0f;
            Matrix4 projection = Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, near, far);
            int u_ProjectionMatrix_Handle = GL.GetUniformLocation(shader, "uProjection");
            GL.UniformMatrix4(u_ProjectionMatrix_Handle, 1, false, Matrix4toArray16(projection));

            int u_ModelMatrix_Handle = GL.GetUniformLocation(shader, "uModel");
            GL.UniformMatrix4(u_ModelMatrix_Handle, 1, false, Matrix4toArray16(objects[0].Transform.transformation));
            objects[0].Mesh.Render(shader);
        }

        public void appendLight( Light light )
        {
            lights.Add(light);
        }

        public void appendObject( Object obj )
        {
            objects.Add(obj);
        }

        public int Width
        {
            get { return width; }
            set { width = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        public Camera Cam
        {
            set { camera = value; }
            get { return camera; }
        }

    }
}