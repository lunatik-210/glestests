using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;
using OpenTK;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Runtime.InteropServices;

namespace Mono.Samples.GLTriangle20
{
    class ObjMesh
    {
        public ObjMesh(Context context, string fileName)
        {
            ObjMeshLoader.Load(context, this, fileName);
        }

        public ObjVertex[] Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }
        ObjVertex[] vertices;

        public ObjTriangle[] Triangles
        {
            get { return triangles; }
            set { triangles = value; }
        }
        ObjTriangle[] triangles;

        public ObjQuad[] Quads
        {
            get { return quads; }
            set { quads = value; }
        }
        ObjQuad[] quads;

        int verticesBufferId;
        int trianglesBufferId;
        int quadsBufferId;

        public void Prepare()
        {
            if (verticesBufferId == 0)
            {
                GL.GenBuffers(1, ref verticesBufferId);
                GL.BindBuffer(All.ArrayBuffer, verticesBufferId);
                GL.BufferData(All.ArrayBuffer, (IntPtr)(vertices.Length * Marshal.SizeOf(typeof(ObjVertex))), vertices, All.StaticDraw);
            }

            if (trianglesBufferId == 0)
            {
                GL.GenBuffers(1, ref trianglesBufferId);
                GL.BindBuffer(All.ElementArrayBuffer, trianglesBufferId);
                GL.BufferData(All.ElementArrayBuffer, (IntPtr)(triangles.Length * Marshal.SizeOf(typeof(ObjTriangle))), triangles, All.StaticDraw);
            }

            if (quadsBufferId == 0)
            {
                GL.GenBuffers(1, ref quadsBufferId);
                GL.BindBuffer(All.ElementArrayBuffer, quadsBufferId);
                GL.BufferData(All.ElementArrayBuffer, (IntPtr)(quads.Length * Marshal.SizeOf(typeof(ObjQuad))), quads, All.StaticDraw);
            }
        }

        public void Render(int program_Handle )
        {
            Prepare();

            int a_vertex_Handle = GL.GetAttribLocation(program_Handle, "a_vertex");
            int a_normal_Handle = GL.GetAttribLocation(program_Handle, "a_normal");

            GL.BindBuffer(All.ArrayBuffer, verticesBufferId);
            GL.VertexAttribPointer(a_vertex_Handle, 3, All.Float, true, Marshal.SizeOf(typeof(ObjVertex)), Marshal.OffsetOf(typeof(ObjVertex), "Vertex"));
            GL.EnableVertexAttribArray(a_vertex_Handle);

            GL.BindBuffer(All.ArrayBuffer, verticesBufferId);
            GL.VertexAttribPointer(a_normal_Handle, 3, All.Float, true, Marshal.SizeOf(typeof(ObjVertex)), Marshal.OffsetOf(typeof(ObjVertex), "Normal"));
            GL.EnableVertexAttribArray(a_normal_Handle);

            GL.BindBuffer(All.ElementArrayBuffer, trianglesBufferId);
            GL.DrawElements(All.Triangles, triangles.Length * 3, All.UnsignedShort, IntPtr.Zero);

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjVertex
        {
            public Vector2 TexCoord;
            public Vector3 Normal;
            public Vector3 Vertex;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjTriangle
        {
            public short Index0;
            public short Index1;
            public short Index2;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ObjQuad
        {
            public short Index0;
            public short Index1;
            public short Index2;
            public short Index3;
        }
    }
}