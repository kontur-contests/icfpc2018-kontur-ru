using System.Collections.Generic;

namespace lib.Utils
{
    public class PriorityQueue<T>
    {
        private readonly Heap<T> heap;

        public PriorityQueue(IComparer<T> comparer = null)
        {
            heap = new Heap<T>(comparer);
        }

        public int Count => heap.Count;

        public T Dequeue() => heap.DeleteMin();

        public void Enqueue(T item) => heap.Add(item);

        public override string ToString()
        {
            return $"{nameof(Count)}: {Count}";
        }
    }
}