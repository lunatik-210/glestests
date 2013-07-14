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

            mesh = new ObjMesh(ctx, "sphere/sphere.obj");
            //mesh = new ObjMesh(ctx, "cube.obj");

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
				GL.Ortho(-1.5f, 1.5f, 1.0f, -1.0f, -1.0f, 1.0f);
			}
			else
			{
				GL.Ortho(-1.0f, 1.0f, -1.5f, 1.5f, -1.0f, 1.0f);
			}
	
			GL.MatrixMode (All.Modelview);
			GL.LoadIdentity ();
			GL.Rotate (rot[0], 1.0f, 0.0f, 0.0f);
			GL.Rotate (rot[1], 0.0f, 1.0f, 0.0f);
			GL.Rotate (rot[2], 0.0f, 1.0f, 0.0f);

			GL.ClearColor (0, 0, 0, 1.0f);
			GL.Clear ((uint) All.ColorBufferBit);

            mesh.Render();

            /*
			//GL.VertexPointer(3, All.Float, 0, cube);

            GL.Color4(0.5f, 0.8f, 0.0f, 1.0f);


            GL.VertexPointer(3, All.Float, 0, cubeVerts);
            GL.NormalPointer(All.Float, 0, cubeNormals);
			GL.EnableClientState (All.VertexArray);
			//GL.ColorPointer (4, All.Float, 0, cubeColors);
            //
            //GL.EnableClientState (All.ColorArray);
			//GL.DrawElements(All.Triangles, 36, All.UnsignedByte, triangles);
            GL.DrawArrays(All.Triangles, 0, cubeNumVerts);
            //GL.VertexPointer(3, All.Float, 0, cubeVerts);
            */

			SwapBuffers ();
		}

		float[] cube = {
			-0.5f, 0.5f, 0.5f, // vertex[0]
			0.5f, 0.5f, 0.5f, // vertex[1]
			0.5f, -0.5f, 0.5f, // vertex[2]
			-0.5f, -0.5f, 0.5f, // vertex[3]
			-0.5f, 0.5f, -0.5f, // vertex[4]
			0.5f, 0.5f, -0.5f, // vertex[5]
			0.5f, -0.5f, -0.5f, // vertex[6]
			-0.5f, -0.5f, -0.5f, // vertex[7]
		};

		byte[] triangles = {
			1, 0, 2, // front
			3, 2, 0,
			6, 4, 5, // back
			4, 6, 7,
			4, 7, 0, // left
			7, 3, 0,
			1, 2, 5, //right
			2, 6, 5,
			0, 1, 5, // top
			0, 5, 4,
			2, 3, 6, // bottom
			3, 7, 6,
		};

		float[] cubeColors = {
			1.0f, 0.0f, 0.0f, 1.0f,
			0.0f, 1.0f, 0.0f, 1.0f,
			0.0f, 0.0f, 1.0f, 1.0f,
			0.0f, 1.0f, 1.0f, 1.0f,
			1.0f, 0.0f, 0.0f, 1.0f,
			0.0f, 1.0f, 0.0f, 1.0f,
			0.0f, 0.0f, 1.0f, 1.0f,
			0.0f, 1.0f, 1.0f, 1.0f,
		};

int cubeNumVerts = 36;

float [] cubeVerts = {
  // f  1//2  7//2  5//2
  -0.5f, -0.5f, -0.5f,
  0.5f, 0.5f, -0.5f,
  0.5f, -0.5f, -0.5f,
  // f  1//2  3//2  7//2 
  -0.5f, -0.5f, -0.5f,
  -0.5f, 0.5f, -0.5f,
  0.5f, 0.5f, -0.5f,
  // f  1//6  4//6  3//6 
  -0.5f, -0.5f, -0.5f,
  -0.5f, 0.5f, 0.5f,
  -0.5f, 0.5f, -0.5f,
  // f  1//6  2//6  4//6 
  -0.5f, -0.5f, -0.5f,
  -0.5f, -0.5f, 0.5f,
  -0.5f, 0.5f, 0.5f,
  // f  3//3  8//3  7//3 
  -0.5f, 0.5f, -0.5f,
  0.5f, 0.5f, 0.5f,
  0.5f, 0.5f, -0.5f,
  // f  3//3  4//3  8//3 
  -0.5f, 0.5f, -0.5f,
  -0.5f, 0.5f, 0.5f,
  0.5f, 0.5f, 0.5f,
  // f  5//5  7//5  8//5 
  0.5f, -0.5f, -0.5f,
  0.5f, 0.5f, -0.5f,
  0.5f, 0.5f, 0.5f,
  // f  5//5  8//5  6//5 
  0.5f, -0.5f, -0.5f,
  0.5f, 0.5f, 0.5f,
  0.5f, -0.5f, 0.5f,
  // f  1//4  5//4  6//4 
  -0.5f, -0.5f, -0.5f,
  0.5f, -0.5f, -0.5f,
  0.5f, -0.5f, 0.5f,
  // f  1//4  6//4  2//4 
  -0.5f, -0.5f, -0.5f,
  0.5f, -0.5f, 0.5f,
  -0.5f, -0.5f, 0.5f,
  // f  2//1  6//1  8//1 
  -0.5f, -0.5f, 0.5f,
  0.5f, -0.5f, 0.5f,
  0.5f, 0.5f, 0.5f,
  // f  2//1  8//1  4//1 
  -0.5f, -0.5f, 0.5f,
  0.5f, 0.5f, 0.5f,
  -0.5f, 0.5f, 0.5f,
};

float [] cubeNormals = {
  // f  1//2  7//2  5//2
  0, 0, -1,
  0, 0, -1,
  0, 0, -1,
  // f  1//2  3//2  7//2 
  0, 0, -1,
  0, 0, -1,
  0, 0, -1,
  // f  1//6  4//6  3//6 
  -1, 0, 0,
  -1, 0, 0,
  -1, 0, 0,
  // f  1//6  2//6  4//6 
  -1, 0, 0,
  -1, 0, 0,
  -1, 0, 0,
  // f  3//3  8//3  7//3 
  0, 1, 0,
  0, 1, 0,
  0, 1, 0,
  // f  3//3  4//3  8//3 
  0, 1, 0,
  0, 1, 0,
  0, 1, 0,
  // f  5//5  7//5  8//5 
  1, 0, 0,
  1, 0, 0,
  1, 0, 0,
  // f  5//5  8//5  6//5 
  1, 0, 0,
  1, 0, 0,
  1, 0, 0,
  // f  1//4  5//4  6//4 
  0, -1, 0,
  0, -1, 0,
  0, -1, 0,
  // f  1//4  6//4  2//4 
  0, -1, 0,
  0, -1, 0,
  0, -1, 0,
  // f  2//1  6//1  8//1 
  0, 0, 1,
  0, 0, 1,
  0, 0, 1,
  // f  2//1  8//1  4//1 
  0, 0, 1,
  0, 0, 1,
  0, 0, 1,
};
	}
}
