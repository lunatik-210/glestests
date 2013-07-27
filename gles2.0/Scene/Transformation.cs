using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpenTK;

namespace Mono.Samples.GLTriangle20.Scene
{
    class Transformation
    {
        public Matrix4 transformation;
       
        private Vector3 translateVector;
        private Vector3 rotationVector;
        private float scaleFactor;

        public Transformation(Vector3 translation, Vector3 rotation, float scale)
        {
            translateVector = translation;
            rotationVector = rotation;
            scaleFactor = scale;
            UpdateTransformationMatrix();
        }

        public Transformation()
        {
            translateVector = Vector3.Zero;
            rotationVector = Vector3.Zero;
            scaleFactor = 1.0f;
            UpdateTransformationMatrix();
        }

        public float ScaleFactor
        {
            get { return scaleFactor; }
            set { scaleFactor = value; UpdateTransformationMatrix(); }
        }

        public Vector3 TranslateVector
        {
            get { return translateVector; }
            set { translateVector = value; UpdateTransformationMatrix(); }
        }

        public Vector3 RotationVector
        {
            get { return rotationVector; }
            set { rotationVector = value; UpdateTransformationMatrix(); }
        }

        private void UpdateTransformationMatrix()
        {
            Matrix4 translation = Matrix4.CreateTranslation(translateVector);
            Matrix4 rotation = Matrix4.CreateRotationX(rotationVector.X) *
                Matrix4.CreateRotationY(rotationVector.Y) *
                Matrix4.CreateRotationZ(rotationVector.Z);
            Matrix4 scaling = Matrix4.Scale(scaleFactor);
            transformation = scaling * rotation * translation;
        }
    }
}