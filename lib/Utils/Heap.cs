using System;
using System.Collections;
using System.Collections.Generic;

namespace lib.Utils
{
    public class Heap<T> : IEnumerable<T>
    {
        private readonly IComparer<T> comparer;
        private readonly List<T> values = new List<T>();

        public Heap(IComparer<T> comparer = null)
        {
            this.comparer = comparer ?? Comparer<T>.Default;
        }

        public bool IsEmpty => Count == 0;

        public int Count => values.Count;

        public T Min
        {
            get
            {
                if (IsEmpty)
                    throw new InvalidOperationException($"{nameof(Heap<T>)} is empty");
                return values[0];
            }
        }

        public void Add(T value)
        {
            values.Add(value);
            var c = values.Count - 1;
            while (c > 0)
            {
                var p = (c - 1) / 2;
                if (comparer.Compare(values[p], values[c]) <= 0)
                    break;
                var t = values[p];
                values[p] = values[c];
                values[c] = t;
                c = p;
            }
        }
        public T DeleteMin()
        {
            if (IsEmpty)
                throw new InvalidOperationException($"{nameof(Heap<T>)} is empty");
            var res = values[0];
            values[0] = values[values.Count - 1];
            values.RemoveAt(values.Count - 1);

            var p = 0;
            while (true)
            {
                var c1 = p * 2 + 1;
                var c2 = p * 2 + 2;
                if (c1 >= values.Count)
                    break;
                if (c2 >= values.Count)
                {
                    if (comparer.Compare(values[p], values[c1]) > 0)
                    {
                        var t = values[p];
                        values[p] = values[c1];
                        values[c1] = t;
                    }
                    break;
                }
                if (comparer.Compare(values[p], values[c1]) <= 0 && comparer.Compare(values[p], values[c2]) <= 0)
                    break;
                if (comparer.Compare(values[p], values[c1]) <= 0)
                {
                    var t = values[p];
                    values[p] = values[c2];
                    values[c2] = t;
                    p = c2;
                }
                else if (comparer.Compare(values[p], values[c2]) <= 0)
                {
                    var t = values[p];
                    values[p] = values[c1];
                    values[c1] = t;
                    p = c1;
                }
                else if (comparer.Compare(values[c1], values[c2]) <= 0)
                {
                    var t = values[p];
                    values[p] = values[c1];
                    values[c1] = t;
                    p = c1;
                }
                else
                {
                    var t = values[p];
                    values[p] = values[c2];
                    values[c2] = t;
                    p = c2;
                }
            }

            return res;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}