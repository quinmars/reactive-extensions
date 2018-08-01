using System;
using System.Collections.Generic;
using System.Text;

namespace akarnokd.reactive_extensions
{
    internal sealed class AsscendingComparer<T, K> : IComparer<T>
    {
        readonly Func<T, K> selector;
        readonly IComparer<K> comparer;

        public AsscendingComparer(Func<T, K> selector, IComparer<K> comparer = null)
        {
            this.selector = selector;
            this.comparer = comparer ?? Comparer<K>.Default;
        }

        public int Compare(T x, T y)
        {
            var a = selector(x);
            var b = selector(y);
            return comparer.Compare(a, b);
        }
    }

    internal sealed class DescendingComparer<T, K> : IComparer<T>
    {
        readonly Func<T, K> selector;
        readonly IComparer<K> comparer;

        public DescendingComparer(Func<T, K> selector, IComparer<K> comparer = null)
        {
            this.selector = selector;
            this.comparer = comparer ?? Comparer<K>.Default;
        }

        public int Compare(T x, T y)
        {
            var a = selector(x);
            var b = selector(y);
            return comparer.Compare(b, a);
        }
    }

    internal sealed class OrderedObservable<T> : IOrderedObservable<T>
    {
        readonly IObservable<T> source;
        readonly OrderedObservable<T> parent;
        readonly IComparer<T> comparer;

        public OrderedObservable(IObservable<T> source, IComparer<T> comparer)
        {
            this.comparer = comparer;
            this.source = source;
        }

        OrderedObservable(IObservable<T> source, IComparer<T> comparer, OrderedObservable<T> parent)
            : this(source, comparer)
        {
            this.parent = parent;
        }

        IOrderedObservable<T> IOrderedObservable<T>.CreateOrderedObservable<TKey>(Func<T, TKey> selector, IComparer<TKey> comparer, bool descending)
        {
            IComparer<T> comb;
            if (descending)
                comb = new DescendingComparer<T, TKey>(selector, comparer);
            else
                comb = new AsscendingComparer<T, TKey>(selector, comparer);

            return new OrderedObservable<T>(source, comb, this);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var comp =  parent == null ? comparer : ComposeComparer();

            var parentObserver = new OrderedObserver(observer, comp);
            var d = source.Subscribe(parentObserver);
            parentObserver.OnSubscribe(d);
            return parentObserver;
        }

        IComparer<T> ComposeComparer()
        {
            // first count the ancestors
            var count = 0;
            var cur = this;
            while (cur != null)
            {
                cur = cur.parent;
                count++;
            }

            // create and 
            var array = new IComparer<T>[count];

            // fill the array
            var i = count;
            cur = this;
            while (cur != null)
            {
                i--;
                array[i] = cur.comparer;
                cur = cur.parent;
            }

            return new CompositeComparer(array);
        }

        sealed class CompositeComparer : IComparer<T>
        {
            readonly IComparer<T>[] comparers;

            public CompositeComparer(IComparer<T>[] comparers)
            {
                this.comparers = comparers;
            }

            public int Compare(T x, T y)
            {
                foreach (var c in comparers)
                {
                    var r = c.Compare(x, y);
                    if (r != 0)
                    {
                        return r;
                    }
                }
                return 0;
            }
        }

        sealed class OrderedObserver : BaseObserver<T, T>
        {
            Heap<T> heap;
            bool done;

            internal OrderedObserver(IObserver<T> downstream, IComparer<T> comparer) : base(downstream)
            {
                heap = new Heap<T>(comparer);
            }

            public override void OnCompleted()
            {
                if (done)
                {
                    return;
                }

                var ex = default(Exception);
                try
                {
                    heap.Build();
                    while (heap.Count > 0)
                        downstream.OnNext(heap.Pop());

                }
                catch (Exception e)
                {
                    ex = e;
                }

                done = true;
                heap = null;

                if (ex != null)
                {
                    downstream.OnError(ex);

                }
                else
                {
                    downstream.OnCompleted();
                }
                Dispose();
            }

            public override void OnError(Exception error)
            {
                if (done)
                {
                    return;
                }
                heap = null;
                downstream.OnError(error);
                Dispose();
            }

            public override void OnNext(T value)
            {
                if (done)
                {
                    return;
                }

                heap.Append(value);
            }
        }
    }
}
