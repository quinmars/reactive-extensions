using System;
using System.Collections.Generic;
using System.Text;

namespace akarnokd.reactive_extensions
{
    internal class Heap<T>
    {
        private struct IndexedItem
        {
            public int Index;
            public T Value;
        }

        readonly List<IndexedItem> list;
        readonly IComparer<T> comparer;
        int count = 0;
        int nextIndex = 1;

        public Heap(IComparer<T> comparer)
        {
            this.list = new List<IndexedItem>();
            this.comparer = comparer;
        }

        public void Append(T value)
        {
            list.Add(new IndexedItem { Index = nextIndex, Value = value });
            nextIndex++;
            count++;
        }

        public void Build()
        { 
            for (int i = count / 2 - 1; i >= 0; --i)
                Heapify(i);
        }

        public int Count => count;

        public T Pop()
        {
            var v = list[0];
            list[0] = list[count - 1];
            list[count - 1] = default;
            count--;

            if (count > 0)
                Heapify(0);

            return v.Value;
        }

        void Heapify(int i)
        {
            while (true)
            {
                int min = i;
                if (Left(i) < count && IsLesser(Left(i), min))
                    min = Left(i);
                if (Right(i) < count && IsLesser(Right(i), min))
                    min = Right(i);
                if (min == i)
                    break;

                Swap(i, min);
                i = min;
            }
        }

        static int Left(int i) => 2 * i + 1;
        static int Right(int i) => 2 * i + 2;

        private bool IsLesser(int a, int b)
        {
            var v = comparer.Compare(list[a].Value, list[b].Value);
            if (v == 0)
                return list[a].Index < list[b].Index;

            return v < 0;
        }

        private void Swap(int a, int b)
        {
            var tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }
    }
}
