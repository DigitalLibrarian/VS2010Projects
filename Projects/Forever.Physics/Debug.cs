using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics
{
  public static class Debug
  {
    public static bool Sanity(Vector3 v)
    {
      if (NaNny.IsNaN(v) || NaNny.IsInfinity(v))
      {
        Report("Vector - infinity or NaN");
        return false;
      }
      return true;
    }
    public static bool Sanity(BoundingBox b)
    {
      if (NaNny.IsNaN(b) || NaNny.IsInfinity(b))
      {
        Report("BoundingBox - infinity or NaN");
        return false;
      }

      if (Vector3.Distance(b.Min, b.Max) == 0f)
      {
        Report("BoundingBox - (min - max) == 0");
      }
      return true;
    }

    public static bool Sanity(Quaternion q)
    {

      if (NaNny.IsNaN(q) || NaNny.IsInfinity(q))
      {
        Report("Quaternion (Xna) - infinity or NaN");
        return false;
      }
      return true;
    }
    
    public static bool Sanity(Matrix m)
    {
      if (NaNny.IsNaN(m) || NaNny.IsInfinity(m))
      {
        Report("Matrix (Xna) - infinity or NaN");
        return false;
      }
      return true;
    }

    public static bool Sanity(float f)
    {
        if (NaNny.IsNaN(f) || NaNny.IsNaN(f))
        {
            Report("float - infinity of NaN");
            return false;
        }
        return true;
    }

    public static bool IsSaneNormal(Vector3 v)
    {
        if (!Debug.Sanity(v))
        {
            return false;
        }
        if (!TrickyMath.AlmostEquals(v.Length(), 1f))
        {
            Report("Non normal normal");
            return false;
        }
        return true;
    }

    public static void Report(string error)
    {
        throw new Exception(error);
    }
  }
}
