﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Voxel.World
{
    public class WorldCell
    {
        public Chunk Chunk { get; private set; }

        public WorldCell(Chunk chunk)
        {
            Chunk = chunk;
        }
    }
}
