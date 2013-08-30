
using System;
using OpenTK;

namespace AndroidUI.Scene
{
    class Objects
    {
        static public void InitSpotLight(ref Light light)
        {
            light.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            light.pos = new Vector3(0.0f, 0.0f, 0.0f);
            light.attenuation = new Vector3(0.0f, 0.0f, 0.0f);
            light.direction = new Vector3(0.0f, 0.0f, 0.0f);

            light.ambient = 0.3f;
            light.diffuse = 0.6f;
            light.specular = 0.9f;

            light.exp = 1.0f;
            light.cosCutOff = (float)(Math.Cos(90.0 * Math.PI / 180.0));

            light.type = LightType.SPOT;
        }

        static public void InitPointLight(ref Light light)
        {
            light.color = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            light.pos = new Vector3(0.0f, 0.0f, 0.0f);
            light.attenuation = new Vector3(0.0f, 0.0f, 0.0f);
            light.direction = new Vector3(0.0f, 0.0f, 0.0f);

            light.ambient = 0.1f;
            light.diffuse = 0.2f;
            light.specular = 0.5f;

            light.exp = 0.0f;
            light.cosCutOff = 0.0f;

            light.type = LightType.POINT;
        }
    }

    public enum LightType
    {
        POINT = 1,
        SPOT = 2
    }

    public struct Light
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
        public LightType type;
    }
}