using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
namespace Forever.Render
{
    /// <summary>
    /// If you want any other behavior than the standard EyeCamera, please implement this
    /// interface and adhere to these contracts.
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// This is the rotation matrix that you can rotate vectors by to
        /// get them in the camera's local coordinate system.
        /// </summary>
        Quaternion Rotation { get; set; }
        /// <summary>
        /// Real world positional vector of the camera
        /// </summary>
        /// 
       // [EntityInspector("Camera.Pos")]
        Vector3 Position { get; set; }
        /// <summary>
        /// This is the projection matrix for your models if you want to see through this camera
        /// </summary>
        Matrix Projection { get; }
        /// <summary>
        /// This is the view matrix for you models if you want to see through this camera
        /// </summary>
        Matrix View { get; }
        /// <summary>
        /// Real-world normalized vector representing up from camera's perspective
        /// </summary>
        //[EntityInspector("Camera.Up")]
        Vector3 Up { get; }
        /// <summary>
        /// Real-world normalized vector representing star board (right) from camera's perspective
        /// </summary>
        //[EntityInspector("Camera.Right")]
        Vector3 Right { get; }
        /// <summary>
        /// Real-world normalized vector representing forward from camera's perspective
        /// </summary>
        //[EntityInspector("Camera.Forward")]
        Vector3 Forward { get; }
        /// <summary>
        /// Rotate on the local coordinate system standard axes by this yaw, pitch, and roll
        /// </summary>
        /// <param name="yaw">Rotation dun on the y axis</param>
        /// <param name="pitch">Rotation dun on the x axis</param>
        /// <param name="roll">Rotation dun on the z axis</param>
        void Rotate(float yaw, float pitch, float roll);
        /// <summary>
        /// This is a Translation method for relative translations specified in the camera's local coordinate space
        /// </summary>
        /// <param name="distance">A vector representing the amount of translation to be done from the camera's perspective</param>
        void Translate(Vector3 translation);

        //TODO - think about a LookAt(Vector3 pos, Vector3 up)
    }
}
