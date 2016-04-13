using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.SpacePartitions
{
    public class Space<T> : SpaceMap<T> where T : class
    {
        public Space(int gridSize) : base(gridSize) { }

        public IPartition<T> GetOrCreate(Vector3 pos)
        {
            return GetOrCreate(VectorToCoord(pos, GridSize));
        }
        public IPartition<T> GetOrCreate(SpaceCoord coord)
        {
            return GetOrCreate(coord, GridSize);
        }
        private IPartition<T> GetOrCreate(SpaceCoord coord, float boxHalfSize)
        {
            if (!ContainsCoord(coord))
            {
                var box = CoordToBoundingBox(coord, boxHalfSize);
                var par = CreateNewPartition(box);
                AddCoord(coord, par);
            }
            return TheMatrix[coord];
        }
        
    }

    /// <summary>
    /// A spatial partitioned matrix of objects. 
    /// </summary>
    public class SpaceMap<T> where T: class
    {
        /// <summary>
        /// 3D matrix of cell spaces
        /// </summary>
        protected Dictionary<SpaceCoord, IPartition<T>> TheMatrix { get; set; }

        public int GridSize { get; private set; }

        public SpaceMap(int gridSize)
        {
            GridSize = gridSize;
            TheMatrix = new Dictionary<SpaceCoord, IPartition<T>>();
        }

        List<SpaceCoord> ActiveCoords = new List<SpaceCoord>();
        List<IPartition<T>> ActivePartitions = new List<IPartition<T>>();
        protected void AddCoord(SpaceCoord coord, IPartition<T> par)
        {
            ActiveCoords.Add(coord);
            ActivePartitions.Add(par);

            TheMatrix.Add(coord, par);
        }

        protected void RemoveCoord(SpaceCoord coord)
        {
            int i = ActiveCoords.FindIndex(0, x => x.GetHashCode() == coord.GetHashCode());
            ActiveCoords.RemoveAt(i);
            ActivePartitions.RemoveAt(i);

            TheMatrix.Remove(coord);
        }

        #region Conversions
        public BoundingBox CoordToBoundingBox(SpaceCoord coord)
        {
            return CoordToBoundingBox(coord);
        }
        protected BoundingBox CoordToBoundingBox(SpaceCoord coord, float boxHalfSize)
        {            
            Vector3 center = CoordToVector(coord, boxHalfSize);
            var min = new Vector3(
                center.X - boxHalfSize,
                center.Y - boxHalfSize,
                center.Z - boxHalfSize);

            var max = new Vector3(
                center.X + boxHalfSize,
                center.Y + boxHalfSize,
                center.Z + boxHalfSize);
            return new BoundingBox(min, max);   
        }

        protected IList<SpaceCoord> CoordBox(SpaceCoord c, int coordBoxHalf = 1)
        {
            List<SpaceCoord> list = new List<SpaceCoord>();
            for (int x = -coordBoxHalf; x < coordBoxHalf+1; x++)
                for (int y = -coordBoxHalf; y < coordBoxHalf + 1; y++)
                    for (int z = -coordBoxHalf; z < coordBoxHalf + 1; z++)
                    {

                        list.Add(
                            new SpaceCoord { X = c.X + x, Y = c.Y + y, Z = c.Z + z }
                            );
                    }

            return list;
        }

        private IList<IPartition<T>> CoordBoxPartitions(SpaceCoord c, int coordBoxHalf = 1)
        {
            List<IPartition<T>> parts = new List<IPartition<T>>();
            var coords = CoordBox(c, coordBoxHalf);
            foreach (var coord in coords)
            {
                if (TheMatrix.ContainsKey(coord))
                {
                    parts.Add(TheMatrix[coord]);
                }
            }

            return parts;
        }

        protected SpaceCoord? GetPartitionCoord(IPartition<T> partition)
        {

            foreach (var coord in TheMatrix.Keys)
            {
                if (TheMatrix[coord] == partition)
                {
                    return coord;
                }
            }
            return null;
        }

        public virtual void RemovePartition(IPartition<T> partition)
        {
            var coord = GetPartitionCoord(partition);
            if (coord.HasValue)
            {
                RemoveCoord(coord.Value);
            }
        }
        
        
        public bool ContainsCoord(SpaceCoord coord)
        {
            return TheMatrix.ContainsKey(coord);
        }

        public void Add(SpaceCoord coord, IPartition<T> par)
        {
            AddCoord(coord, par);
        }

        protected virtual IPartition<T> CreateNewPartition(BoundingBox box)
        {
            return new Partition<T>(box);
        }

        public SpaceCoord PositionToCoord(Vector3 pos)
        {
            return PositionToCoord(pos, GridSize);
        }

        private SpaceCoord PositionToCoord(Vector3 pos, float boxHalfSize)
        {
            
            foreach (var c in TheMatrix.Keys)
            {
                var p = TheMatrix[c];
                if(p.Box.Contains(pos) != ContainmentType.Disjoint)
                {
                    return c;
                }
            }

            return VectorToCoord(pos, boxHalfSize);
        }

        public Vector3 CoordToVector(SpaceCoord coord)
        {
            return CoordToVector(coord, GridSize);
        }

        protected Vector3 CoordToVector(SpaceCoord coord, float boxHalfSize)
        {

            var x = coord.X;
            var y = coord.Y;
            var z = coord.Z;
            var corner =  new Vector3(x, y, z) * (boxHalfSize*2);
            return corner +(new Vector3(1, 1, 1) * boxHalfSize);

        }

        public SpaceCoord VectorToCoord(Vector3 vect)
        {
            return VectorToCoord(vect, GridSize);
        }

        protected SpaceCoord VectorToCoord(Vector3 vect, float boxHalfSize)
        {
            var x = vect.X;
            var y = vect.Y;
            var z = vect.Z;

            if (x < 0) x -= boxHalfSize*2;
            if (y < 0) y -= boxHalfSize*2;
            if (z < 0) z -= boxHalfSize*2;

            return RawSpaceCoord(x, y, z, boxHalfSize);

        }


        private SpaceCoord RawSpaceCoord(float x, float y, float z, float boxHalfSize)
        {
            return new SpaceCoord
            {
                X = (int)(x / (boxHalfSize*2)),
                Y = (int)(y / (boxHalfSize*2)),
                Z = (int)(z / (boxHalfSize*2)) 
            };
        }
        #endregion


        public IEnumerable<IPartition<T>> GetSpacePartitions(Vector3 pos, float radius)
        {
            int cellRadius = (int)(radius / GridSize);
            var c = VectorToCoord(pos, GridSize);
            return CoordBoxPartitions(c, cellRadius);
        }

        public int Find(Func<T, bool> test, List<T> buffer, int maxSize)
        {
            int numAdded = 0;
            for (int i = 0; i < ActivePartitions.Count; i++)
            {
                var partition = ActivePartitions[i];
                int objCount = partition.Objects.Count;
                for(int j = 0; j < objCount;j++)
                {
                    if (test(partition.Objects[j]))
                    {
                        buffer.Add(partition.Objects[j]);
                        numAdded++;
                        if (buffer.Count >= maxSize)
                        {
                            return numAdded;
                        }
                    }
                }
            }
            return numAdded;
        }

        public int Find(Func<T, bool> test, T[] buffer, int start, int max)
        {
            if (max == 0) return 0;
            int numAdded = 0;
            for (int i = 0; i < TheMatrix.Values.Count; i++)
            {
                var partition = TheMatrix.Values.ElementAt(i);
                foreach(var obj in partition.Objects)
                {
                    if (test(obj))
                    {
                        buffer[start++] = obj;
                        if (numAdded++ >= max)
                        {
                            return numAdded;
                        }
                    }
                }
            }
            return numAdded;
        }

        public IEnumerable<T> Query(Func<SpaceCoord, T, bool> test)
        {
            foreach (var coord in TheMatrix.Keys)
            {
                var par = TheMatrix[coord];
                foreach (var mem in par.Objects)
                {
                    if (test(coord, mem))
                    {
                        yield return mem;
                    }
                }
            }
        }

        public IEnumerable<T> QueryLocalSpace(Vector3 pos, float radius)
        {
            return QueryLocalSpace(pos, radius, (c, o) => true);
        }

        public IEnumerable<T> QueryLocalSpace(Vector3 pos, float radius, Func<SpaceCoord, T, bool> test)
        {
            // TODO - yield return
            var list = new List<T>();

            int cellRadius = (int)(radius / GridSize);
            var c = VectorToCoord(pos, GridSize);

            foreach (var coord in CoordBox(c, cellRadius))
            {
                var cleanVect = CoordToVector(coord, GridSize);
                if (TheMatrix.ContainsKey(coord))
                {
                    var par = TheMatrix[coord];
                    var members = par.Objects.ToList();
                    foreach (var mem in members)
                    {
                        if (test(coord, mem))
                        {
                            list.Add(mem);
                        }
                    }
                }
            }
            return list;
        }
    }
}
