﻿using NUnit.Framework;
using System;
using akarnokd.reactive_extensions;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace akarnokd.reactive_extensions_test.observablesource
{
    [TestFixture]
    public class ObservableSourceSwitchMapTest
    {
        [Test]
        public void Basic()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => ObservableSource.Range(v * 100, 5).Hide()).Test();

            to.AssertEmpty();

            us.EmitAll(1, 2, 3, 4, 5);

            to.AssertResult(
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }

        [Test]
        public void Basic_Fused()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => ObservableSource.Range(v * 100, 5)).Test();

            to.AssertEmpty();

            us.EmitAll(1, 2, 3, 4, 5);

            to.AssertResult(
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }
        [Test]
        public void Basic_DelayError()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => ObservableSource.Range(v * 100, 5), delayErrors: true).Test();

            to.AssertEmpty();

            us.EmitAll(1, 2, 3, 4, 5);

            to.AssertResult(
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }

        [Test]
        public void Mapper_Crash()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap<int, int>(v => throw new InvalidOperationException()).Test();

            Assert.True(us.HasObservers);

            to.AssertEmpty();

            us.OnNext(1);

            Assert.False(us.HasObservers);

            to.AssertFailure(typeof(InvalidOperationException));
        }

        [Test]
        public void Error_Outer()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => ObservableSource.Range(v * 100, 5)).Test();

            to.AssertEmpty();

            us.EmitError(new InvalidOperationException(), 1, 2, 3, 4, 5);

            to.AssertFailure(
                typeof(InvalidOperationException),
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }

        [Test]
        public void Error_Outer_DelayError()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => ObservableSource.Range(v * 100, 5), delayErrors: true).Test();

            to.AssertEmpty();

            us.EmitError(new InvalidOperationException(), 1, 2, 3, 4, 5);

            to.AssertFailure(
                typeof(InvalidOperationException),
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }

        [Test]
        public void Error_Inner()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => {
                if (v > 3)
                {
                    return ObservableSource.Error<int>(new InvalidOperationException());
                }
                return ObservableSource.Range(v * 100, 5);
            }).Test();

            to.AssertEmpty();

            us.EmitError(new InvalidOperationException(), 1, 2, 3, 4, 5);

            to.AssertFailure(
                typeof(InvalidOperationException),
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                300, 301, 302, 303, 304
            );
        }

        [Test]
        public void Error_Inner_DelayError()
        {
            var us = new PublishSubject<int>();

            var to = us.SwitchMap(v => {
                if (v == 3)
                {
                    return ObservableSource.Error<int>(new InvalidOperationException());
                }
                return ObservableSource.Range(v * 100, 5);
            }, delayErrors: true).Test();

            to.AssertEmpty();

            us.EmitAll(1, 2, 3, 4, 5);

            to.AssertFailure(
                typeof(InvalidOperationException),
                100, 101, 102, 103, 104,
                200, 201, 202, 203, 204,
                400, 401, 402, 403, 404,
                500, 501, 502, 503, 504
            );
        }

        [Test]
        public void Switch_Inner_Completes_First()
        {
            var source = new PublishSubject<IObservableSource<int>>();

            var to = source.Switch().Test();

            to.AssertEmpty();

            var us1 = new MonocastSubject<int>();

            source.OnNext(us1);

            Assert.True(us1.HasObserver());

            var us2 = new MonocastSubject<int>();

            source.OnNext(us2);

            Assert.True(us2.HasObserver());
            Assert.False(us1.HasObserver());

            us2.EmitAll(1, 2, 3, 4, 5);

            to.AssertValuesOnly(1, 2, 3, 4, 5);

            source.OnCompleted();

            to.AssertResult(1, 2, 3, 4, 5);
        }

        [Test]
        public void Switch_Outer_Completes_First()
        {
            var source = new PublishSubject<IObservableSource<int>>();

            var to = source.Switch().Test();

            to.AssertEmpty();

            var us1 = new MonocastSubject<int>();

            source.OnNext(us1);

            Assert.True(us1.HasObserver());

            var us2 = new MonocastSubject<int>();

            source.OnNext(us2);

            Assert.True(us2.HasObserver());
            Assert.False(us1.HasObserver());

            source.OnCompleted();

            Assert.True(us2.HasObserver());

            to.AssertEmpty();

            us2.EmitAll(1, 2, 3, 4, 5);

            to.AssertResult(1, 2, 3, 4, 5);
        }
    }
}
