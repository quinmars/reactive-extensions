﻿using NUnit.Framework;
using System;
using akarnokd.reactive_extensions;
using System.Reactive.Concurrency;

namespace akarnokd.reactive_extensions_test.maybe
{
    [TestFixture]
    public class MaybeDelaySubscriptionTest
    {
        [Test]
        public void Time_Basic()
        {
            MaybeSource.Empty<int>()
                .DelaySubscription(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default)
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult();
        }

        [Test]
        public void Time_Success()
        {
            MaybeSource.Just(1)
                .DelaySubscription(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default)
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult(1);
        }

        [Test]
        public void Time_Error()
        {
            MaybeSource.Error<int>(new InvalidOperationException())
                .DelaySubscription(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default)
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertFailure(typeof(InvalidOperationException));
        }

        [Test]
        public void Time_Dispose()
        {
            var ts = new TestScheduler();

            var to = MaybeSource.Empty<int>()
                .DelaySubscription(TimeSpan.FromSeconds(1), ts)
                .Test();

            ts.AdvanceTimeBy(500);

            to.Dispose();

            ts.AdvanceTimeBy(500);

            to.AssertEmpty();
        }

        [Test]
        public void Time_Dispose_Other()
        {
            var ts = new TestScheduler();

            var cs = new MaybeSubject<int>();

            var to = cs
                .DelaySubscription(TimeSpan.FromSeconds(1), ts)
                .Test();

            Assert.False(cs.HasObserver());

            ts.AdvanceTimeBy(500);

            Assert.False(cs.HasObserver());

            ts.AdvanceTimeBy(500);

            Assert.True(cs.HasObserver());

            to.Dispose();

            Assert.False(cs.HasObserver());

            to.AssertEmpty();
        }

        [Test]
        public void Other_Basic()
        {
            MaybeSource.Empty<int>()
                .DelaySubscription(MaybeSource.Timer(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default))
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult();
        }

        [Test]
        public void Other_Empty_Empty()
        {
            MaybeSource.Empty<int>()
                .DelaySubscription(MaybeSource.Empty<string>())
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult();
        }

        [Test]
        public void Other_Success_Empty()
        {
            MaybeSource.Just(1)
                .DelaySubscription(MaybeSource.Empty<string>())
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult(1);
        }

        [Test]
        public void Other_Success()
        {
            MaybeSource.Just(1)
                .DelaySubscription(MaybeSource.Timer(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default))
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertResult(1);
        }

        [Test]
        public void Other_Error()
        {
            MaybeSource.Error<int>(new InvalidOperationException())
                .DelaySubscription(MaybeSource.Timer(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default))
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertFailure(typeof(InvalidOperationException));
        }

        [Test]
        public void Other_Delay_Error()
        {
            MaybeSource.Error<int>(new NullReferenceException())
                .DelaySubscription(MaybeSource.Error<int>(new InvalidOperationException()))
                .Test()
                .AwaitDone(TimeSpan.FromSeconds(5))
                .AssertFailure(typeof(InvalidOperationException));
        }

        [Test]
        public void Other_Dispose()
        {
            var ts = new MaybeSubject<int>();

            var to = MaybeSource.Empty<int>()
                .DelaySubscription(ts)
                .Test();

            Assert.True(ts.HasObserver());

            to.Dispose();

            Assert.False(ts.HasObserver());

            to.AssertEmpty();
        }

        [Test]
        public void Other_Dispose_Other()
        {
            var ts = new MaybeSubject<int>();

            var cs = new MaybeSubject<int>();

            var to = cs
                .DelaySubscription(ts)
                .Test();

            Assert.False(cs.HasObserver());

            ts.OnCompleted();

            Assert.True(cs.HasObserver());

            to.Dispose();

            Assert.False(cs.HasObserver());

            to.AssertEmpty();
        }
    }
}
