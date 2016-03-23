using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    public class CircularIndex<T>
    {
        public ICollection<T> Collection { get; set; }

        public CircularIndex()
        {
            Collection = new List<T>();
        }

        public T Get(int id)
        {
            var index = 0;
            var count = Collection.Count;
            if (count > 0)
            {
                index = id % count;
            }
            return Collection.ElementAt(index);
        }

        public void Add(T element)
        {
            Collection.Add(element);
        }

        public void Remove(T element)
        {
            Collection.Remove(element);
        }
    }
}
