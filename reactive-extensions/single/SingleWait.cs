﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace akarnokd.reactive_extensions
{

    /// <summary>
    /// Wait until the upstream terminates and rethrow any exception.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <remarks>Since 0.0.11</remarks>
    internal sealed class SingleWaitValue<T> : CountdownEvent, ISingleObserver<T>
    {
        IDisposable upstream;

        Exception error;

        T value;

        public SingleWaitValue() : base(1)
        {
        }

        public void OnError(Exception error)
        {
            this.error = error;
            Signal();
        }

        public void OnSubscribe(IDisposable d)
        {
            DisposableHelper.SetOnce(ref upstream, d);
        }

        public void OnSuccess(T item)
        {
            value = item;
            Signal();
        }

        void DisposeUpstream()
        {
            DisposableHelper.Dispose(ref upstream);
        }

        public T Wait(int timeout, CancellationTokenSource cts)
        {
            if (CurrentCount != 0)
            {
                if (cts != null)
                {
                    if (timeout == int.MaxValue)
                    {
                        try
                        {
                            base.Wait(cts.Token);
                        }
                        catch
                        {
                            DisposeUpstream();
                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!base.Wait(timeout, cts.Token))
                            {
                                throw new TimeoutException();
                            }
                        }
                        catch
                        {
                            DisposeUpstream();
                            throw;
                        }
                    }
                }
                else
                {
                    if (timeout == int.MaxValue)
                    {
                        try
                        {
                            base.Wait();
                        }
                        catch
                        {
                            DisposeUpstream();
                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!base.Wait(timeout))
                            {
                                throw new TimeoutException();
                            }
                        }
                        catch
                        {
                            DisposeUpstream();
                            throw;
                        }
                    }
                }
            }
            var ex = error;
            if (ex != null)
            {
                throw ex;
            }
            return value;
        }
    }
}
