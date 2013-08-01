using System;
using System.Runtime.InteropServices;
using System.Text;

using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Util;
using Android.Views;
using Android.Content;
using OpenTK;

// Render a triangle using OpenGLES 2.0

using AndroidUI.Scene;

namespace AndroidUI {

    class PaintingView : AndroidGameView, ScaleGestureDetector.IOnScaleGestureListener
	{
        private int viewportWidth, viewportHeight;

        private Scene.Scene scene;
        private Shader shader;

        float prevx;
        float prevy;

        private ScaleGestureDetector mScaleDetector;

        private string vertexShaderCode =
            "uniform mat4 uModel;" +
            "uniform mat4 uView;" +
            "uniform mat4 uProjection;" +
            "uniform mat4 uNormal;" +
            "attribute vec3 a_vertex;" +
            "attribute vec3 a_normal;" +
            "varying vec3 v_vertex;" +
            "varying vec3 v_normal;" +
            "void main() {" +
            "        vec4 vertex = uModel * vec4(a_vertex, 1.0);" +
            "        v_vertex=vertex.xyz;" +
            "        vec3 n_normal=normalize(a_normal);" +
            "        v_normal=(uNormal*vec4(n_normal,1.0)).xyz;" +
            "        gl_Position = uProjection * uView * vertex;" +
            "}";

        private string fragmentShaderCode =
            "precision mediump float;" +
            "const int MAX_LIGHTS = 8;" +
            "struct Light " +
            "{" +
            "    vec3 position;" +
            "    vec4 color;" +
            "};" +
            "uniform Light u_lights[MAX_LIGHTS];" +
            "uniform int numLights;" +
            "uniform vec3 u_camera;" +
            "varying vec3 v_vertex;" +
            "varying vec3 v_normal;" +
            "void main() {" +
            "        vec3 n_normal=normalize(v_normal);" +
            "        vec3 lightvector;" +
            "        vec3 lookvector = normalize( u_camera - v_vertex );" +
            "        float ambient = 0.2;" +
            "        float k_diffuse = 0.8;" +
            "        float k_specular = 0.8;" +
            "        vec4 final_color = vec4(0.0, 0.0, 0.0, 0.0);" +
            "        float diffuse;" +
            "        vec3 reflectvector;" +
            "        float specular;" +
            "        for(int i=0; i<MAX_LIGHTS; i++) {" +
            "            if( i >= numLights )" +
            "                break;" +
            "            lightvector = normalize( u_lights[i].position - v_vertex );" +
            "            diffuse = k_diffuse * max(dot(n_normal, lightvector), 0.0);" +
            "            reflectvector = reflect(-lightvector, n_normal);" +
            "            specular = k_specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);" +
            "            final_color += (ambient+diffuse+specular)*u_lights[i].color;" +
            "        }" +
            "        gl_FragColor = final_color;" +
            "}";

		public PaintingView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Init ();
		}

		public PaintingView (IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
			Init ();
		}

		void Init ()
		{

		}

		protected override void CreateFrameBuffer ()
		{
			GLContextVersion = GLContextVersion.Gles2_0;

			try {
				Log.Verbose ("AndroidUI", "Loading with default settings");
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
                Log.Verbose("AndroidUI", "{0}", ex);
			}

			try {
                Log.Verbose("AndroidUI", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
                Log.Verbose("AndroidUI", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			viewportHeight = Height; viewportWidth = Width;

            shader = new Shader(vertexShaderCode, fragmentShaderCode);

            ObjMesh sphere = new ObjMesh(Context, "triad_sphere.obj");

            scene = new Scene.Scene(shader);
            scene.Cam = new Scene.Camera(new Vector3(0.0f, 0.0f, 10.0f));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, 5.0f, 4.0f), new Vector4(0.0f, 0.5f, 1.0f, 1.0f)));
            scene.appendLight(new Scene.Light(new Vector3(5.0f, 5.0f, 4.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, -5.0f, 4.0f), new Vector4(1.0f, 0.5f, 0.0f, 1.0f)));

            float d = 2.0f;

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    for (int k = -1; k < 2; ++k)
                    {
                        Scene.Object obj = new Scene.Object()
                        {
                            Position = new Transformation()
                            {
                                TranslateVector = new Vector3(d * i, d * j, d * k),
                                ScaleFactor = 0.8f
                            },
                            Mesh = sphere,
                        };
                        scene.appendObject(obj);
                    }
                }
            }

            mScaleDetector = new ScaleGestureDetector(Context, this);

            //UpdateFrame += delegate(object sender, FrameEventArgs args)
            //{
            //};

            RenderFrame += delegate
            {
                Render();
            };

            scene.init();
            scene.UpdateProjection(Width, Height);
           
            Run(30);
		}   

        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);
            mScaleDetector.OnTouchEvent(e);
            if (mScaleDetector.IsInProgress)
            {
                return true;
            }

            if (e.Action == MotionEventActions.Down)
            {
                prevx = e.GetX();
                prevy = e.GetY();
            }
            if (e.Action == MotionEventActions.Move)
            {
                float e_x = e.GetX();
                float e_y = e.GetY();

                scene.UpdateSceneOrientation((prevx - e_x), (prevy - e_y));

                prevx = e_x;
                prevy = e_y;
            }
            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
                Render();
            return true;
        }

		private void Render()
		{
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1);
            GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));

            GL.Viewport(0, 0, viewportWidth, viewportHeight);

            scene.render();

			SwapBuffers();
		}

		protected override void OnResize (EventArgs e)
		{
			viewportHeight = Height;
			viewportWidth = Width;

            scene.UpdateProjection(Width, Height);

			MakeCurrent();
			Render();
		}

        #region IOnScaleGestureListener Members

        public bool OnScale(ScaleGestureDetector detector)
        {
            scene.ScaleCamera(1.0f/detector.ScaleFactor);
            return true;
        }

        public bool OnScaleBegin(ScaleGestureDetector detector)
        {
            return true;
        }

        public void OnScaleEnd(ScaleGestureDetector detector)
        {
        }

        #endregion
    }
}
