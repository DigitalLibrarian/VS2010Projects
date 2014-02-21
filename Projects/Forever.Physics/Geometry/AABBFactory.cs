using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Forever.Physics.Geometry
{

  public struct TriangleVertexIndices
  {
    public int I0;
    public int I1;
    public int I2;
  }


  public class AABBFactory
  {
    public static AABBTree AABBTree(Model model)
    {

      AABBNodeInfo tree_info = new AABBNodeInfo();
      tree_info.leaf_min_verts = 6;
      tree_info.max_tree_depth = 4;

      return AABBTree(model, tree_info);
    }

    public static AABBTree AABBTree(Model model, AABBNodeInfo tree_info)
    {
      List<TriangleVertexIndices> indices = new List<TriangleVertexIndices>();
      List<Vector3> points = new List<Vector3>();
      AABBFactory.ExtractData(model, points, indices, true);


      VertexPositionColor[] vertices = new VertexPositionColor[indices.Count * 3];

      List<float[]> triangles = new List<float[]>();

      int i = 0;
      foreach (TriangleVertexIndices index in indices)
      {
        vertices[i++] = new VertexPositionColor(points[index.I0], Color.White);
        vertices[i++] = new VertexPositionColor(points[index.I1], Color.White);
        vertices[i++] = new VertexPositionColor(points[index.I2], Color.White);

        float[] tri = new float[3];
        tri[0] = points[index.I0].X;
        tri[1] = points[index.I1].Y;
        tri[2] = points[index.I2].Z;
        triangles.Add(tri);
      }
      return new AABBTree(triangles, tree_info);
    }


    public static void ExtractData(Model mdl, List<Vector3> vtcs, List<TriangleVertexIndices> idcs, bool includeNoncoll)
    {
      Matrix m = Matrix.Identity;
      foreach (ModelMesh mm in mdl.Meshes)
      {
        m = GetAbsoluteTransform(mm.ParentBone);
        ExtractModelMeshData(mm, ref m, vtcs, idcs, "Collision Model", includeNoncoll);
      }
    }

    public static Matrix GetAbsoluteTransform(ModelBone bone)
    {
      if (bone == null)
        return Matrix.Identity;
      return bone.Transform * GetAbsoluteTransform(bone.Parent);
    }

    public static void ExtractModelMeshData(ModelMesh mm, ref Matrix xform,
        List<Vector3> vertices, List<TriangleVertexIndices> indices, string name, bool includeNoncoll)
    {
      foreach (ModelMeshPart mmp in mm.MeshParts)
      {

        if (!includeNoncoll)
        {
          EffectAnnotation annot = mmp.Effect.CurrentTechnique.Annotations["collide"];
          if (annot != null && annot.GetValueBoolean() == false)
          {
            Console.WriteLine("Ignoring model mesh part {0}:{1} because it's set to not collide.",
                name, mm.Name);
            continue;
          }
        }
        ExtractModelMeshPartData(mm, mmp, ref xform, vertices, indices, name);
      }
    }

    public static void ExtractModelMeshPartData(ModelMesh mm, ModelMeshPart mmp, ref Matrix xform,
        List<Vector3> vertices, List<TriangleVertexIndices> indices, string name)
    {
      int offset = vertices.Count;
      Vector3[] a = new Vector3[mmp.NumVertices];

      int stride = mmp.VertexBuffer.VertexDeclaration.VertexStride;

      mmp.VertexBuffer.GetData<Vector3>(mmp.VertexOffset * stride,  a, 0, mmp.NumVertices, stride);


      for (int i = 0; i != a.Length; ++i)
        Vector3.Transform(ref a[i], ref xform, out a[i]);
      vertices.AddRange(a);

      if (mmp.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
        throw new Exception(
            String.Format("Model {0} uses 32-bit indices, which are not supported.",
                          name));
      short[] s = new short[mmp.PrimitiveCount * 3];

     
      mmp.IndexBuffer.GetData<short>(mmp.StartIndex * sizeof(short),s, 0, s.Length);
      TriangleVertexIndices[] tvi = new TriangleVertexIndices[mmp.PrimitiveCount];
      for (int i = 0; i != tvi.Length; ++i)
      {
        //triangle creation
        tvi[i].I0 = s[i * 3 + 0] + offset;
        tvi[i].I1 = s[i * 3 + 1] + offset;
        tvi[i].I2 = s[i * 3 + 2] + offset;
      }
      indices.AddRange(tvi);
    }
  }
}
