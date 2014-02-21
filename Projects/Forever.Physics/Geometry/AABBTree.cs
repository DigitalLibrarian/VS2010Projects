using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace Forever.Physics.Geometry
{
  
    public class AABBTree
    {
        private AABBNode root;
        public AABBNode Root { get {return root; } }
        public AABBTree(List<float[]> vertices, AABBNodeInfo tree_information)
        {
            root = new AABBNode(vertices, tree_information, 0);
        }


    }

    public class AABBNode
    {
        BoundingBox box;
        public BoundingBox BBox { get { return box; } set { box = value; } }
        public Vector3 Center { get { return (box.Min + box.Max) * 0.5f; } }

        private AABBNodeInfo _tree_info;
        public AABBNodeInfo TreeInfo { get { return _tree_info; } }
        private int _depth;
        public int Depth { get { return _depth; } }

        private bool _isLeaf = false;
        public bool IsLeaf { get { return _isLeaf; } }

        private AABBNode _Left, _Right;
        public AABBNode Left { get { return _Left; } }
        public AABBNode Right { get { return _Right; } }

      private bool _Marked = false;
      public bool Marked
      {
        get { return _Marked; }
        set { _Marked = value; }
      }
        public AABBNode(List<float[]> vertices, AABBNodeInfo tree_information, int depth)
        {
          _Marked = false; //doubly so!
            _tree_info = tree_information;
            _depth = depth;
            
            _BuildTree_PreserveTriangles(vertices);
        }


        private void _BuildTree_PreserveTriangles(List<float[]> vertices)
        {
            float[] min, max;
            AABBHelper.GetMinMax(vertices, out min, out max);
            box = new BoundingBox(
                new Vector3(min[0], min[1], min[2]),
                new Vector3(max[0], max[1], max[2])
            );
          

            //make prune control this
            if ((_tree_info.prune && vertices.Count <= _tree_info.leaf_min_verts) || _depth == _tree_info.max_tree_depth)
            {
                _isLeaf = true;
                return;
            }

            int chosen_axis = DetermineBestAxisAtFaceValue(vertices, box);
            
            float center = (min[chosen_axis] + max[chosen_axis]) / 2f;
            float face_center;
          
            List<float[]> left_side = new List<float[]>();
            List<float[]> right_side = new List<float[]>();
          
  
            float[][] tri = new float[3][];
            int count = 0;

            
            for (int i = 0; i < vertices.Count; i++)
            {
                tri[count] = vertices[i];
                if ( tri[count][0] < box.Min.X
                  || tri[count][1] < box.Min.Y
                  || tri[count][2] < box.Min.Z

                  || tri[count][0] > box.Max.X
                  || tri[count][1] > box.Max.Y
                  || tri[count][2] > box.Max.Z
                  )
                {
                  Debug.Report("Error in AABB calculation");
                }

                if (count == 2)
                {
                  
                    face_center = (tri[0][chosen_axis] + tri[1][chosen_axis] + tri[2][chosen_axis]) / 3f;
                    if (face_center < center)
                    {
                        left_side.Add(tri[0]);
                        left_side.Add(tri[1]);
                        left_side.Add(tri[2]);
                    }
                    else
                    {
                        right_side.Add(tri[0]);
                        right_side.Add(tri[1]);
                        right_side.Add(tri[2]);

                    }
                    count = 0;

                }
                else
                {
                    count++;
                }
            }
          
            if (left_side.Count == 0 || right_side.Count == 0)
            {
              this._isLeaf = true;
            }
            else
            {
              _Left = new AABBNode(left_side, _tree_info, _depth + 1);
              _Right = new AABBNode(right_side, _tree_info, _depth + 1);
              this._isLeaf = false;
            }
        }

   

        /// <summary>
        /// Looks at mesh data and finds the longest axis 
        /// of a axis aligned bounding box
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns>
        /// 0 for X axis, 1 for Y axis, and 2 for Z axis
        /// </returns>
        private int DetermineBestAxisAtFaceValue(List<float[]> vertices, BoundingBox box)
        {
            int left_count = 0;
            int right_count = 0;
            int diff = 0;
            int small_diff = vertices.Count;
            int best_axis = 0;
            int tri_index;

            Vector3 vec_center = (box.Max + box.Min) * 0.5f;
            float[] center = new float[3];
            center[0] = vec_center.X;
            center[1] = vec_center.Y;
            center[2] = vec_center.Z;
            

            float[][] tri = new float[3][];
            float face_center;
            for (int axis = 0; axis < 3; axis++)    //for each axis
            {
                tri_index = 0;
                left_count = right_count = 0;
                for (int i = 0; i < vertices.Count; i++)
                {
                    tri[tri_index] = vertices[i];
                    if(tri_index == 2){
                        face_center = (tri[0][axis] + tri[1][axis] + tri[2][axis]) / 3f;
                        if(face_center < center[axis]){
                            left_count++;
                        }else{
                            right_count++;
                        }
                        tri_index = 0;
                    }else{
                        tri_index++;
                    }
                }
                diff = Math.Abs(left_count - right_count);
                if(diff < small_diff){
                    small_diff = diff;
                    best_axis = axis;
                }
            }
            return best_axis;
        }




        private int DetermineBestAxisAtVertexDensity(List<float[]> vertices, BoundingBox box)
        {
          int left_count = 0;
          int right_count = 0;
          int diff = 0;
          int small_diff = vertices.Count;
          int best_axis = 0;
          int tri_index;

          Vector3 vec_center = (box.Max + box.Min) * 0.5f;
          float[] center = new float[3];
          center[0] = vec_center.X;
          center[1] = vec_center.Y;
          center[2] = vec_center.Z;


          for (int axis = 0; axis < 3; axis++)    //for each axis
          {
            left_count = right_count = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                if(vertices[i][axis] < center[axis])
                {
                  left_count++;
                }
                else
                {
                  right_count++;
                }
            }
            diff = Math.Abs(left_count - right_count);
            if (diff < small_diff)
            {
              small_diff = diff;
              best_axis = axis;
            }
          }
          return best_axis;
        }


        private BoundingBox FindAABB(List<float[]> vertices)
        {
            float[] min, max;
            AABBHelper.GetMinMax(vertices, out min, out max);
            return new BoundingBox(
                new Vector3(min[0], min[1], min[2]),
                new Vector3(max[0], max[1], max[2])
            );
        }
    }
    
    public struct AABBNodeInfo{
      //max verts to just christen as isLeaf if in prune mode
        public int leaf_min_verts;
      //hard limit on tree depth
        public int max_tree_depth;
      // prune means that it will stop recursing if it ever reachs a 
      // node with small enough number of triangles.
        public bool prune;
    }




}
