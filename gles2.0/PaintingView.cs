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

namespace Mono.Samples.GLTriangle20 {

	class PaintingView : AndroidGameView
	{
		int viewportWidth, viewportHeight;
		int program;
		float [] vertices;
        ObjMesh mesh;

        float[] rot;
        float[] rateOfRotationPS;//degrees

		public PaintingView (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
            mesh = new ObjMesh(context, "sphere.obj");
			Init ();
		}

		public PaintingView (IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
			: base (handle, transfer)
		{
			Init ();
		}

		void Init ()
		{
            rateOfRotationPS = new float[] { 30, 45, 60 };
            rot = new float[] { 0, 0, 0 };
		}

		// This method is called everytime the context needs
		// to be recreated. Use it to set any egl-specific settings
		// prior to context creation
		protected override void CreateFrameBuffer ()
		{
			GLContextVersion = GLContextVersion.Gles2_0;

			// the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
			try {
				Log.Verbose ("GLTriangle", "Loading with default settings");

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}

			// this is a graphics setting that sets everything to the lowest mode possible so
			// the device returns a reliable graphics setting.
			try {
				Log.Verbose ("GLTriangle", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);

				// if you don't call this, the context won't be created
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}

		// This gets called when the drawing surface has been created
		// There is already a GraphicsContext and Surface at this point,
		// following the standard OpenTK/GameWindow logic
		//
		// Android will only render when it refreshes the surface for
		// the first time, so if you don't call Run, you need to hook
		// up the Resize delegate or override the OnResize event to
		// get the updated bounds and re-call your rendering code.
		// This will also allow non-Run-loop code to update the screen
		// when the device is rotated.
		protected override void OnLoad (EventArgs e)
		{
			// This is completely optional and only needed
			// if you've registered delegates for OnLoad
			base.OnLoad (e);

			viewportHeight = Height; viewportWidth = Width;

			// Vertex and fragment shaders
			string vertexShaderSrc = 
                              "uniform mat4 u_modelViewProjectionMatrix; \n" +
                              "attribute vec4 vPosition;    \n" + 
							  "void main()                  \n" +
							  "{                            \n" +
                              "   gl_Position = u_modelViewProjectionMatrix * vPosition;  \n" +
							  "}                            \n";

            string fragmentShaderSrc = 
                                   "precision mediump float;\n" +
		      					   "void main()                                  \n" +
		      					   "{                                            \n" +
		      					   "  gl_FragColor = vec4 (1.0, 0.0, 0.0, 1.0);  \n" +
		      					   "}                                            \n";

            string vertexShaderCode =
                "uniform mat4 u_modelViewProjectionMatrix;" +
                "attribute vec3 a_vertex;" +
                "attribute vec3 a_normal;" +
                "attribute vec4 a_color;" +
                "varying vec3 v_vertex;" +
                "varying vec3 v_normal;" +
                "varying vec4 v_color;" +
                "void main() {" +
                "        v_vertex=a_vertex;" +
                "        vec3 n_normal=normalize(a_normal);" +
                "        v_normal=n_normal;" +
                "        v_color=a_color;" +
                "        gl_Position = u_modelViewProjectionMatrix * vec4(a_vertex,1.0);" +
                "}";

            string fragmentShaderCode =
                "precision mediump float;" +
                "varying vec3 v_vertex;" +
                "varying vec3 v_normal;" +
                "varying vec4 v_color;" +
                "void main() {" +
                "        vec3 n_normal=normalize(v_normal);" +
                "        gl_FragColor = v_color;" +
                "}";

            int vertexShader = LoadShader(All.VertexShader, vertexShaderSrc);
            int fragmentShader = LoadShader(All.FragmentShader, fragmentShaderSrc);
			program = GL.CreateProgram();
			if (program == 0)
				throw new InvalidOperationException ("Unable to create program");

			GL.AttachShader (program, vertexShader);
			GL.AttachShader (program, fragmentShader);

			GL.LinkProgram (program);

			int linked = 0;
			GL.GetProgram (program, All.LinkStatus, ref linked);
			if (linked == 0) {
				// link failed
				int length = 0;
				GL.GetProgram (program, All.InfoLogLength, ref length);
				if (length > 0) {
					var log = new StringBuilder (length);
					GL.GetProgramInfoLog (program, length, ref length, log);
					Log.Debug ("GL2", "Couldn't link program: " + log.ToString ());
				}

				GL.DeleteProgram (program);
				throw new InvalidOperationException ("Unable to link program");
			}

            UpdateFrame += delegate(object sender, FrameEventArgs args)
            {
                // Rotate at a constant speed
                for (int i = 0; i < 3; i++)
                    rot[i] += (float)(rateOfRotationPS[i] * args.Time);
            };

            RenderFrame += delegate
            {
                RenderTriangle();
            };

            Run(30);
		}

		int LoadShader (All type, string source)
		{
			int shader = GL.CreateShader (type);
			if (shader == 0)
				throw new InvalidOperationException ("Unable to create shader");

			int length = 0;
			GL.ShaderSource (shader, 1, new string [] {source}, (int[])null);
			GL.CompileShader (shader);

			int compiled = 0;
			GL.GetShader (shader, All.CompileStatus, ref compiled);
			if (compiled == 0) {
				length = 0;
				GL.GetShader (shader, All.InfoLogLength, ref length);
				if (length > 0) {
					var log = new StringBuilder (length);
					GL.GetShaderInfoLog (shader, length, ref length, log);
					Log.Debug ("GL2", "Couldn't compile shader: " + log.ToString ());
				}

				GL.DeleteShader (shader);
				throw new InvalidOperationException ("Unable to compile shader of type : " + type.ToString ());
			}

			return shader;
		}

		void RenderTriangle ()
		{
    		GL.ClearColor (0.7f, 0.7f, 0.7f, 1);
			GL.Clear((int)All.ColorBufferBit);

			GL.Viewport(0, 0, viewportWidth, viewportHeight);
			GL.UseProgram(program);

            int u_modelViewProjectionMatrix_Handle = GL.GetUniformLocation(program, "u_modelViewProjectionMatrix");
            Matrix4 mat = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), rot[0])
                          * Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), rot[1])
                          * Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), rot[2]);

            float[] modelMatrix = new float[] {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };

            GL.UniformMatrix4(u_modelViewProjectionMatrix_Handle, 1, false, modelMatrix);

            mesh.Render(program);

			SwapBuffers ();
		}

		// this is called whenever android raises the SurfaceChanged event
		protected override void OnResize (EventArgs e)
		{
			viewportHeight = Height;
			viewportWidth = Width;

			// the surface change event makes your context
			// not be current, so be sure to make it current again
			MakeCurrent ();
			RenderTriangle ();
		}
	}
}
