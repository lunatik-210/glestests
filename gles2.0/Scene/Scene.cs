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

            shader.Bind();

            ScaleCamera(1.0f);

            InitLights();

            UpdateProjection();
        }

        public void ScaleCamera(float scale)
        {
            shader.Bind();

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
            shader.Bind();

            int u_ModelMatrix_Handle = GL.GetUniformLocation(shader.Program, "uModel");
            int u_NormalMatrix_Handle = GL.GetUniformLocation(shader.Program, "uNormal");

            foreach (Object obj in objects)
            {
                shader.SetUniform(u_ModelMatrix_Handle, obj.Position.transformation * globalTransform.transformation);
                shader.SetUniform(u_NormalMatrix_Handle, Matrix4.Transpose(Matrix4.Invert(obj.Position.transformation * globalTransform.transformation)));

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

            shader.SetUniform("uProjection", projection);
        }

        private void InitLights()
        {
            shader.SetUniform("numLights", lights.Count);

            for (int i = 0; i < lights.Count; i++)
            {
                shader.SetUniform("u_lights[" + i + "].color", lights[i].color);
                shader.SetUniform("u_lights[" + i + "].position", lights[i].pos);
                shader.SetUniform("u_lights[" + i + "].attenuation", lights[i].attenuation);
                shader.SetUniform("u_lights[" + i + "].direction", lights[i].direction);
                shader.SetUniform("u_lights[" + i + "].exponent", lights[i].exp);
                shader.SetUniform("u_lights[" + i + "].cosCutoff", lights[i].cosCutOff);
                shader.SetUniform("u_lights[" + i + "].ambient", lights[i].ambient);
                shader.SetUniform("u_lights[" + i + "].diffuse", lights[i].diffuse);
                shader.SetUniform("u_lights[" + i + "].specular", lights[i].specular);
                shader.SetUniform("u_lights[" + i + "].type", (int)(lights[i].type));
            }
        }

        private void UpdateCamera()
        {
            shader.SetUniform("uView", camera.View);
            shader.SetUniform("u_camera", camera.Pos);
        }
    }
}