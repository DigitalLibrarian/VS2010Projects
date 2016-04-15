using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Voxel.SVO
{
    public static class NodePosition
    {
        public static readonly int  NEAR_BOTTOM_LEFT = 4;
		public static readonly int  NEAR_BOTTOM_RIGHT = 0;
		public static readonly int  NEAR_TOP_LEFT = 6;
		public static readonly int  NEAR_TOP_RIGHT = 2;
 
		public static readonly int  FAR_BOTTOM_LEFT = 5;
		public static readonly int  FAR_BOTTOM_RIGHT = 1;
		public static readonly int  FAR_TOP_RIGHT = 3;
		public static readonly int  FAR_TOP_LEFT = 7;

        //any defined node should be < 8 && > 0
        public static readonly int MaxNodeCount = 8;
    };

}
