using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using System.Globalization;

namespace Mono.Samples.GLCube
{
    class ObjMeshLoader
    {
        public static bool Load(Context context, ObjMesh mesh, string fileName)
        {
            try
            {
                using (var input = context.Assets.Open(fileName))
                using (StreamReader streamReader = new StreamReader(input))
                {
                    Load(mesh, streamReader);
                    streamReader.Close();
                    return true;
                }
            }
            catch { return false; }
        }

        static char[] splitCharacters = new char[] { ' ' };

        static List<Vector3> vertices;
        static List<Vector3> normals;
        static List<Vector2> texCoords;
        static Dictionary<ObjMesh.ObjVertex, short> objVerticesIndexDictionary;
        static List<ObjMesh.ObjVertex> objVertices;
        static List<ObjMesh.ObjTriangle> objTriangles;
        static List<ObjMesh.ObjQuad> objQuads;

        static void Load(ObjMesh mesh, TextReader textReader)
        {
            vertices = new List<Vector3>();
            normals = new List<Vector3>();
            texCoords = new List<Vector2>();
            objVerticesIndexDictionary = new Dictionary<ObjMesh.ObjVertex, short>();
            objVertices = new List<ObjMesh.ObjVertex>();
            objTriangles = new List<ObjMesh.ObjTriangle>();
            objQuads = new List<ObjMesh.ObjQuad>();

            string line;
            while ((line = textReader.ReadLine()) != null)
            {
                line = line.Trim(splitCharacters);
                line = line.Replace("  ", " ");

                string[] parameters = line.Split(splitCharacters);

                switch (parameters[0])
                {
                    case "p": // Point
                        break;

                    case "g":
                        break;

                    case "v": // Vertex
                        float x = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(parameters[3], CultureInfo.InvariantCulture.NumberFormat);
                        vertices.Add(new Vector3(x, y, z));
                        break;

                    case "vt": // TexCoord
                        float u = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float v = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        texCoords.Add(new Vector2(u, v));
                        break;

                    case "vn": // Normal
                        float nx = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float ny = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        float nz = float.Parse(parameters[3], CultureInfo.InvariantCulture.NumberFormat);
                        normals.Add(new Vector3(nx, ny, nz));
                        break;

                    case "f":
                        switch (parameters.Length)
                        {
                            case 4:
                                ObjMesh.ObjTriangle objTriangle = new ObjMesh.ObjTriangle();
                                objTriangle.Index0 = ParseFaceParameter(parameters[1]);
                                objTriangle.Index1 = ParseFaceParameter(parameters[2]);
                                objTriangle.Index2 = ParseFaceParameter(parameters[3]);
                                objTriangles.Add(objTriangle);
                                break;

                            case 5:
                                ObjMesh.ObjQuad objQuad = new ObjMesh.ObjQuad();
                                objQuad.Index0 = ParseFaceParameter(parameters[1]);
                                objQuad.Index1 = ParseFaceParameter(parameters[2]);
                                objQuad.Index2 = ParseFaceParameter(parameters[3]);
                                objQuad.Index3 = ParseFaceParameter(parameters[4]);
                                objQuads.Add(objQuad);
                                break;
                        }
                        break;
                }
            }

            mesh.Vertices = objVertices.ToArray();
            mesh.Triangles = objTriangles.ToArray();
            mesh.Quads = objQuads.ToArray();

            objVerticesIndexDictionary = null;
            vertices = null;
            normals = null;
            texCoords = null;
            objVertices = null;
            objTriangles = null;
            objQuads = null;
        }

        static char[] faceParamaterSplitter = new char[] { '/' };
        static short ParseFaceParameter(string faceParameter)
        {
            Vector3 vertex = new Vector3();
            Vector2 texCoord = new Vector2();
            Vector3 normal = new Vector3();

            string[] parameters = faceParameter.Split(faceParamaterSplitter);

            short vertexIndex = 0;
            short.TryParse(parameters[0], out vertexIndex);
            if (vertexIndex < 0) vertexIndex = (short)(vertices.Count + vertexIndex);
            else vertexIndex = (short)(vertexIndex - 1);
            vertex = vertices[vertexIndex];

            if (parameters.Length > 1)
            {
                short texCoordIndex = 0;
                short.TryParse(parameters[1], out texCoordIndex);
                if (texCoordIndex < 0) texCoordIndex = (short)(texCoords.Count + texCoordIndex);
                else texCoordIndex = (short)(texCoordIndex - 1);
                texCoord = texCoords[texCoordIndex];
            }

            if (parameters.Length > 2)
            {
                int normalIndex = 0;
                int.TryParse(parameters[2], out normalIndex);
                if (normalIndex < 0) normalIndex = normals.Count + normalIndex;
                else normalIndex = normalIndex - 1;
                normal = normals[normalIndex];
            }

            return FindOrAddObjVertex(ref vertex, ref texCoord, ref normal);
        }

        static short FindOrAddObjVertex(ref Vector3 vertex, ref Vector2 texCoord, ref Vector3 normal)
        {
            ObjMesh.ObjVertex newObjVertex = new ObjMesh.ObjVertex();
            newObjVertex.Vertex = vertex;
            newObjVertex.TexCoord = texCoord;
            newObjVertex.Normal = normal;

            short index;
            if (objVerticesIndexDictionary.TryGetValue(newObjVertex, out index))
            {
                return index;
            }
            else
            {
                objVertices.Add(newObjVertex);
                objVerticesIndexDictionary[newObjVertex] = (short)(objVertices.Count - 1);
                return (short)(objVertices.Count - 1);
            }
        }
    }
}