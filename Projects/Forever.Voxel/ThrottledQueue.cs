using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forever.Voxel
{
    public class ThrottledQueue<T>
    {
        public int MaxPerPump { get; set; }
        Queue<T> Queue { get; set; }

        public ThrottledQueue(int maxPerPump)
        {
            MaxPerPump = maxPerPump;
            Queue = new Queue<T>();
        }

        public void Enqueue(T t)
        {
            Queue.Enqueue(t);
        }

        public bool HasCapacity()
        {
            return Queue.Count() < MaxPerPump;
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
