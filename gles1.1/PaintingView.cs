using System;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Platform;
using OpenTK.Platform.Android;

using Android.Views;
using Android.Util;
using Android.Content;
using OpenTK.Audio;

namespace Mono.Samples.GLCube {

	class PaintingView : AndroidGameView
	{
		float [] rot;
		float [] rateOfRotationPS;//degrees
		int viewportWidth, viewportHeight;
        Context ctx;
        ObjMesh mesh;

		public PaintingView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
            ctx = context;
		}

		public PaintingView (IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
			Initialize ();
		}

		private void Initialize ()
		{
			rateOfRotationPS = new float [] { 30, 45, 60 };
			rot = new float [] { 0, 0, 0};
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		protected override void CreateFrameBuffer ()
		{
			GLContextVersion = GLContextVersion.Gles1_1;

			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try {
				Log.Verbose ("GLCube", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("GLCube", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLCube", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}


		// This gets called when the drawing surface is ready
		protected override void OnLoad (EventArgs e)
		{
			// this call is optional, and meant to raise delegates
			// in case any are registered
			base.OnLoad (e);

			// UpdateFrame and RenderFrame are called
			// by the render loop. This is takes effect
			// when we use 'Run ()', like below
			UpdateFrame += delegate (object sender, FrameEventArgs args) {
				// Rotate at a constant speed
				for (int i = 0; i < 3; i ++)
					rot [i] += (float) (rateOfRotationPS [i] * args.Time);
			};

			RenderFrame += delegate {
				RenderCube ();
			};
			
			GL.Enable(All.CullFace);
			GL.ShadeModel(All.Smooth);
			
			GL.Hint(All.PerspectiveCorrectionHint, All.Nicest);

            //mesh = new ObjMesh(ctx, "sphere/sphere.obj");
            mesh = new ObjMesh(ctx, "cube.obj");

			// Run the render loop
			Run (30);
		}

		// this occurs mostly on rotation.
		protected override void OnResize (EventArgs e)
		{
			viewportWidth = Width;
			viewportHeight = Height;
		}

		void RenderCube ()
		{
			GL.Viewport(0, 0, viewportWidth, viewportHeight);
			
			GL.MatrixMode (All.Projection);
			GL.LoadIdentity ();
			
			if ( viewportWidth > viewportHeight )
			{
				GL.Ortho(-2.5f, 2.5f, 2.0f, -2.0f, -2.0f, 2.0f);
			}
			else
			{
				GL.Ortho(-2.0f, 2.0f, -2.5f, 2.5f, -2.0f, 2.0f);
			}
	
			GL.MatrixMode (All.Modelview);
			GL.LoadIdentity ();

			GL.ClearColor (0, 0, 0, 1.0f);
			GL.Clear ((uint) All.ColorBufferBit);

            GL.Enable(All.Lighting);
            //GL.Enable(All.LightModelAmbient);
            //GL.Enable(All.ColorMaterial);
            GL.Enable(All.Light0);
            GL.Enable(All.CullFace);
            GL.Enable(All.Alpha);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);

            float [] lightp = {-2.0f, -2.0f, -2.0f};
            
            float [] ambientlightArray = { 0.5f, 0.5f, 0.5f, 1 };
            float [] diffuselightArray = { 0.8f, 0.8f, 0.8f, 1 };
            float [] specularlightArray = { 0.8f, 0.8f, 0.8f, 1 };
            
            float [] ambientmaterial = {0.6f, 0.2f, 0.2f, 1f};
            float [] diffusematerial = {0.2f, 0.5f, 0.8f, 1f};
            float [] specularmaterial = {0.2f, 0.5f, 0.9f, 1f};

            GL.Light(All.Light0, All.Position, lightp);
            GL.Light(All.Light0, All.Ambient, ambientlightArray);
            GL.Light(All.Light0, All.Diffuse, diffuselightArray);
            GL.Light(All.Light0, All.Specular, specularlightArray);

            GL.Material(All.FrontAndBack, All.Ambient, ambientmaterial);
            GL.Material(All.FrontAndBack, All.Diffuse, diffusematerial);
            GL.Material(All.FrontAndBack, All.Specular, specularmaterial);

            

            //GL.Material(All.FrontAndBack, All.Shininess, 128.0f);         

            GL.PushMatrix();

            GL.Rotate(rot[0], 1.0f, 0.0f, 0.0f);
            GL.Rotate(rot[1], 0.0f, 1.0f, 0.0f);
            GL.Rotate(rot[2], 0.0f, 1.0f, 0.0f);

            mesh.Render();

            GL.PopMatrix();
            //GL.Disable(All.Lighting);

			SwapBuffers ();
		}
	}
}
