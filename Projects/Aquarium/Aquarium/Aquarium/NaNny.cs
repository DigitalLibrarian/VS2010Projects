using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Aquarium
{
  public static class NaNny
  {
    public  const int CornersInBB = 8;

    public static bool IsNaN(Quaternion q)
    {
      return float.IsNaN(q.W) || float.IsNaN(q.X) || float.IsNaN(q.Y) || float.IsNaN(q.Z);
    }
    public static bool IsInfinity(Quaternion q)
    {
      return float.IsInfinity(q.W) || float.IsInfinity(q.X) || float.IsInfinity(q.Y) || float.IsInfinity(q.Z);
    }

    public static bool IsNaN(float f)
    {
        return float.IsNaN(f);
    }
    public static bool IsInfinity(float f)
    {
        return float.IsInfinity(f);
    }
   


    public static bool IsNaN(Matrix m)
    {
     return 
         float.IsNaN(m.M11) || float.IsNaN(m.M12) || float.IsNaN(m.M13) || float.IsNaN(m.M14)
      || float.IsNaN(m.M21) || float.IsNaN(m.M22) || float.IsNaN(m.M23) || float.IsNaN(m.M24)
      || float.IsNaN(m.M31) || float.IsNaN(m.M32) || float.IsNaN(m.M33) || float.IsNaN(m.M34)
      || float.IsNaN(m.M41) || float.IsNaN(m.M42) || float.IsNaN(m.M43) || float.IsNaN(m.M44);
    }

    public static bool IsInfinity(Matrix m)
    {
      return
          float.IsInfinity(m.M11) || float.IsInfinity(m.M12) || float.IsInfinity(m.M13) || float.IsInfinity(m.M14)
       || float.IsInfinity(m.M21) || float.IsInfinity(m.M22) || float.IsInfinity(m.M23) || float.IsInfinity(m.M24)
       || float.IsInfinity(m.M31) || float.IsInfinity(m.M32) || float.IsInfinity(m.M33) || float.IsInfinity(m.M34)
       || float.IsInfinity(m.M41) || float.IsInfinity(m.M42) || float.IsInfinity(m.M43) || float.IsInfinity(m.M44);
    }

    public static bool IsNaN(Vector3 v)
    {
      return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
    }
    public static bool IsInfinity(Vector3 v)
    {
      return float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.Z);
    }


    public static bool IsNaN(BoundingBox b)
    {
      Vector3[] corners = new Vector3[8];
      b.GetCorners(corners);
      bool nan = false;
      for (int i = 0; i < CornersInBB && !nan; i++)
      {
        Vector3 v = corners[i];
        nan = nan || float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
      }
      return nan;
    }
    public static bool IsInfinity(BoundingBox b)
    {
      Vector3[] corners = new Vector3[8];
      b.GetCorners(corners);
      bool nan = false;
      for (int i = 0; i < CornersInBB && !nan; i++)
      {
        Vector3 v = corners[i];
        nan = nan || float.IsInfinity(v.X) || float.IsInfinity(v.Y) || float.IsInfinity(v.Z);
      }

      return nan;
    }
  }
}
