﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace akarnokd.reactive_extensions
{
    internal sealed class ObservableSourceReplay<T> : IConnectableObservableSource<T>
    {
        readonly IObservableSource<T> source;

        CacheSubject<T> connection;

        public ObservableSourceReplay(IObservableSource<T> source)
        {
            this.source = source;
        }

        public IDisposable Connect(Action<IDisposable> onConnect = null)
        {
            for (; ; )
            {
                var subject = Volatile.Read(ref connection);
                if (subject == null)
                {
                    subject = new CacheSubject<T>();
                    if (Interlocked.CompareExchange(ref connection, subject, null) != null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (subject.HasException() || subject.HasCompleted() || subject.IsDisposed())
                    {
                        Interlocked.CompareExchange(ref connection, null, subject);
                        continue;
                    }
                }

                var shouldConnect = subject.Prepare();

                onConnect?.Invoke(subject);

                if (shouldConnect)
                {
                    source.Subscribe(subject);
                }

                return subject;
            }
        }

        public void Reset()
        {
            var subject = Volatile.Read(ref connection);
            if (subject != null) {
                if (subject.HasException() || subject.HasCompleted() || subject.IsDisposed())
                {
                    Interlocked.CompareExchange(ref connection, null, subject);
                }
            }
        }

        public void Subscribe(ISignalObserver<T> observer)
        {
            for (; ; )
            {
                var subject = Volatile.Read(ref connection);
                if (subject == null)
                {
                    subject = new CacheSubject<T>();
                    if (Interlocked.CompareExchange(ref connection, subject, null) != null)
                    {
                        continue;
                    }
                }

                subject.Subscribe(observer);

                break;
            }
        }
    }
}
