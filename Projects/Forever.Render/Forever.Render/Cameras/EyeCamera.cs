using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Forever.Render;
using Microsoft.Xna.Framework.Graphics;

namespace Forever.Render.Cameras
{
    /// <summary>
    /// Basic Camera with iCamera handles.  You are the EYE
    /// </summary>
    public class EyeCamera : BasicCamera
    {
        protected float FOV = MathHelper.Pi / 4;
        public float AspectRatio { get { return _graphics.Viewport.AspectRatio; } }
        protected float nearClip =  0.01f;
        protected float farClip = 10000.0f;
     
        public override Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(this.FOV, this.AspectRatio, this.nearClip, this.farClip);
            }
        }

        
     
        public override Matrix View
        {
            get
            {


                return Matrix.Invert(Matrix.CreateFromQuaternion(this.Rotation) * Matrix.CreateTranslation(this.Position));
            }
        }


        private GraphicsDevice _graphics;
        public EyeCamera(GraphicsDevice graphicsDevice)
        {
            _graphics = graphicsDevice;
            Rotation = Quaternion.Identity;
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
        }
        

        /// <summary>
        /// This is a Translation method for relative translations specified in the camera's local coordinate space
        /// </summary>
        /// <param name="distance">A vector representing the amount of translation to be done from the camera's perspective</param>
        public void Translate(Vector3 distance)
        {
            Vector3 diff = Vector3.Transform(distance, this.Rotation);
            this.Position += diff;

        }
    }
}
