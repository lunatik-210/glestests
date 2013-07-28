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

namespace AndroidUI.Scene
{
    class Scene
    {
        private Camera camera = null;
        private List<Light> lights = null;
        private List<Object> objects = null;
        private int width;
        private int height;

        private float xangle = 0.0f;
        private float yangle = 0.0f;
        private float angle_inc = 0.005f;

        public Scene() 
        {
            lights = new List<Light>();
            objects = new List<Object>();
        }

        public void init(int shader)
        {
            Matrix4 view = Matrix4.LookAt(camera.Pos, Vector3.Zero, Vector3.UnitY);

            GL.UseProgram(shader);

            int handle = GL.GetUniformLocation(shader, "uView");
            GL.UniformMatrix4(handle, 1, false, Matrix4toArray16(view));

            handle = GL.GetUniformLocation(shader, "u_camera");
            GL.Uniform3(handle, camera.Pos.X, camera.Pos.Y, camera.Pos.Z);

            for (int i = 0; i < lights.Count; i++)
            {
               handle = GL.GetUniformLocation(shader, "u_lights[" + i + "].color");
               GL.Uniform4(handle, lights[i].Color.X, lights[i].Color.Y, lights[i].Color.Z, lights[i].Color.W);
               handle = GL.GetUniformLocation(shader, "u_lights[" + i + "].position");
               GL.Uniform3(handle, lights[i].Pos.X, lights[i].Pos.Y, lights[i].Pos.Z);
            }
            
        }

        public void Update(float xdiff, float ydiff)
        {
            xangle += xdiff;
            yangle += ydiff;
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
            int u_NormalMatrix_Handle = GL.GetUniformLocation(shader, "uNormal");

            Transformation t = new Transformation();
            t.RotationVector = new Vector3(-yangle * 0.005f, -xangle * 0.005f, 0);

            foreach (Object obj in objects)
            {
                GL.UniformMatrix4(u_ModelMatrix_Handle, 1, false, Matrix4toArray16(obj.Transform.transformation * t.transformation ));
                GL.UniformMatrix4(u_NormalMatrix_Handle, 1, false, Matrix4toArray16(Matrix4.Transpose(Matrix4.Invert(obj.Transform.transformation * t.transformation))));
                obj.Mesh.Render(shader);
            }
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