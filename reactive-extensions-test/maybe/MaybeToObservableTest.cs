﻿using NUnit.Framework;
using System;
using akarnokd.reactive_extensions;

namespace akarnokd.reactive_extensions_test.maybe
{
    [TestFixture]
    public class MaybeToObservableTest
    {
        [Test]
        public void Basic()
        {
            // do not make this var to ensure the target type is correct
            IObservable<int> o = MaybeSource.Empty<int>().ToObservable<int>();

            o.Test().AssertResult();
        }

        [Test]
        public void Success()
        {
            // do not make this var to ensure the target type is correct
            IObservable<int> o = MaybeSource.Just(1).ToObservable<int>();

            o.Test().AssertResult(1);
        }

        [Test]
        public void Error()
        {
            // do not make this var to ensure the target type is correct
            IObservable<int> o = MaybeSource.Error<int>(new InvalidOperationException()).ToObservable<int>();

            o.Test().AssertFailure(typeof(InvalidOperationException));
        }

        [Test]
        public void Disposed()
        {
            var up = new MaybeSubject<int>();

            IObservable<int> o = up.ToObservable<int>();

            var to = o.Test();

            Assert.True(up.HasObserver());

            to.Dispose();

            Assert.False(up.HasObserver());

        }
    }
}
