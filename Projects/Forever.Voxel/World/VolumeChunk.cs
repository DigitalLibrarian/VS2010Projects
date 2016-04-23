using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.Voxel.World
{
    class VolumeChunk : IVolumeChunk
    {
        public Matrix World { get; set; }

        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public int InstanceCount
        {
            get { throw new NotImplementedException(); }
        }

        public Microsoft.Xna.Framework.Graphics.VertexBufferBinding[] Bindings
        {
            get { throw new NotImplementedException(); }
        }

        public void Allocate(Action<int, int, int> initialVisitor = null)
        {
            throw new NotImplementedException();
        }

        public void Deallocate()
        {
            throw new NotImplementedException();
        }

        public int RebuildInstanceBuffer(ReferencePoint reference)
        {
            throw new NotImplementedException();
        }

        public void SetVoxel(int x, int y, int z, Voxel voxel)
        {
            throw new NotImplementedException();
        }
    }
}
