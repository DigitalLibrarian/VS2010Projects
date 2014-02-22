using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Aquarium.GA.SpacePartitions
{
    /// <summary>
    /// A spatial partitioned matrix of objects. 
    /// </summary>
    public class Space<T> where T: class
    {
        /// <summary>
        /// 3D matrix of cell spaces
        /// </summary>
        Dictionary<SpaceCoord, Partition<T>> TheMatrix { get; set; }
        Dictionary<T, Partition<T>> PartitionAssignment { get; set; }

        public IEnumerable<SpaceCoord> Coords { get { return TheMatrix.Keys; } }
        public IEnumerable<Partition<T>> Partitions { get { return TheMatrix.Values; } }


        public int GridSize { get; private set; }

        public int Count { get; private set; }

        public Space(int gridSize)
        {
            GridSize = gridSize;
            TheMatrix = new Dictionary<SpaceCoord, Partition<T>>();
            PartitionAssignment = new Dictionary<T, Partition<T>>();
            Count = 0;
        }

        public void Register(T obj, Vector3 position)
        {
            var coord = PositionToCoord(position, GridSize);
            var par = GetOrCreate(coord, GridSize);
            AssignPartition(obj, par);
            Count++;
        }

        public void AssignPartition(T obj, Partition<T> par)
        {
            par.Objects.Add(obj);
            PartitionAssignment[obj] = par;
        }

        public void UnRegister(T obj)
        {
            Partition<T> p = PartitionAssignment[obj];
            p.Objects.Remove(obj);
        }

        public void Update(T obj, Vector3 position)
        {
            UnRegister(obj);
            Register(obj, position);


        }


        #region Conversions
        private BoundingBox CoordinateBoundingBox(SpaceCoord coord, float boxHalfSize)
        {
            
                Vector3 center = CoordToVector(coord, boxHalfSize);
                var min = new Vector3(
                    coord.X == 0 ? -boxHalfSize * 2 : center.X - boxHalfSize,
                    coord.Y == 0 ? -boxHalfSize * 2 : center.Y - boxHalfSize,
                    coord.Z == 0 ? -boxHalfSize * 2 : center.Z - boxHalfSize);

                var max = new Vector3(
                    coord.X == 0 ? +boxHalfSize * 2 : center.X + boxHalfSize,
                    coord.Y == 0 ? +boxHalfSize * 2 : center.Y + boxHalfSize,
                    coord.Z == 0 ? +boxHalfSize * 2 : center.Z + boxHalfSize);

                return new BoundingBox(min, max);
            
        }

        private IEnumerable<SpaceCoord> CoordBox(SpaceCoord c, int coordBoxHalf=1)
        {
            List<SpaceCoord> list = new List<SpaceCoord>();
            for (int x = -coordBoxHalf; x < coordBoxHalf+1; x++)
                for (int y = -coordBoxHalf; y < coordBoxHalf + 1; y++)
                    for (int z = -coordBoxHalf; z < coordBoxHalf + 1; z++)
                        list.Add(
                            new SpaceCoord { X = c.X + x, Y = c.Y + y , Z = c.Z +z}
                            );

            return list;
        }

        private IEnumerable<Partition<T>> CoordBoxPartitions(SpaceCoord c, int coordBoxHalf=1)
        {
            List<Partition<T>> parts = new List<Partition<T>>();
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


        public Partition<T> GetOrCreate(Vector3 pos)
        {
            return GetOrCreate(VectorToCoord(pos, GridSize), GridSize);
        }
        private Partition<T> GetOrCreate(SpaceCoord coord, float boxHalfSize)
        {
            if (!TheMatrix.ContainsKey(coord))
            {
                var box = CoordinateBoundingBox(coord, boxHalfSize);

                foreach (var cKey in CoordBox(coord))
                {
                    if (!TheMatrix.ContainsKey(cKey)) continue;
                    var p = TheMatrix[cKey];
                    var ct = p.Box.Contains(box);
                    if (ct == ContainmentType.Contains)
                    {
                        if (coord.X == 0 || coord.Y == 0 || coord.Z == 0
                            || (cKey.X == 0 || cKey.Y == 0 || cKey.Z == 0))
                        {
                            var biggerBox = p.Box.ExtendToContain(box);
                            p.Box = biggerBox;
                            TheMatrix[coord] = p;
                            return TheMatrix[coord];
                            break;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }

                var par = new Partition<T>(box);
                TheMatrix.Add(coord, par);
            }
            return TheMatrix[coord];
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

        Vector3 CoordToVector(SpaceCoord coord, float boxHalfSize)
        {

            var x = coord.X;
            var y = coord.Y;
            var z = coord.Z;
            var corner =  new Vector3(x, y, z) * (boxHalfSize*2);
            return corner +(new Vector3(1, 1, 1) * boxHalfSize);

        }

        SpaceCoord VectorToCoord(Vector3 vect, float boxHalfSize)
        {
            var x = vect.X;
            var y = vect.Y;
            var z = vect.Z;

            if (x < 0) x -= boxHalfSize*2;
            if (y < 0) y -= boxHalfSize*2;
            if (z < 0) z -= boxHalfSize*2;

            var c = new SpaceCoord
            {
                X = (int)(x / (boxHalfSize*2)),
                Y = (int)(y / (boxHalfSize*2)),
                Z = (int)(z / (boxHalfSize*2)) 
            };
            
            
            return c;

        }
        #endregion


        public IEnumerable<T> Query(Func<SpaceCoord, T,  bool> test)
        {
            var list = new List<T>();

            foreach (var coord in TheMatrix.Keys)
            {
                var par = TheMatrix[coord];
                foreach (var mem in par.Objects)
                {
                    if (test(coord, mem))
                    {
                        list.Add(mem);
                    }
                }
            }
            return list;
        }

       

        public IEnumerable<T> QueryLocalSpace(Vector3 pos, float radius, Func<SpaceCoord, T, bool> test)
        {
            var list = new List<T>();

            int cellRadius = (int)(radius / GridSize);
            var c = VectorToCoord(pos, GridSize);

            foreach (var coord in CoordBox(c, cellRadius))
            {
                if (TheMatrix.ContainsKey(coord))
                {
                    var par = TheMatrix[coord];
                    foreach (var mem in par.Objects)
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

        public IEnumerable<Partition<T>> GetSpacePartitions(Vector3 pos, float radius)
        {

            int cellRadius = (int)(radius / GridSize);
            var c = VectorToCoord(pos, GridSize);
            return CoordBoxPartitions(c, cellRadius);
        }
    }
}
