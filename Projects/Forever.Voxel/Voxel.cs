using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.Voxel
{
    public struct Voxel
    {
        public struct ViewState
        {
            public Color Color;
            // Center of the voxel
            public Vector4 Position;
        }

        public VoxelState State { get; set; }
        public Material Material { get; set; }

        public bool ShouldRender()
        {
            return State == VoxelState.Active;
        }
    }
}
