using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Forever.Physics.Geometry
{
    public static class AABBHelper
    {
      public static Vector3 RandomCorner(BoundingBox box)
      {
        Random rand = new Random();
        return   new Vector3(
          rand.Next(1) == 1 ? box.Min.X : box.Max.X,
          rand.Next(1) == 1 ? box.Min.Y : box.Max.Y,
          rand.Next(1) == 1 ? box.Min.Z : box.Max.Z
        );
      }

        public static void GetMinMax(Vector3[] vertices, out Vector3 min, out Vector3 max)
        {
            min = vertices[0];
            max = min;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].X < min.X)
                {
                    min.X = vertices[i].X;
                }
                if (vertices[i].Y < min.Y)
                {
                    min.Y = vertices[i].Y;
                }
                if (vertices[i].Z < min.Z)
                {
                    min.Z = vertices[i].Z;
                }


                if (vertices[i].X > max.X)
                {
                    max.X = vertices[i].X;
                }
                if (vertices[i].Y > max.Y)
                {
                    max.Y = vertices[i].Y;
                }
                if (vertices[i].Z > max.Z)
                {
                    max.Z = vertices[i].Z;
                }
            }
        }

        public static void GetMinMax(float[][] vertices, out float[] min, out float[] max)
        {
            min = new float[3];
            max = new float[3];
            min[0] = min[1] = min[2] = max[0] = max[1] = max[2] = 0f; //no guarantees ;P

            for (int i = 0; i < vertices.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    min[j] = (vertices[i][j] < min[j] ? vertices[i][j] : min[j]);
                    max[j] = (vertices[i][j] > max[j] ? vertices[i][j] : max[j]);
                }
            }
        }

        public static void GetMinMax(List<float[]> vertices, out float[] min, out float[] max)
        {
            min = new float[3];
            max = new float[3];
          
            if (vertices.Count == 0)
            {
              throw new Exception("Cannot get min/max of nothing!");
            }

            min[0] = max[0] = vertices[0][0];
            min[1] = max[1] = vertices[0][1];
            min[2] = max[2] = vertices[0][2];
          
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    min[j] = (vertices[i][j] < min[j] ? vertices[i][j] : min[j]);
                    max[j] = (vertices[i][j] > max[j] ? vertices[i][j] : max[j]);
                }
            }
        }

       


        public static List<float[]> UnrollIndexedVertexData(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            List<float[]> verts = new List<float[]>();

            VertexPositionNormalTexture v;
            float[] vec;
            for (int i = 0; i < indices.Length; i++)
            {
                v = vertices[indices[i]];
                vec = new float[3];
                vec[0] = v.Position.X;
                vec[1] = v.Position.Y;
                vec[2] = v.Position.Z;
                verts.Add(vec);
            }

            return verts;
        }

        public static BoundingBox TransformBoundingBox(BoundingBox box, Matrix transform)
        {

            Vector3[] corners = box.GetCorners();
            for (int i = 0; i < corners.Length; i++)
            {
              Debug.Sanity(corners[i]);
              Debug.Sanity(transform);

              //Big fucking problem
              /*
               * Sometimes  this operation will produce an infinity value
               * even though both of the operands on the right side of the 
               * '=' operator are not NaN or infinity.
               * 
               * This may be a side effect of my forcing the system to store
               * all the matrices in XNA native Matrix class.  My "conversion" 
               * to and from 4x3 matrices is probably to blame.
               * 
               * */
              Vector3 temp = Vector3.Transform(corners[i], transform);

              if (Debug.Sanity(temp))
              {
                corners[i] = temp;
              }
              else
              {
                /* This seems to be where it all falls apart. These values seem
                 * to be really near out of range in the debugger
                 */
                throw new Exception("Insane vector from sane vector * sane matrix.");
              }
            }

            Vector3 min, max;
            GetMinMax(corners, out min, out max);
            return new BoundingBox(min, max);
        }

      public static void TransformAABBTree(AABBTree tree, Matrix transform)
      {
        TransformAABBNode(tree.Root, transform);
      }
      public static void TransformAABBNode(AABBNode node, Matrix transform)
      {
        node.BBox = TransformBoundingBox(node.BBox, transform);
        if(node.Left != null){
          TransformAABBNode(node.Left, transform);
        }

        if (node.Right != null)
        {
          TransformAABBNode(node.Right, transform);
        }
      }

      public static float[] Vector3toFloatArray(Vector3 v){

        float[] a = new float[3];
        a[0] = v.X;
        a[1] = v.Y;
        a[2] = v.Z;
        return a;
      }

      public static void Leafs(AABBNode node, List<AABBNode> leafs)
      {
        if (node.IsLeaf)
        {
          leafs.Add(node);
        }
        else
        {
          Leafs(node.Left, leafs);
          Leafs(node.Right, leafs);
        }
        
      }

      public static float LongestLeafCornerLength(AABBTree tree)
      {

        List<AABBNode> leafs = new List<AABBNode>();
        AABBHelper.Leafs(tree.Root, leafs);

        if (leafs.Count == 0)
        {
          //>)
          return -1f;
        }

        float longest_length = 0f;
        for (int i = 0; i < leafs.Count; i++)
        {
          BoundingBox box = leafs[i].BBox;
          Vector3[] corners = box.GetCorners();

          for (int j = 0; j < corners.Length; j++)
          {
            Vector3 to_corner = corners[j];
            float length = to_corner.Length();
            if (length > longest_length)
            {
              longest_length = length;
            }
          }
        }

        return longest_length;
      }
    }
  }
