using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.Render.Cameras
{
    public class BasicCamera : ICamera
    {
        public Quaternion Rotation{ get; set; }
        public Vector3 Position{ get; set; }
        public virtual Matrix Projection { get; set; }
        public virtual Matrix View {get;set;}

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
