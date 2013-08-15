
using System;
using OpenTK;

namespace AndroidUI.Scene
{
    class Objects
    {
        static public void InitSpotLight(ref SpotLight light)
        {
            light.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            light.pos = new Vector3(0.0f, 0.0f, 0.0f);
            light.attenuation = new Vector3(0.0f, 0.0f, 0.0f);
            light.direction = new Vector3(0.0f, 0.0f, 0.0f);

            light.ambient = 0.4f;
            light.diffuse = 0.6f;
            light.specular = 0.8f;

            light.exp = 4.5f;
            light.cosCutOff = (float)(Math.Cos(55.0 * Math.PI / 180.0));
        }
    }

    public struct SpotLight
    {
        public Vector4 color;
        public Vector3 pos;
        public Vector3 attenuation;
        public Vector3 direction;
        public float exp;
        public float cosCutOff;
        public float ambient;
        public float diffuse;
        public float specular;
    }
}