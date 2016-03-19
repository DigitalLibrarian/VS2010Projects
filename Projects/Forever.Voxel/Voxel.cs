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
            // Center of the voxel
            public Vector4 Position;
            public Color Color;
        }

        public VoxelState State { get; set; }
        public Material Material { get; set; }

        public bool ShouldRender()
        {
            return State == VoxelState.Active;
        }
        public void Rez()
        {
            State = VoxelState.Active;
        }
        public void Derez()
        {
            State = VoxelState.Inactive;
        }
    }
}
