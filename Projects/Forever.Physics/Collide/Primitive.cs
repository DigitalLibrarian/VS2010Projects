using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using Forever.Physics.Geometry;

namespace Forever.Physics.Collide
{
  public enum CollideType { None, Sphere, Plane, Box };

  public abstract class Primitive
  {
    protected CollideType primType = CollideType.None;
    public CollideType PrimType { get { return primType; } }

    IRigidBody body;
    
    protected Matrix offset;

    public IRigidBody Body { get { return body; } }
    public Matrix OffsetMatrix { get { return offset; } } 
    public virtual Matrix Transform { get { return this.Body.World * this.offset; } }

    public Vector3 Forward
    {
        get
        {
            return Vector3.Transform(body.Forward, OffsetMatrix);
        }
    }
    public Vector3 Up
    {
        get
        {
            return Vector3.Transform(body.Up, OffsetMatrix);
        }
    }
    public Vector3 Right
    {
        get
        {
            return Vector3.Transform(body.Right, OffsetMatrix);
        }
    }

    private Matrix BodyWorld { get { return Body.World; } }
      
      public Vector3 getAxis(int index)
      {

          Matrix primWorld = BodyWorld * OffsetMatrix;

          
          
          Vector3[] axis = new Vector3[] {
              primWorld.Right,
              primWorld.Up,
              primWorld.Backward
          };

          Vector3 v = axis[index];
          if (v.Length() != 0)
          {
              //v.Normalize();
          }
          return axis[index];
      }


    public Primitive(IRigidBody b, Vector3 off)
    {
      body = b;
    }

    public Primitive(IRigidBody b, Matrix off)
    {
      body = b;
      offset = off;
    }
  }

  public class Sphere : Primitive
  {
    //position is determined by body and offset from base class
    float radius;
    public float Radius { get { return radius; } set { radius = value; } }

    public Sphere(IRigidBody b, Matrix o, float r) : base(b, o)
    {
      primType = CollideType.Sphere;
      radius = r;
    }
  }

  public class Plane : Primitive
  {
    Vector3 normal, point;

    public Vector3 Normal { get { return normal; } }
    public float Offset { 
      get { 
        return ((-normal.X * point.X) + (-normal.Y * point.Y) + (-normal.Z * point.Z)); 
      } 
    }
    public Vector3 PrinciplePoint { get { return point; } }

    public Plane(IRigidBody b, Vector3 p, Vector3 n)
      : base(b, p)
    {
      primType = CollideType.Plane;
      normal = n;
      point = p;
      offset = Matrix.Identity;

    }

      // TODO - unit test
    /// <summary>
    /// Project the given point in world coordinates to the nearest point on the plane in world coordinates
    /// </summary>
    /// <param name="w_point"></param>
    /// <returns></returns>
    public Vector3 ClosestPoint(Vector3 w_point)
    {
        float dist = Vector3.Dot(Normal, w_point + (normal * Offset));
        return w_point - (dist * Normal);
    }
  }

  public class Box : Primitive
  {

    Vector3 halfSizes;
    public Vector3 HalfSizes { get { return halfSizes; } }

    public Box(IRigidBody b, Matrix off, Vector3 half_sizes) : base(b, off){
      halfSizes = half_sizes;
      primType = CollideType.Box;
    }

    public Vector3[] LocalVerts()
    {
      float[] mults = {
        -1f, -1f, -1f ,
        -1f, -1f, +1f ,
        -1f, +1f, -1f ,
        -1f, +1f, +1f ,
        +1f, -1f, -1f ,
        +1f, -1f, +1f ,
        +1f, +1f, -1f ,
        +1f, +1f, +1f ,
      };


      Vector3[] verts = new Vector3[8];
      int z = 0;
      for (int i = 0; i < verts.Length; i++)
      {
        verts[i] = new Vector3(
          halfSizes.X * mults[z++],
          halfSizes.Y * mults[z++],
          halfSizes.Z * mults[z++]
        );

        verts[i] = Vector3.Transform(verts[i], offset);
      }

      return verts;
    }

    public Vector3[] WorldVerts()
    {
      float[] mults = {
        -1f, -1f, -1f , // 0 : 1, 2, 4
        -1f, -1f, +1f , // 1 : 0, 5, 3
        -1f, +1f, -1f , // 2 : 3, 0, 6
        -1f, +1f, +1f , // 3 : 2, 1, 7
        +1f, -1f, -1f , // 4 : 0, 5, 6
        +1f, -1f, +1f , // 5 : 1, 4, 7
        +1f, +1f, -1f , // 6 : 2, 4, 7
        +1f, +1f, +1f , // 7 : 3, 7, 6
      };

      Vector3[] verts = new Vector3[8];
      int z = 0;
      for (int i = 0; i < verts.Length; i++)
      {
        verts[i] = new Vector3(
          halfSizes.X * mults[z++],
          halfSizes.Y * mults[z++],
          halfSizes.Z * mults[z++]
        );

        verts[i] = Vector3.Transform(verts[i], this.Transform);
      }

      return verts;
    }


    public List<Vector3[]> WorldEdges()
    {
        List<Vector3[]> result = new List<Vector3[]>();

        Vector3[] worldVerts = this.WorldVerts();



        
        result.Add(new Vector3[] { worldVerts[0], worldVerts[1] });
        result.Add(new Vector3[] { worldVerts[2], worldVerts[3] });

        result.Add(new Vector3[] { worldVerts[1], worldVerts[5] });
        result.Add(new Vector3[] { worldVerts[1], worldVerts[3] });

        result.Add(new Vector3[] { worldVerts[3], worldVerts[7] });
        result.Add(new Vector3[] { worldVerts[0], worldVerts[2] });

        result.Add(new Vector3[] { worldVerts[2], worldVerts[6] });
        result.Add(new Vector3[] { worldVerts[0], worldVerts[4] });

        result.Add(new Vector3[] { worldVerts[4], worldVerts[5] });
        result.Add(new Vector3[] { worldVerts[6], worldVerts[4] });

        result.Add(new Vector3[] { worldVerts[5], worldVerts[7] });
        result.Add(new Vector3[] { worldVerts[6], worldVerts[7] });

        return result;
    }


  }




}
