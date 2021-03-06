﻿using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;

namespace akarnokd.reactive_extensions
{
    /// <summary>
    /// Signals the terminal events of the single source
    /// through the specified scheduler.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <remarks>Since 0.0.11</remarks>
    internal sealed class SingleObserveOn<T> : ISingleSource<T>
    {
        readonly ISingleSource<T> source;

        readonly IScheduler scheduler;

        public SingleObserveOn(ISingleSource<T> source, IScheduler scheduler)
        {
            this.source = source;
            this.scheduler = scheduler;
        }

        public void Subscribe(ISingleObserver<T> observer)
        {
            source.Subscribe(new ObserveOnObserver(observer, scheduler));
        }

        sealed class ObserveOnObserver : ISingleObserver<T>, IDisposable
        {
            readonly ISingleObserver<T> downstream;

            readonly IScheduler scheduler;

            IDisposable upstream;

            IDisposable task;

            Exception error;

            T value;

            static readonly Func<IScheduler, ObserveOnObserver, IDisposable> RUN =
                (s, t) => { t.Run(); return DisposableHelper.EMPTY; };

            public ObserveOnObserver(ISingleObserver<T> downstream, IScheduler scheduler)
            {
                this.downstream = downstream;
                this.scheduler = scheduler;
            }

            public void Dispose()
            {
                upstream.Dispose();
                DisposableHelper.Dispose(ref task);
            }

            public void OnCompleted()
            {
                Schedule();
            }

            public void OnError(Exception error)
            {
                this.error = error;
                Schedule();
            }

            public void OnSuccess(T item)
            {
                this.value = item;
                Schedule();
            }

            void Schedule()
            {
                var d = Volatile.Read(ref task);
                if (d != DisposableHelper.DISPOSED)
                {
                    var u = scheduler.Schedule(this, RUN);

                    if (Interlocked.CompareExchange(ref task, u, d) == DisposableHelper.DISPOSED)
                    {
                        u.Dispose();
                    }
                }
            }

            public void OnSubscribe(IDisposable d)
            {
                upstream = d;
                downstream.OnSubscribe(this);
            }

            internal void Run()
            {
                for (; ; )
                {
                    var d = Volatile.Read(ref task);
                    if (d == DisposableHelper.DISPOSED)
                    {
                        break;
                    }
                    if (Interlocked.CompareExchange(ref task, DisposableHelper.EMPTY, d) == d)
                    {
                        var ex = error;
                        if (ex != null)
                        {
                            downstream.OnError(ex);
                        }
                        else
                        {
                            downstream.OnSuccess(value);
                        }
                        break;
                    }
                }
            }
        }
    }
}
