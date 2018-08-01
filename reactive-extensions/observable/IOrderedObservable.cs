using System;
using System.Collections.Generic;
using System.Text;

namespace akarnokd.reactive_extensions
{
    public interface IOrderedObservable<out T> : IObservable<T>
    {
        IOrderedObservable<T> CreateOrderedObservable<K>(Func<T, K> selector, IComparer<K> comparer, bool descending);
    }
}
