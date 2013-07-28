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

	class PaintingView : AndroidGameView
	{
		int viewportWidth, viewportHeight;
		int program;
        ObjMesh mesh;
        Scene.Scene scene;

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

            scene = new Scene.Scene();
            scene.Cam = new Scene.Camera(new Vector3(0.0f, 0.0f, 10.0f));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, 5.0f, 4.0f), new Vector4(0.0f, 0.5f, 1.0f, 1.0f)));
            scene.appendLight(new Scene.Light(new Vector3(5.0f, 5.0f, 4.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));
            scene.appendLight(new Scene.Light(new Vector3(-5.0f, -5.0f, 4.0f), new Vector4(1.0f, 0.5f, 0.0f, 1.0f)));

            float d = 2.0f;

            for (int i = -1; i <2; ++i)
            {
                for (int j = -1; j <2; ++j)
                {
                    for (int k = -1; k <2; ++k)
                    {
                        Scene.Object obj = new Scene.Object();
                        obj.Transform = new Transformation()
                        {
                            TranslateVector = new Vector3(d*i, d*j, d*k),
                            ScaleFactor = 0.8f
                        };
                        obj.Mesh = mesh;
                        scene.appendObject(obj);
                    }
                }                
            }

		}

		protected override void CreateFrameBuffer ()
		{
			GLContextVersion = GLContextVersion.Gles2_0;

			try {
				Log.Verbose ("GLTriangle", "Loading with default settings");
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}

			try {
				Log.Verbose ("GLTriangle", "Loading with custom Android settings (low mode)");
				GraphicsMode = new AndroidGraphicsMode (0, 0, 0, 0, 0, false);
				base.CreateFrameBuffer ();
				return;
			} catch (Exception ex) {
				Log.Verbose ("GLTriangle", "{0}", ex);
			}
			throw new Exception ("Can't load egl, aborting");
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			viewportHeight = Height; viewportWidth = Width;

            string vertexShaderCode =
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

            string fragmentShaderCode =
                "precision mediump float;" +
                "const int MAX_LIGHTS = 8;" +
                "struct Light " +
                "{" +
                "    vec3 position;" +
                "    vec4 color;" +
                "};" +
                "uniform Light u_lights[MAX_LIGHTS];" +
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
                "            lightvector = normalize( u_lights[i].position - v_vertex );" +
                "            diffuse = k_diffuse * max(dot(n_normal, lightvector), 0.0);" +
                "            reflectvector = reflect(-lightvector, n_normal);" +
                "            specular = k_specular * pow(max(dot(lookvector, reflectvector), 0.0), 40.0);" +
                "            final_color += (ambient+diffuse+specular)*u_lights[i].color;" +
                "        }" +
                "        gl_FragColor = final_color;" +
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

            UpdateFrame += delegate(object sender, FrameEventArgs args)
            {
                for (int i = 0; i < 3; i++)
                    rot[i] += (float)(rateOfRotationPS[i] * args.Time);
            };

            RenderFrame += delegate
            {
                Render();
            };
            GL.UseProgram(program);
            scene.Width = viewportWidth;
            scene.Height = viewportHeight;
            scene.init(program);
           
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

        float prevx;
        float prevy;

        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);
            if (e.Action == MotionEventActions.Down)
            {
                prevx = e.GetX();
                prevy = e.GetY();
            }
            if (e.Action == MotionEventActions.Move)
            {
                float e_x = e.GetX();
                float e_y = e.GetY();

                float xdiff = (prevx - e_x);
                float ydiff = (prevy - e_y);
                scene.Update(xdiff, ydiff);
                prevx = e_x;
                prevy = e_y;
            }
            if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
                Render();
            return true;
        }

		void Render()
		{
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1);
            GL.Clear((int)(All.ColorBufferBit | All.DepthBufferBit));

            GL.Viewport(0, 0, viewportWidth, viewportHeight);
            GL.UseProgram(program);

            GL.Enable(All.DepthTest);
            GL.Enable(All.CullFace);
            GL.CullFace(All.Back);
            GL.Hint(All.GenerateMipmapHint, All.Nicest);

            scene.render(program);

			SwapBuffers();
		}


		protected override void OnResize (EventArgs e)
		{
			viewportHeight = Height;
			viewportWidth = Width;

            scene.Width = viewportWidth;
            scene.Height = viewportHeight;

			MakeCurrent ();
			Render();
		}
	}
}
