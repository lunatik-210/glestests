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
using Skweez.Filetools;
using Android.Widget;

namespace AndroidUI {
    class PaintingView : AndroidGameView, ScaleGestureDetector.IOnScaleGestureListener
	{
        private int viewportWidth, viewportHeight;

        private Scene.Scene scene;
        private Shader spotShader, depthShader;

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

            try {
                vertexShaderCode = FileTools.getContentByStream(Context.Assets.Open("Shaders/Spot/vs.glsl"));
                fragmentShaderCode = FileTools.getContentByStream(Context.Assets.Open("Shaders/Spot/fs.glsl"));
                spotShader = new Shader(vertexShaderCode, fragmentShaderCode);

                vertexShaderCode = FileTools.getContentByStream(Context.Assets.Open("Shaders/Depth/vs.glsl"));
                fragmentShaderCode = FileTools.getContentByStream(Context.Assets.Open("Shaders/Depth/fs.glsl"));
                depthShader = new Shader(vertexShaderCode, fragmentShaderCode);
            }
            catch(Exception ex) {
                throw new Exception("Can't load shaders from file: {0}", ex);
            }

            //////////////////////////////////////////////////////////////////////

            ObjMesh sphere = new ObjMesh(Context, "triad_sphere.obj");

            scene = new Scene.Scene(spotShader);
            scene.Cam = new Scene.Camera(new Vector3(0.0f, 0.0f, 10.0f));
            Vector3 lightAtt = new Vector3(1.0f, 0.00f, 0.02f);
            
            Light light;
            
            light = new Light();
            Objects.InitSpotLight(ref light);
            light.direction = new Vector3(1.0f, 0.0f, 0.0f);
            light.color = new Vector4(0.0f, 0.5f, 1.0f, 1.0f);
            light.attenuation = lightAtt;
            scene.appendLight(light);

            light = new Light();
            Objects.InitSpotLight(ref light);
            light.direction = new Vector3(-1.0f, 0.0f, 0.0f);
            light.color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            light.attenuation = lightAtt;
            scene.appendLight(light);

            light = new Light();
            Objects.InitSpotLight(ref light);
            light.direction = new Vector3(0.0f, 0.0f, 1.0f);
            light.color = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
            light.attenuation = lightAtt;
            scene.appendLight(light);

            light = new Light();
            Objects.InitPointLight(ref light);
            light.pos = new Vector3(0.0f, 6.0f, 6.0f);
            light.color = new Vector4(0.0f, 0.75f, 0.0f, 1.0f);
            light.attenuation = lightAtt;
            scene.appendLight(light);

            float d = 2.0f;
            int id = 0;

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    for (int k = -1; k < 2; ++k)
                    {
                        if (i == 0 && j == 0 && k == 0) continue;
                        Scene.Object obj = new Scene.Object()
                        {
                            Position = new Transformation()
                            {
                                TranslateVector = new Vector3(d * i, d * j, d * k),
                                ScaleFactor = 0.8f
                            },
                            Mesh = sphere,
                            Id = id++,
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

            scene.setViewport(Width, Height);
            scene.init();

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
          
            if (e.Action == MotionEventActions.Up)
            {
                if ( Math.Abs( e.DownTime - e.EventTime ) < 120)
                {
                    Toast.MakeText(Context, "You've triggered ball with id = " + GetTappedObjectID((int)(e.GetX()), (int)(e.GetY())), ToastLength.Short).Show();
                }
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

        byte GetTappedObjectID(int x, int y)
        {
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1);
            GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));
            GL.Viewport(0, 0, viewportWidth, viewportHeight);

            scene.SetShader(depthShader);

            scene.render();

            Byte[] id = new Byte[4];
            GL.ReadPixels((int)(x), viewportHeight - (int)(y) - 1, 1, 1, All.Rgba, All.UnsignedByte, id);

            scene.SetShader(spotShader);
            return id[0];
        }

		private void Render()
		{
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));
            GL.Viewport(0, 0, viewportWidth, viewportHeight);

            scene.render();

			SwapBuffers();
		}

		protected override void OnResize (EventArgs e)
		{
			viewportHeight = Height;
			viewportWidth = Width;

            scene.setViewport(Width, Height);

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
