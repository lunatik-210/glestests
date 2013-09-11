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

        private float xangle = 0.0f;
        private float yangle = 0.0f;
        private Shader shader = null;
        private Transformation globalTransform;

        public int viewportWidth;
        public int viewportHeight;

        public Scene(Shader shader) 
        {
            lights = new List<Light>();
            objects = new List<Object>();
            globalTransform = new Transformation();
            this.shader = shader;
        }

        public void init()
        {
            GL.Enable(All.DepthTest);
            GL.Enable(All.CullFace);
            GL.CullFace(All.Back);
            GL.Hint(All.GenerateMipmapHint, All.Nicest);

            shader.Use();

            ScaleCamera(1.0f);

            InitLights();

            UpdateProjection();
        }

        public void ScaleCamera(float scale)
        {
            shader.Use();

            camera.Pos = new Vector3(camera.Pos.X, camera.Pos.Y, camera.Pos.Z * scale);

            UpdateCamera();
        }

        public void UpdateSceneOrientation(float xdiff, float ydiff)
        {
            xangle += xdiff;
            yangle += ydiff;
            globalTransform.RotationVector = new Vector3(-yangle * 0.003f, -xangle * 0.003f, 0.0f);
        }

        public void render()
        {
            shader.Use();

            int u_ModelMatrix_Handle = GL.GetUniformLocation(shader.Program, "uModel");
            int u_NormalMatrix_Handle = GL.GetUniformLocation(shader.Program, "uNormal");

            foreach (Object obj in objects)
            {
                GL.UniformMatrix4(u_ModelMatrix_Handle, 1, false,
                    Tools.Matrix4toArray16(obj.Position.transformation * globalTransform.transformation));

                GL.UniformMatrix4(u_NormalMatrix_Handle, 1, false,
                    Tools.Matrix4toArray16(Matrix4.Transpose(Matrix4.Invert(obj.Position.transformation * globalTransform.transformation))));

                obj.Render(shader);
            }
        }

        public void SetShader(Shader shader)
        {
            this.shader = shader;
            init();
        }

        public void appendLight( Light light )
        {
            lights.Add(light);
        }

        public void appendObject( Object obj )
        {
            objects.Add(obj);
        }

        public void setViewport(int width, int height)
        {
            viewportWidth = width;
            viewportHeight = height;
            UpdateProjection();
        }

        public Camera Cam
        {
            set { camera = value; }
            get { return camera; }
        }

        private void UpdateProjection()
        {
            float ratio = (float)(viewportWidth) / (float)(viewportHeight);
            float k = 0.065f;
            float left = -k * ratio;
            float right = k * ratio;
            float bottom = -k;
            float top = k;
            float near = 0.1f;
            float far = 100.0f;

            Matrix4 projection = Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, near, far);
            //Matrix4 projection = OrthoProjection(left, right, bottom, top, near, far);

            int u_ProjectionMatrix_Handle = GL.GetUniformLocation(shader.Program, "uProjection");
            GL.UniformMatrix4(u_ProjectionMatrix_Handle, 1, false, Tools.Matrix4toArray16(projection));
        }

        private void InitLights()
        {
            int handle = -1;
            
            handle = GL.GetUniformLocation(shader.Program, "numLights");
            GL.Uniform1(handle, lights.Count);

            for (int i = 0; i < lights.Count; i++)
            {
                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].color");
                GL.Uniform4(handle, lights[i].color.X, lights[i].color.Y, lights[i].color.Z, lights[i].color.W);
                
                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].position");
                GL.Uniform3(handle, lights[i].pos.X, lights[i].pos.Y, lights[i].pos.Z);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].attenuation");
                GL.Uniform3(handle, lights[i].attenuation.X, lights[i].attenuation.Y, lights[i].attenuation.Z);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].direction");
                GL.Uniform3(handle, lights[i].direction.X, lights[i].direction.Y, lights[i].direction.Z);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].exponent");
                GL.Uniform1(handle, lights[i].exp);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].cosCutoff");
                GL.Uniform1(handle, lights[i].cosCutOff);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].ambient");
                GL.Uniform1(handle, lights[i].ambient);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].diffuse");
                GL.Uniform1(handle, lights[i].diffuse);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].specular");
                GL.Uniform1(handle, lights[i].specular);

                handle = GL.GetUniformLocation(shader.Program, "u_lights[" + i + "].type");
                GL.Uniform1(handle, (int)(lights[i].type));
            }
        }

        private void UpdateCamera()
        {
            int handle = GL.GetUniformLocation(shader.Program, "uView");
            GL.UniformMatrix4(handle, 1, false, Tools.Matrix4toArray16(camera.View));

            handle = GL.GetUniformLocation(shader.Program, "u_camera");
            GL.Uniform3(handle, camera.Pos.X, camera.Pos.Y, camera.Pos.Z);
        }
    }
}