using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Voxel
{
    public class ThrottledQueue<T>
    {
        public int MaxPerPump { get; set; }
        public int Capacity { get; set; }
        Queue<T> Queue { get; set; }

        public ThrottledQueue(int maxPerPump, int capacity)
        {
            MaxPerPump = maxPerPump;
            Capacity = capacity;
            Queue = new Queue<T>(capacity);
        }

        public int Count()
        {
            return Queue.Count();
        }

        public void Enqueue(T t)
        {
            Queue.Enqueue(t);
        }

        public bool HasCapacity()
        {
            return Queue.Count() < Capacity;
        }

        public bool Any()
        {
            return Queue.Any();
        }

        public bool Contains(T t)
        {
            return Queue.Contains(t);
        }

        public IEnumerable<T> Pump()
        {
            for (int i = 0; i < MaxPerPump; i++)
            {
                if (Queue.Any())
                {
                    yield return Queue.Dequeue();
                }
            }
        }
    }
}
