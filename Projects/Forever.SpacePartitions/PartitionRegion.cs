using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Forever.SpacePartitions
{
    public interface IPartitionRegion<T> : IEnumerable<T> where T : class
    {
        Space<T> Space { get; }
        IEnumerable<IPartition<T>> GetPartitions();

        void Refetch();
    }

    /// <summary>
    /// Returns and caches the partitions inside/intersecting the sphere
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PartitionSphere<T> : IPartitionRegion<T> where T : class
    {
        public Space<T> Space { get; private set; }
        IEnumerable<IPartition<T>> Partitions;
        BoundingSphere _sphere;
        BoundingSphere _fetchedSphere;
        IPartition<T> Principle;

        public BoundingSphere Sphere
        {
            get { return _sphere; }
            set
            {
                _sphere = value;
            }
        }
       
        public PartitionSphere(Space<T> space)
        {
            Space = space;
            Partitions = new IPartition<T>[0];
        }

        public IEnumerable<IPartition<T>> GetPartitions()
        {
            UpdatePartitions();
            return Partitions;
        }

        private void UpdatePartitions()
        {
            if (Sphere == null) return;
            bool geomFail = false;
            if (Principle != null)
            {
                geomFail = !Principle.Box.Intersects(Sphere);
                if (!geomFail)
                {
                    geomFail = Principle.Box.Contains(Sphere.Center) != ContainmentType.Contains;
                }
            }
            else
            {
                geomFail = true;
            }

            if ( _fetchedSphere == null ||
                _fetchedSphere.Contains(Sphere.Center) != ContainmentType.Contains ||
                geomFail
                )
            {
                Refetch();
            }
        }

        public void Refetch()
        {
            Principle = Space.GetOrCreate(Sphere.Center);
            Partitions = Space.GetSpacePartitions(Sphere.Center, Sphere.Radius);
            _fetchedSphere = Sphere;
        }

        public List<T> ToList()
        {
            var objs = new List<T>();
            foreach (var par in GetPartitions())
            {
                objs.AddRange(par.Objects);
            }
            return objs;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var objs = ToList();
            return objs.GetEnumerator();
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
