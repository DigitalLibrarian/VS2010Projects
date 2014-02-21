using System;
using System.Collections.Generic;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Forever.Physics.Geometry;

namespace Forever.Physics.Collide
{
  public static class PrimitiveHelper
  {

    public static BoundingSphere ModelBoundingSphere(Model model)
    {

      List<TriangleVertexIndices> indices = new List<TriangleVertexIndices>();
      List<Vector3> points = new List<Vector3>();
      AABBFactory.ExtractData(model, points, indices, true);

      BoundingSphere modelSphere = BoundingSphere.CreateFromPoints(points);
     
      return modelSphere;
    }
  }
}
