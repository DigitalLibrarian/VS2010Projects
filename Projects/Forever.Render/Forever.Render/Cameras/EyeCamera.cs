using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Forever.Render;

namespace Forever.Render.Cameras
{
    /// <summary>
    /// Basic Camera with iCamera handles.  You are the EYE
    /// </summary>
    public class EyeCamera : ICamera
    {
        protected float FOV = MathHelper.Pi / 3;
        protected float aspectRatio = 1;
        protected float nearClip =  0.01f;
        protected float farClip = 10000000.0f;
     
     
        protected Vector3 cameraPosition;

        public Quaternion Rotation { get; set; }
     
        public Vector3 Position { get { return this.cameraPosition; }  set { this.cameraPosition = value; } }
     
        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(this.FOV, this.aspectRatio, this.nearClip, this.farClip);
            }
     
        }
     
        public Matrix View
        {
            get
            {


                return Matrix.Invert(Matrix.CreateFromQuaternion(this.Rotation) * Matrix.CreateTranslation(this.Position));
            }
        }
     
     
     
        public EyeCamera()
        {
           this.cameraPosition = Vector3.Zero;
           this.Rotation = Quaternion.Identity;
        }

        /// <summary>
        /// Rotate on the local coordinate system standard axes by this yaw, pitch, and roll
        /// </summary>
        /// <param name="yaw">Rotation dun on the y axis</param>
        /// <param name="pitch">Rotation dun on the x axis</param>
        /// <param name="roll">Rotation dun on the z axis</param>
        public void Rotate(float yaw, float pitch, float roll)
        {
            Vector3 up = new Vector3(0, 1, 0);
            Vector3 right = new Vector3(1, 0, 0);
            Vector3 forward = new Vector3(0, 0, 1);
            
            Quaternion q1 = Quaternion.CreateFromAxisAngle(up, yaw);
            Quaternion q2 = Quaternion.CreateFromAxisAngle(right, pitch);
            Quaternion q3 = Quaternion.CreateFromAxisAngle(forward, roll);
            Rotation = q1 * q2 * q3 * Rotation;
            //cameraRotation = Matrix.CreateFromQuaternion(q1 * q2 * q3) * cameraRotation;
        }
        

        /// <summary>
        /// This is a Translation method for relative translations specified in the camera's local coordinate space
        /// </summary>
        /// <param name="distance">A vector representing the amount of translation to be done from the camera's perspective</param>
        public void Translate(Vector3 distance)
        {
            Vector3 diff = Vector3.Transform(distance, this.Rotation);
            this.cameraPosition += diff;

        }


        /// <summary>
        /// Returns the vector representing the direction (in real world)
        /// that is directly in front of the camera.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, this.Rotation);
            }
        }
        /// <summary>
        /// Returns the vector representing the direction (in real world)
        /// that is star board of the camera
        /// </summary>
        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.Right, this.Rotation);
            }
        }

        /// <summary>
        /// Returns the vector representing the direction (in real world)
        /// that is up from the camera's perspective
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.Up, this.Rotation);
            }
        }
        
        
    }
}
