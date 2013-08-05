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
using System.IO;

namespace AndroidUI {

    class PaintingView : AndroidGameView, ScaleGestureDetector.IOnScaleGestureListener
	{
        private int viewportWidth, viewportHeight;

        private Scene.Scene scene;
        private Shader shader;

        float prevx;
        float prevy;

        private ScaleGestureDetector mScaleDetector;

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

            string vertexShaderCode = null;
            string fragmentShaderCode = null;

            // try catch is needed here (see ObjMeshLoader.Load(Context,filename))
            // Tools is greate place for such a function like load text i think
            using (var input = Context.Assets.Open("Shaders/sh.vs"))
            using (StreamReader streamReader = new StreamReader(input))
            {
                vertexShaderCode = streamReader.ReadToEnd();
                streamReader.Close();
            }

            using (var input = Context.Assets.Open("Shaders/sh.fs"))
            using (StreamReader streamReader = new StreamReader(input))
            {
                fragmentShaderCode = streamReader.ReadToEnd();
                streamReader.Close();
            }
            //////////////////////////////////////////////////////////////////////

            shader = new Shader(vertexShaderCode, fragmentShaderCode);

            ObjMesh sphere = new ObjMesh(Context, "triad_sphere.obj");

            scene = new Scene.Scene(shader);
            scene.Cam = new Scene.Camera(new Vector3(0.0f, 0.0f, 10.0f));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, 5.0f, 4.0f), new Vector4(0.0f, 0.5f, 1.0f, 1.0f), new Vector3(0.5f, 0.0f, 0.02f)));
            scene.appendLight(new Scene.Light(new Vector3(5.0f, 5.0f, 4.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f), new Vector3(0.5f, 0.0f, 0.02f)));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, -5.0f, 4.0f), new Vector4(1.0f, 0.5f, 0.0f, 1.0f), new Vector3(0.5f, 0.0f, 0.02f)));

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
