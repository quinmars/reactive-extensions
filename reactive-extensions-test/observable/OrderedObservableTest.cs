using NUnit.Framework;
using System;
using akarnokd.reactive_extensions;
using System.Reactive.Linq;
using System.Linq;
using System.Reactive;
using System.Collections.Generic;

namespace akarnokd.reactive_extensions_test.observable
{
    [TestFixture]
    public class OrderedObservableTest
    {
        [Test]
        public void Already_Ordered()
        {
            Observable.Range(1, 5)
                .OrderBy(v => v)
                .Test()
                .AssertResult(1, 2, 3, 4, 5);
        }

        [Test]
        public void Inverse()
        {
            Observable.Range(1, 5)
                .OrderBy(v => -v)
                .Test()
                .AssertResult(5, 4, 3, 2, 1);
        }

        [Test]
        public void Linq()
        {
            var enu = from x in Enumerable.Range(0, 5)
                      from y in Enumerable.Range(0, 5)
                      from z in Enumerable.Range(0, 5)
                      orderby x descending, y ascending, z descending
                      select Tuple.Create(x, y);

            var obs = from x in Observable.Range(0, 5)
                      from y in Observable.Range(0, 5)
                      from z in Observable.Range(0, 5)
                      orderby x descending, y ascending, z descending
                      select Tuple.Create(x, y);

            obs.Test()
                .AssertResult(enu.ToArray());
        }

        [Test]
        public void StableSort()
        {
            var rnd = new Random(0);
            var array = Enumerable.Range(0, 100).Select(x => Tuple.Create(rnd.Next(0, 10), x)).ToArray();

            array.ToObservable()
                .OrderBy(x => x.Item1)
                .Test()
                .AssertResult(array.OrderBy(x => x.Item1).ToArray());
        }

        [Test]
        public void ThrowingSelector()
        {
            int Selector(int x) => throw new Exception("test");

            Observable.Range(0, 4)
                .OrderBy(Selector)
                .Test()
                .AssertError(typeof(Exception), "test");
        }

        class IntThrowingComparer : IComparer<int>
        {
            public int Compare(int x, int y) => throw new NotImplementedException("test");
        }

        [Test]
        public void ThrowingComparer()
        {
            Observable.Range(0, 4)
                .OrderBy(x => x, new IntThrowingComparer())
                .Test()
                .AssertError(typeof(NotImplementedException), "test");
        }
    }
}
