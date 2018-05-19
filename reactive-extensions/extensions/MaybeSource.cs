﻿using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static akarnokd.reactive_extensions.ValidationHelper;

namespace akarnokd.reactive_extensions
{
    /// <summary>
    /// Extension and factory methods for dealing with
    /// <see cref="IMaybeSource{T}"/>s.
    /// </summary>
    /// <remarks>Since 0.0.5</remarks>
    public static class MaybeSource
    {
        /// <summary>
        /// Test an observable by creating a TestObserver and subscribing 
        /// it to the <paramref name="source"/> maybe.
        /// </summary>
        /// <typeparam name="T">The value type of the source maybe.</typeparam>
        /// <param name="source">The source maybe to test.</param>
        /// <param name="dispose">Dispose the TestObserver before the subscription happens</param>
        /// <returns>The new TestObserver instance.</returns>
        public static TestObserver<T> Test<T>(this IMaybeSource<T> source, bool dispose = false)
        {
            RequireNonNull(source, nameof(source));
            var to = new TestObserver<T>();
            if (dispose)
            {
                to.Dispose();
            }
            source.Subscribe(to);
            return to;
        }

        //-------------------------------------------------
        // Factory methods
        //-------------------------------------------------

        /// <summary>
        /// Creates a maybe that calls the specified <paramref name="onSubscribe"/>
        /// action with a <see cref="IMaybeEmitter{T}"/> to allow
        /// bridging the callback world with the reactive world.
        /// </summary>
        /// <param name="onSubscribe">The action that is called with an emitter
        /// that can be used for signalling an item, completion or error event.</param>
        /// <returns>The new maybe instance</returns>
        public static IMaybeSource<T> Create<T>(Action<IMaybeEmitter<T>> onSubscribe)
        {
            RequireNonNull(onSubscribe, nameof(onSubscribe));

            return new MaybeCreate<T>(onSubscribe);
        }

        /// <summary>
        /// Creates an empty completable that completes immediately.
        /// </summary>
        /// <typeparam name="T">The element type of the maybe.</typeparam>
        /// <returns>The shared empty maybe source instance.</returns>
        public static IMaybeSource<T> Empty<T>()
        {
            return MaybeEmpty<T>.INSTANCE;
        }

        /// <summary>
        /// Creates a failing completable that signals the specified error
        /// immediately.
        /// </summary>
        /// <typeparam name="T">The element type of the maybe.</typeparam>
        /// <param name="error">The error to signal.</param>
        /// <returns>The new maybe source instance.</returns>
        public static IMaybeSource<T> Error<T>(Exception error)
        {
            RequireNonNull(error, nameof(error));

            return new MaybeError<T>(error);
        }

        /// <summary>
        /// Creates a maybe that never terminates.
        /// </summary>
        /// <typeparam name="T">The element type of the maybe.</typeparam>
        /// <returns>The shared never-terminating maybe instance.</returns>
        public static IMaybeSource<T> Never<T>()
        {
            return MaybeNever<T>.INSTANCE;
        }

        /// <summary>
        /// Creates a maybe that succeeds with the given <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <param name="item">The item to succeed with.</param>
        /// <returns>The new maybe source instance.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static IMaybeSource<T> Just<T>(T item)
        {
            return new MaybeJust<T>(item);
        }

        public static IMaybeSource<T> FromAction<T>(Action action)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> FromFunc<T>(Func<T> action)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> FromTask<T>(Task task)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> FromTask<T>(Task<T> task)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> AmbAll<T>(this IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Amb<T>(params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatAll<T>(this IMaybeSource<T>[] sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Concat<T>(params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Concat<T>(IEnumerable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Concat<T>(int maxConcurrency, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Concat<T>(int maxConcurrency, bool delayErrors, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Concat<T>(this IObservable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEagerAll<T>(this IMaybeSource<T>[] sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEager<T>(params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEager<T>(IEnumerable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEager<T>(int maxConcurrency, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEager<T>(int maxConcurrency, bool delayErrors, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> ConcatEager<T>(this IObservable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Defer<T>(Func<IMaybeSource<T>> supplier)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> MergeAll<T>(this IMaybeSource<T>[] sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Merge<T>(params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Merge<T>(IEnumerable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Merge<T>(int maxConcurrency, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Merge<T>(int maxConcurrency, bool delayErrors, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IObservable<T> Merge<T>(this IObservable<IMaybeSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<long> Timer(TimeSpan time, IScheduler scheduler)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Using<T, S>(Func<S> stateFactory, Func<S, IMaybeSource<T>> sourceSelector, Action<S> stateCleanup = null, bool eagerCleanup = false)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<R> Zip<T, R>(Func<T[], R> mapper, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<R> Zip<T, R>(Func<T[], R> mapper, bool delayErrors, params IMaybeSource<T>[] sources)
        {
            throw new NotImplementedException();
        }

        public static IMaybeSource<R> Zip<T, R>(this IMaybeSource<T>[] sources, Func<T[], R> mapper, bool delayErrors = false)
        {
            throw new NotImplementedException();
        }

        //-------------------------------------------------
        // Instance methods
        //-------------------------------------------------

        /// <summary>
        /// Applies a function to the source at assembly-time and returns the
        /// maybe source returned by this function.
        /// This allows creating reusable set of operators to be applied to maybe sources.
        /// </summary>
        /// <typeparam name="T">The upstream element type.</typeparam>
        /// <typeparam name="R">The element type of the returned maybe source.</typeparam>
        /// <param name="source">The upstream maybe source.</param>
        /// <param name="composer">The function called immediately on <paramref name="source"/>
        /// and should return a maybe source.</param>
        /// <returns>The maybe source returned by the <paramref name="composer"/> function.</returns>
        public static IMaybeSource<R> Compose<T, R>(this IMaybeSource<T> source, Func<IMaybeSource<T>, IMaybeSource<R>> composer)
        {
            return composer(source);
        }

        public static IMaybeSource<T> DoOnSubscribe<T>(this IMaybeSource<T> source, Action<IDisposable> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoOnDispose<T>(this IMaybeSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoOnCompleted<T>(this IMaybeSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoOnSuccess<T>(this IMaybeSource<T> source, Action<T> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoOnError<T>(this IMaybeSource<T> source, Action<Exception> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoOnTerminate<T>(this IMaybeSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoAfterTerminate<T>(this IMaybeSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DoFinally<T>(this IMaybeSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Timeout<T>(this IMaybeSource<T> source, TimeSpan time, IScheduler scheduler, IMaybeSource<T> fallback = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> OnErrorComplete<T>(this IMaybeSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> OnErrorResumeNext<T>(this IMaybeSource<T> source, IMaybeSource<T> fallback)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> OnErrorResumeNext<T>(this IMaybeSource<T> source, Func<Exception, IMaybeSource<T>> handler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IObservable<T> Repeat<T>(this IMaybeSource<T> source, long times = long.MaxValue)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IObservable<T> Repeat<T>(this IMaybeSource<T> source, Func<bool> handler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IObservable<T> RepeatWhen<T, U>(this IMaybeSource<T> source, Func<IObservable<object>, IObservable<U>> handler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Retry<T>(this IMaybeSource<T> source, long times = long.MaxValue)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Retry<T>(this IMaybeSource<T> source, Func<Exception, long, bool> handler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> RetryWhen<T, U>(this IMaybeSource<T> source, Func<IObservable<Exception>, IObservable<U>> handler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> SubscribeOn<T>(this IMaybeSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> ObserveOn<T>(this IMaybeSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> UnsubscribeOn<T>(this IMaybeSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> OnTerminateDetach<T>(this IMaybeSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Cache<T>(this IMaybeSource<T> source, Action<IDisposable> cancel = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> Delay<T>(this IMaybeSource<T> source, TimeSpan time, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DelaySubscription<T>(this IMaybeSource<T> source, TimeSpan time, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> DelaySubscription<T>(this IMaybeSource<T> source, ICompletableSource other)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> TakeUntil<T>(this IMaybeSource<T> source, ICompletableSource other)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> TakeUntil<T, U>(this IMaybeSource<T> source, IObservable<U> other)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<R> Map<T, R>(this IMaybeSource<T> source, Func<T, R> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }


        public static IMaybeSource<R> FlatMap<T, R>(this IMaybeSource<T> source, Func<T, IMaybeSource<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IObservable<R> FlatMap<T, R>(this IMaybeSource<T> source, Func<T, IEnumerable<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IObservable<R> FlatMap<T, R>(this IMaybeSource<T> source, Func<T, IObservable<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static ISingleSource<R> FlatMap<T, R>(this IMaybeSource<T> source, Func<T, ISingleSource<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static ISingleSource<T> DefaultIfEmpty<T>(this IMaybeSource<T> source, T defaultItem)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> SwitchIfEmpty<T>(this IMaybeSource<T> source, params IMaybeSource<T>[] fallbacks)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> SwitchIfEmpty<T>(this IMaybeSource<T> source, ISingleSource<T> fallback)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Hides the identity and disposable of the upstream from
        /// the downstream.
        /// </summary>
        /// <param name="source">The maybe source to hide.</param>
        /// <returns>The new maybe source instance.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static IMaybeSource<T> Hide<T>(this IMaybeSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new MaybeHide<T>(source);
        }


        // ------------------------------------------------
        // Leaving the reactive world
        // ------------------------------------------------

        public static void SubscribeSafe<T>(this IMaybeSource<T> source, IMaybeObserver<T> observer)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Subscribe to this maybe source and call the
        /// appropriate action depending on the success or terminal signal received.
        /// </summary>
        /// <param name="source">The maybee source to observe.</param>
        /// <param name="onSuccess">Called with the success item when the maybe source succeeds.</param>
        /// <param name="onError">Called with the exception when the maybe source terminates with an error.</param>
        /// <param name="onCompleted">Called when the maybe source completes normally.</param>
        /// <returns>The disposable that allows cancelling the source.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static IDisposable Subscribe<T>(this IMaybeSource<T> source, Action<T> onSuccess = null, Action<Exception> onError = null, Action onCompleted = null)
        {
            RequireNonNull(source, nameof(source));

            var parent = new MaybeLambdaObserver<T>(onSuccess, onError, onCompleted);
            source.Subscribe(parent);
            return parent;
        }

        public static void BlockingSubscribe<T>(this IMaybeSource<T> source, IMaybeObserver<T> observer)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static void BlockingSubscribe<T>(this IMaybeSource<T> source, Action<T> onSuccess, Action<Exception> onError = null, Action onCompleted = null, Action <IDisposable> onSubscribe = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static void Wait<T>(this IMaybeSource<T> source, long timeoutMillis = long.MinValue, CancellationTokenSource cts = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static bool Wait<T>(this IMaybeSource<T> source, out T result, long timeoutMillis = long.MinValue, CancellationTokenSource cts = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static Task<T> ToTask<T>(this IMaybeSource<T> source, CancellationTokenSource cts = null)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        /// <summary>
        /// Subscribes a maybe observer (subclass) to the maybe
        /// source and returns this observer instance as well.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <typeparam name="U">The observer type.</typeparam>
        /// <param name="source">The maybe source to subscribe to.</param>
        /// <param name="observer">The maybe observer (subclass) to subscribe with.</param>
        /// <returns>The <paramref name="observer"/> provided as parameter.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static U SubscribeWith<T, U>(this IMaybeSource<T> source, U observer) where U : IMaybeObserver<T>
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(observer, nameof(observer));

            source.Subscribe(observer);
            return observer;
        }

        //-------------------------------------------------
        // Interoperation with other reactive types
        //-------------------------------------------------

        public static IObservable<R> ConcatMap<T, R>(this IObservable<T> source, Func<T, IMaybeSource<T>> mapper, bool delayErrors = false)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IObservable<R> FlatMap<T, R>(this IObservable<T> source, Func<T, IMaybeSource<R>> mapper, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IMaybeSource<R> FlatMap<T, R>(this ISingleSource<T> source, Func<T, IMaybeSource<T>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IObservable<R> SwitchMap<T, R>(this IObservable<T> source, Func<T, IMaybeSource<T>> mapper, bool delayErrors = false)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> FirstElement<T>(this IObservable<T> source)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> SingleElement<T>(this IObservable<T> source)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> LastElement<T>(this IObservable<T> source)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }

        public static IMaybeSource<T> ElementAtIndex<T>(this IObservable<T> source, long index)
        {
            RequireNonNull(source, nameof(source));

            throw new NotImplementedException();
        }
    }
}
