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
            rateOfRotationPS = new float[] { 1, 2, 3 };
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

            string vertexShaderCode =
                "uniform mat4 u_modelViewProjectionMatrix;" +
                "attribute vec3 a_vertex;" +
                "attribute vec3 a_normal;" +
                "varying vec3 v_vertex;" +
                "varying vec3 v_normal;" +
                "void main() {" +
                "        v_vertex=a_vertex;" +
                "        vec3 n_normal=normalize(a_normal);" +
                "        v_normal=n_normal;" +
                "        gl_Position = u_modelViewProjectionMatrix * vec4(a_vertex,1.0);" +
                "}";

            string fragmentShaderCode =
                "precision mediump float;" +
                "uniform vec3 u_lightPosition;" +
                "uniform vec3 u_camera;" +
                "varying vec3 v_vertex;" +
                "varying vec3 v_normal;" +
                "void main() {" +
                "        vec3 n_normal=normalize(v_normal);" +
                "        vec3 lightvector = normalize(u_lightPosition - v_vertex);" +
                "        vec3 lookvector = normalize(u_camera - v_vertex);" +
                "        float ambient = 0.2;" +
                "        float k_diffuse = 0.8;" +
                "        float k_specular = 0.4;" +
                "        float diffuse = k_diffuse * max(dot(n_normal, lightvector), 0.0);" +
                "        vec3 reflectvector = reflect(-lightvector, n_normal);" +
                "        float specular = k_specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);" +
                "        vec4 color = vec4(1.0, 0.5, 0.0, 1.0);" +
                "        gl_FragColor = (ambient+diffuse+specular)*color;" +
                "}";

            int vertexShader = LoadShader(All.VertexShader, vertexShaderCode);
            int fragmentShader = LoadShader(All.FragmentShader, fragmentShaderCode);
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

            //GL.Enable(All.DepthTest);
            GL.Enable(All.CullFace);
            GL.Hint(All.GenerateMipmapHint, All.Nicest);

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

		void RenderTriangle()
		{
            GL.ClearColor(0.7f, 0.7f, 0.7f, 1);
            GL.Clear((int)All.ColorBufferBit);

            GL.Viewport(0, 0, viewportWidth, viewportHeight);
            GL.UseProgram(program);

            Vector3 cameraPos = new Vector3(0.0f, 0.0f, 6.0f);
            LinkModelViewProjectionMatrix(cameraPos.X, cameraPos.Y, cameraPos.Z);
            LinkVector3(0.0f, 0.6f, 3.0f, "u_lightPosition");
            LinkVector3(cameraPos.X, cameraPos.Y, cameraPos.Z, "u_camera");

            mesh.Render(program);

			SwapBuffers ();
		}

        void LinkModelViewProjectionMatrix(float x, float y, float z)
        {
            int u_modelViewProjectionMatrix_Handle = GL.GetUniformLocation(program, "u_modelViewProjectionMatrix");

            Matrix4 view = Matrix4.LookAt(new Vector3(x, y, z), new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));

            float ratio = (float) viewportWidth / viewportHeight;
            float k=0.055f;
            float left = -k*ratio;
            float right = k*ratio;
            float bottom = -k;
            float top = k;
            float near = 0.1f;
            float far = 10.0f;
            Matrix4 projection = Matrix4.CreatePerspectiveOffCenter(left, right, bottom, top, near, far);

            Matrix4 model = Matrix4.CreateFromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), rot[0])
                          * Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), rot[1])
                          * Matrix4.CreateFromAxisAngle(new Vector3(0.0f, 0.0f, 1.0f), rot[2]);

            Matrix4 mat = model * view * projection;

            float[] modeViewlProjectionMatrix = new float[] {
                mat.M11, mat.M12, mat.M13, mat.M14,
                mat.M21, mat.M22, mat.M23, mat.M24,
                mat.M31, mat.M32, mat.M33, mat.M34,
                mat.M41, mat.M42, mat.M43, mat.M44
            };
            GL.UniformMatrix4(u_modelViewProjectionMatrix_Handle, 1, false, modeViewlProjectionMatrix);
        }

        void LinkVector3(float x, float y, float z, string variable)
        {
            int handle = GL.GetUniformLocation(program, variable);
            GL.Uniform3(handle, x, y, z);
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
