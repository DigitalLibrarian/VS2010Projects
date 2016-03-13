using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aquarium.Life
{
    public class Group<T> where T : new()
    {
        int Min { get; set; }
        int Max { get; set; }

        Func<IEnumerator<int>, T> Factory { get; set; }

        public CircularIndex<T> Index { get; private set; }

        public Group(int min, int max, Func<IEnumerator<int>, T> factory)
        {
            Min = min;
            Max = max;
            Factory = factory;

            Index = new CircularIndex<T>();
        }

        public void Read(IEnumerator<int> g)
        {
            var num = Fuzzy.PositiveInteger(g.Next());
            num = Fuzzy.CircleClamp(num, Min, Max);

            for (int i = 0; i < num; i++)
            {
                var element = Create(g);
                Index.Add(element);
            }
        }

        public void Add(T g)
        {
            Index.Add(g);
        }
        
        protected T Create(IEnumerator<int> g)
        {
            return Factory(g);
        }
    }
}
