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
    /// <see cref="ISingleSource{T}"/>s.
    /// </summary>
    /// <remarks>Since 0.0.5</remarks>
    public static class SingleSource
    {
        /// <summary>
        /// Test an observable by creating a TestObserver and subscribing 
        /// it to the <paramref name="source"/> single.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The source single to test.</param>
        /// <param name="dispose">Dispose the TestObserver before the subscription happens</param>
        /// <returns>The new TestObserver instance.</returns>
        public static TestObserver<T> Test<T>(this ISingleSource<T> source, bool dispose = false)
        {
            RequireNonNull(source, nameof(source));
            var to = new TestObserver<T>(true);
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
        /// Creates a single that calls the specified <paramref name="onSubscribe"/>
        /// action with a <see cref="ISingleEmitter{T}"/> to allow
        /// bridging the callback world with the reactive world.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="onSubscribe">The action that is called with an emitter
        /// that can be used for signaling an item, completion or error event.</param>
        /// <returns>The new single instance</returns>
        public static ISingleSource<T> Create<T>(Action<ISingleEmitter<T>> onSubscribe)
        {
            RequireNonNull(onSubscribe, nameof(onSubscribe));

            return new SingleCreate<T>(onSubscribe);
        }

        /// <summary>
        /// Creates a failing single that signals the specified error
        /// immediately.
        /// </summary>
        /// <typeparam name="T">The element type of the single.</typeparam>
        /// <param name="error">The error to signal.</param>
        /// <returns>The new single source instance.</returns>
        public static ISingleSource<T> Error<T>(Exception error)
        {
            RequireNonNull(error, nameof(error));

            return new SingleError<T>(error);
        }

        /// <summary>
        /// Creates a single that never terminates.
        /// </summary>
        /// <typeparam name="T">The element type of the single.</typeparam>
        /// <returns>The shared never-terminating single instance.</returns>
        public static ISingleSource<T> Never<T>()
        {
            return SingleNever<T>.INSTANCE;
        }

        /// <summary>
        /// Creates a single that succeeds with the given <paramref name="item"/>.
        /// </summary>
        /// <typeparam name="T">The type of the single item.</typeparam>
        /// <param name="item">The item to succeed with.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static ISingleSource<T> Just<T>(T item)
        {
            return new SingleJust<T>(item);
        }

        /// <summary>
        /// Wraps and runs a function for each incoming
        /// single observer and signals the value returned
        /// by the function as the success event.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="func">The function to call for each observer.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> FromFunc<T>(Func<T> func)
        {
            RequireNonNull(func, nameof(func));

            return new SingleFromFunc<T>(func);
        }

        /// <summary>
        /// Creates a single source that succeeds or fails
        /// its observers when the given (possibly still ongoing)
        /// task terminates.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="task">The task to wrap.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> FromTask<T>(Task<T> task)
        {
            return task.ToSingle();
        }

        /// <summary>
        /// Relays the signals of the first single source to respond
        /// and disposes the other sources.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> AmbAll<T>(this ISingleSource<T>[] sources)
        {
            RequireNonNull(sources, nameof(sources));

            return new SingleAmb<T>(sources);
        }

        /// <summary>
        /// Relays the signals of the first single source to respond
        /// and disposes the other sources.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> Amb<T>(params ISingleSource<T>[] sources)
        {
            RequireNonNull(sources, nameof(sources));

            return AmbAll(sources);
        }

        /// <summary>
        /// Relays the signals of the first single source to respond
        /// and disposes the other sources.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The enumerable sequence of single sources.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> Amb<T>(this IEnumerable<ISingleSource<T>> sources)
        {
            RequireNonNull(sources, nameof(sources));

            return new SingleAmbEnumerable<T>(sources);
        }

        /// <summary>
        /// Runs each source single and emits their success items in order,
        /// optionally delaying errors from all of them until
        /// all terminate.
        /// </summary>
        /// <typeparam name="T">The success and result value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatAll<T>(this ISingleSource<T>[] sources, bool delayErrors = false)
        {
            RequireNonNull(sources, nameof(sources));

            return new SingleConcat<T>(sources, delayErrors);
        }

        /// <summary>
        /// Runs each source single and emits their success items in order.
        /// </summary>
        /// <typeparam name="T">The success and result value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Concat<T>(params ISingleSource<T>[] sources)
        {
            return ConcatAll(sources, false);
        }

        /// <summary>
        /// Runs each source single and emits their success items in order,
        /// optionally delaying errors from all of them until
        /// all terminate.
        /// </summary>
        /// <typeparam name="T">The success and result value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Concat<T>(bool delayErrors, params ISingleSource<T>[] sources)
        {
            return ConcatAll(sources, delayErrors);
        }

        /// <summary>
        /// Runs each source single returned by the enumerable sequence
        /// and emits their success items in order,
        /// optionally delaying errors from all of them until
        /// all terminate.
        /// </summary>
        /// <typeparam name="T">The success and result value type.</typeparam>
        /// <param name="sources">The enumerable sequence of single sources.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Concat<T>(this IEnumerable<ISingleSource<T>> sources, bool delayErrors = false)
        {
            RequireNonNull(sources, nameof(sources));

            return new SingleConcatEnumerable<T>(sources, delayErrors);
        }

        /// <summary>
        /// Runs each inner single source produced by the observable sequence
        /// and emits their success items in order,
        /// optionally delaying errors from all of them until
        /// all terminate.
        /// </summary>
        /// <typeparam name="T">The success and result value type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Concat<T>(this IObservable<ISingleSource<T>> sources, bool delayErrors = false)
        {
            return ConcatMap(sources, v => v, delayErrors);
        }

        /// <summary>
        /// Runs some or all single sources at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="sources">The array of single sources to run eagerly.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEagerAll<T>(this ISingleSource<T>[] sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(sources, nameof(sources));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleConcatEager<T>(sources, maxConcurrency, delayErrors);
        }

        /// <summary>
        /// Runs all single sources at once but emits
        /// their success item in order.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The array of single sources to run eagerly.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(params ISingleSource<T>[] sources)
        {
            return ConcatEagerAll(sources);
        }

        /// <summary>
        /// Runs some or all single sources, produced by
        /// an enumerable sequence, at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The enumerable sequence of single sources to run eagerly.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(this IEnumerable<ISingleSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(sources, nameof(sources));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleConcatEagerEnumerable<T>(sources, maxConcurrency, delayErrors);
        }

        /// <summary>
        /// Runs some or all single sources at once but emits
        /// their success item in order.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <param name="sources">The array of single sources to run eagerly.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(int maxConcurrency, params ISingleSource<T>[] sources)
        {
            return ConcatEagerAll(sources, false, maxConcurrency);
        }

        /// <summary>
        /// Runs some or all single sources at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="sources">The array of single sources to run eagerly.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(bool delayErrors, int maxConcurrency, params ISingleSource<T>[] sources)
        {
            return ConcatEagerAll(sources, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Runs all single sources at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="sources">The array of single sources to run eagerly.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(bool delayErrors, params ISingleSource<T>[] sources)
        {
            return ConcatEagerAll(sources, delayErrors);
        }

        /// <summary>
        /// Runs some or all single sources, provided by
        /// the observable sequence, at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="sources">The observable sequence of single sources to run eagerly.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ConcatEager<T>(this IObservable<ISingleSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            return ConcatMapEager(sources, v => v, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Defers the creation of the actual single source
        /// provided by a supplier function until a single observer completes.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="supplier">The function called for each individual single
        /// observer and should return a single source to subscribe to.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> Defer<T>(Func<ISingleSource<T>> supplier)
        {
            RequireNonNull(supplier, nameof(supplier));

            return new SingleDefer<T>(supplier);
        }

        /// <summary>
        /// Runs and merges some or all single sources into one observable sequence
        /// and optionally delays all errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">Delays errors until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> MergeAll<T>(this ISingleSource<T>[] sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(sources, nameof(sources));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleMerge<T>(sources, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Runs and merges all single sources into one observable sequence.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(params ISingleSource<T>[] sources)
        {
            return MergeAll(sources);
        }

        /// <summary>
        /// Runs and merges some or all single sources, provided by an enumerable sequence,
        /// into one observable sequence and optionally delays all errors 
        /// until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The enumerable sequence of single sources.</param>
        /// <param name="delayErrors">Delays errors until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(this IEnumerable<ISingleSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(sources, nameof(sources));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleMergeEnumerable<T>(sources, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Runs and merges some or all single sources into one observable sequence.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="maxConcurrency">The maximum number of inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(int maxConcurrency, params ISingleSource<T>[] sources)
        {
            return MergeAll(sources, maxConcurrency: maxConcurrency);
        }

        /// <summary>
        /// Runs and merges all single sources into one observable sequence
        /// and optionally delays all errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">Delays errors until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(bool delayErrors, params ISingleSource<T>[] sources)
        {
            return MergeAll(sources, delayErrors);
        }

        /// <summary>
        /// Runs and merges some or all single sources into one observable sequence
        /// and optionally delays all errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The array of single sources.</param>
        /// <param name="delayErrors">Delays errors until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(bool delayErrors, int maxConcurrency, params ISingleSource<T>[] sources)
        {
            return MergeAll(sources, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Runs and merges some or all single sources, provided by an observable sequence,
        /// into one observable sequence and optionally delays all errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The success element and result type.</typeparam>
        /// <param name="sources">The observable sequence of single sources.</param>
        /// <param name="delayErrors">Delays errors until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Merge<T>(this IObservable<ISingleSource<T>> sources, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            return FlatMap(sources, v => v, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Signals 0L after a specified time elapsed on the given scheduler.
        /// </summary>
        /// <param name="time">The time to wait before signaling a success value of 0L.</param>
        /// <param name="scheduler">The scheduler to use for emitting the success value.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<long> Timer(TimeSpan time, IScheduler scheduler)
        {
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleTimer(time, scheduler);
        }

        /// <summary>
        /// Generates a resource and a dependent single source
        /// for each single observer and cleans up the resource
        /// just before or just after the single source terminated
        /// or the observer has disposed the setup.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <typeparam name="S">The resource type.</typeparam>
        /// <param name="resourceSupplier">The supplier for a per-observer resource.</param>
        /// <param name="sourceSelector">Function that receives the per-observer resource returned
        /// by <paramref name="resourceSupplier"/> and returns a single source.</param>
        /// <param name="resourceCleanup">The optional callback for cleaning up the resource supplied by
        /// the <paramref name="resourceSupplier"/>.</param>
        /// <param name="eagerCleanup">If true, the per-observer resource is cleaned up before the
        /// terminal event is signaled to the downstream. If false, the cleanup happens after.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> Using<T, S>(Func<S> resourceSupplier, Func<S, ISingleSource<T>> sourceSelector, Action<S> resourceCleanup = null, bool eagerCleanup = true)
        {
            RequireNonNull(resourceSupplier, nameof(resourceSupplier));
            RequireNonNull(sourceSelector, nameof(sourceSelector));

            return new SingleUsing<T, S>(resourceSupplier, sourceSelector, resourceCleanup, eagerCleanup);
        }

        /// <summary>
        /// Waits for all single sources to produce a success item and
        /// calls the <paramref name="mapper"/> function to generate
        /// the output success value to be signaled to the downstream.
        /// </summary>
        /// <typeparam name="T">The success value type of the <paramref name="sources"/>.</typeparam>
        /// <typeparam name="R">The output success value type.</typeparam>
        /// <param name="mapper">The function receiving the success values of all the
        /// <paramref name="sources"/> and should return the result value to be
        /// signaled as the success value.</param>
        /// <param name="sources">The array of single sources to zip together.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12<br/>
        /// If any of the sources don't succeed, the other sources are disposed and
        /// the output is the completion/exception of that source.
        /// </remarks>
        public static ISingleSource<R> Zip<T, R>(Func<T[], R> mapper, params ISingleSource<T>[] sources)
        {
            return Zip(sources, mapper, false);
        }

        /// <summary>
        /// Waits for all single sources to produce a success item and
        /// calls the <paramref name="mapper"/> function to generate
        /// the output success value to be signaled to the downstream.
        /// </summary>
        /// <typeparam name="T">The success value type of the <paramref name="sources"/>.</typeparam>
        /// <typeparam name="R">The output success value type.</typeparam>
        /// <param name="mapper">The function receiving the success values of all the
        /// <paramref name="sources"/> and should return the result value to be
        /// signaled as the success value.</param>
        /// <param name="sources">The array of single sources to zip together.</param>
        /// <param name="delayErrors">If true, the operator waits for all
        /// sources to terminate, even if some of them didn't produce a success item
        /// and terminates with the aggregate signal. If false, the downstream
        /// is terminated with the terminal event of the first empty source.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<R> Zip<T, R>(Func<T[], R> mapper, bool delayErrors, params ISingleSource<T>[] sources)
        {
            return Zip(sources, mapper, delayErrors);
        }

        /// <summary>
        /// Waits for all single sources to produce a success item and
        /// calls the <paramref name="mapper"/> function to generate
        /// the output success value to be signaled to the downstream.
        /// </summary>
        /// <typeparam name="T">The success value type of the <paramref name="sources"/>.</typeparam>
        /// <typeparam name="R">The output success value type.</typeparam>
        /// <param name="mapper">The function receiving the success values of all the
        /// <paramref name="sources"/> and should return the result value to be
        /// signaled as the success value.</param>
        /// <param name="sources">The array of single sources to zip together.</param>
        /// <param name="delayErrors">If true, the operator waits for all
        /// sources to terminate, even if some of them didn't produce a success item
        /// and terminates with the aggregate signal. If false, the downstream
        /// is terminated with the terminal event of the first empty source.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<R> Zip<T, R>(this ISingleSource<T>[] sources, Func<T[], R> mapper, bool delayErrors = false)
        {
            RequireNonNull(sources, nameof(sources));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleZip<T, R>(sources, mapper, delayErrors);
        }

        /// <summary>
        /// Waits for all single sources to produce a success item and
        /// calls the <paramref name="mapper"/> function to generate
        /// the output success value to be signaled to the downstream.
        /// </summary>
        /// <typeparam name="T">The success value type of the <paramref name="sources"/>.</typeparam>
        /// <typeparam name="R">The output success value type.</typeparam>
        /// <param name="mapper">The function receiving the success values of all the
        /// <paramref name="sources"/> and should return the result value to be
        /// signaled as the success value.</param>
        /// <param name="sources">The enumerable sequence of single sources to zip together.</param>
        /// <param name="delayErrors">If true, the operator waits for all
        /// sources to terminate, even if some of them didn't produce a success item
        /// and terminates with the aggregate signal. If false, the downstream
        /// is terminated with the terminal event of the first empty source.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<R> Zip<T, R>(this IEnumerable<ISingleSource<T>> sources, Func<T[], R> mapper, bool delayErrors = false)
        {
            RequireNonNull(sources, nameof(sources));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleZipEnumerable<T, R>(sources, mapper, delayErrors);
        }

        //-------------------------------------------------
        // Instance methods
        //-------------------------------------------------

        /// <summary>
        /// Applies a function to the source at assembly-time and returns the
        /// single source returned by this function.
        /// This allows creating reusable set of operators to be applied to single sources.
        /// </summary>
        /// <typeparam name="T">The upstream element type.</typeparam>
        /// <typeparam name="R">The element type of the returned single source.</typeparam>
        /// <param name="source">The upstream single source.</param>
        /// <param name="composer">The function called immediately on <paramref name="source"/>
        /// and should return a single source.</param>
        /// <returns>The single source returned by the <paramref name="composer"/> function.</returns>
        public static ISingleSource<R> Compose<T, R>(this ISingleSource<T> source, Func<ISingleSource<T>, ISingleSource<R>> composer)
        {
            return composer(source);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> whenever the
        /// upstream single <paramref name="source"/> signals a success item.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoOnSuccess<T>(this ISingleSource<T> source, Action<T> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onSuccess: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> after the
        /// upstream single <paramref name="source"/>'s success
        /// item has been signaled to the downstream.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoAfterSuccess<T>(this ISingleSource<T> source, Action<T> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onAfterSuccess: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> whenever a
        /// single observer subscribes to the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoOnSubscribe<T>(this ISingleSource<T> source, Action<IDisposable> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onSubscribe: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> whenever a
        /// single observer disposes to the connection to
        /// the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoOnDispose<T>(this ISingleSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onDispose: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> before a
        /// single observer receives the error signal from
        /// the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoOnError<T>(this ISingleSource<T> source, Action<Exception> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onError: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> before a
        /// single observer gets terminated normally or with an error by
        /// the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoOnTerminate<T>(this ISingleSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onTerminate: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> after a
        /// single observer gets terminated normally or exceptionally by
        /// the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoAfterTerminate<T>(this ISingleSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, onAfterTerminate: handler);
        }

        /// <summary>
        /// Calls the given <paramref name="handler"/> exactly once per single
        /// observer and after the single observer gets terminated normally
        /// or exceptionally or the observer disposes the connection to the
        /// the single <paramref name="source"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to peek into.</param>
        /// <param name="handler">The handler to call.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DoFinally<T>(this ISingleSource<T> source, Action handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return SinglePeek<T>.Create(source, doFinally: handler);
        }

        /// <summary>
        /// If the upstream doesn't terminate within the specified
        /// timeout, the single observer is terminated with
        /// a <see cref="TimeoutException"/> or is switched to the optional
        /// <paramref name="fallback"/> single source.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to timeout.</param>
        /// <param name="timeout">The time to wait before canceling the source.</param>
        /// <param name="scheduler">The scheduler to use wait for the termination of the upstream.</param>
        /// <param name="fallback">The optional single source to switch to if the upstream times out.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> Timeout<T>(this ISingleSource<T> source, TimeSpan timeout, IScheduler scheduler, ISingleSource<T> fallback = null)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleTimeout<T>(source, timeout, scheduler, fallback);
        }

        /// <summary>
        /// Switches to a <paramref name="fallback"/> single source if
        /// the upstream fails.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source that can fail.</param>
        /// <param name="fallback">The fallback single source to resume with if <paramref name="source"/> fails.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> OnErrorResumeNext<T>(this ISingleSource<T> source, ISingleSource<T> fallback)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(fallback, nameof(fallback));

            return new SingleOnErrorResumeNext<T>(source, fallback);
        }

        /// <summary>
        /// Switches to a fallback single source provided
        /// by a handler function if the main single source fails.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source that can fail.</param>
        /// <param name="handler">The function that receives the exception from the main
        /// source and should return a fallback single source to resume with.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> OnErrorResumeNext<T>(this ISingleSource<T> source, Func<Exception, ISingleSource<T>> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return new SingleOnErrorResumeNextSelector<T>(source, handler);
        }

        /// <summary>
        /// Subscribe to a single source repeatedly if it
        /// succeeds or completes normally and
        /// emit its success items.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to subscribe to repeatedly.</param>
        /// <param name="times">The maximum number of times to resubscribe.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Repeat<T>(this ISingleSource<T> source, long times = long.MaxValue)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNegative(times, nameof(times));

            return new SingleRepeat<T>(source, times);
        }

        /// <summary>
        /// Subscribe to a single source repeatedly if it
        /// succeeds or completes normally and
        /// the given handler returns true.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to subscribe to repeatedly.</param>
        /// <param name="handler">The predicate called with the current repeat count
        /// and should return true to indicate repetition.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> Repeat<T>(this ISingleSource<T> source, Func<long, bool> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return new SingleRepeatPredicate<T>(source, handler);
        }

        /// <summary>
        /// Repeats (resubscribes to) the single after a success or completion and when the observable
        /// returned by a handler produces an arbitrary item.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="U">The arbitrary element type signaled by the handler observable.</typeparam>
        /// <param name="source">The single source to repeat while it successfully terminates.</param>
        /// <param name="handler">The function that is called for each observer and takes an observable sequence of
        /// errors. It should return an observable of arbitrary items that should signal that arbitrary item in
        /// response to receiving the completion signal from the source observable. If this observable signals
        /// a terminal event, the sequence is terminated with that signal instead.</param>
        /// <returns>An observable sequence producing the elements of the given single source repeatedly while it terminates successfully.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <remarks>Since 0.0.13</remarks>
        public static IObservable<T> RepeatWhen<T, U>(this ISingleSource<T> source, Func<IObservable<object>, IObservable<U>> handler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(handler, nameof(handler));

            return new SingleRepeatWhen<T, U>(source, handler);
        }

        /// <summary>
        /// Subscribe to a single source repeatedly (or up to a maximum
        /// number of times) if it keeps failing.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to subscribe to repeatedly.</param>
        /// <param name="times">The maximum number of times to resubscribe.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> Retry<T>(this ISingleSource<T> source, long times = long.MaxValue)
        {
            RequireNonNull(source, nameof(source));

            return new SingleRetry<T>(source, times);
        }

        /// <summary>
        /// Subscribe to a single source repeatedly if it keeps failing
        /// and the handler returns true upon a failure.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to subscribe to repeatedly.</param>
        /// <param name="handler">The predicate called with the current retry count
        /// and should return true to indicate repetition.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> Retry<T>(this ISingleSource<T> source, Func<Exception, long, bool> handler)
        {
            RequireNonNull(source, nameof(source));

            return new SingleRetryPredicate<T>(source, handler);
        }

        /// <summary>
        /// Retries (resubscribes to) the single source after a failure and when the observable
        /// returned by a handler produces an arbitrary item.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="U">The arbitrary element type signaled by the handler observable.</typeparam>
        /// <param name="source">The single source to repeat until it successfully terminates.</param>
        /// <param name="handler">The function that is called for each observer and takes an observable sequence of
        /// errors. It should return an observable of arbitrary items that should signal that arbitrary item in
        /// response to receiving the failure Exception from the source observable. If this observable signals
        /// a terminal event, the sequence is terminated with that signal instead.</param>
        /// <returns>A single source that retries a single source if it fails.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="handler"/> is null.</exception>
        /// <remarks>Since 0.0.13</remarks>
        public static ISingleSource<T> RetryWhen<T, U>(this ISingleSource<T> source, Func<IObservable<Exception>, IObservable<U>> handler)
        {
            RequireNonNull(source, nameof(source));

            return new SingleRetryWhen<T, U>(source, handler);
        }

        /// <summary>
        /// Subscribes to the source on the given scheduler.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The target single source to subscribe to</param>
        /// <param name="scheduler">The scheduler to use when subscribing to <paramref name="source"/>.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> SubscribeOn<T>(this ISingleSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleSubscribeOn<T>(source, scheduler);
        }

        /// <summary>
        /// Signals the terminal events of the single source
        /// through the specified <paramref name="scheduler"/>.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to observe on the specified scheduler.</param>
        /// <param name="scheduler">The scheduler to use.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> ObserveOn<T>(this ISingleSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleObserveOn<T>(source, scheduler);
        }

        /// <summary>
        /// When the downstream disposes, the upstream's disposable
        /// is called from the given scheduler.
        /// Note that termination in general doesn't call
        /// <code>Dispose()</code> on the upstream.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to dispose.</param>
        /// <param name="scheduler">The scheduler to use.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> UnsubscribeOn<T>(this ISingleSource<T> source, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleUnsubscribeOn<T>(source, scheduler);
        }

        /// <summary>
        /// When the upstream terminates or the downstream disposes,
        /// it detaches the references between the two, avoiding
        /// leaks of one or the other.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to detach from upon termination or cancellation.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> OnTerminateDetach<T>(this ISingleSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleOnTerminateDetach<T>(source);
        }

        /// <summary>
        /// Cache the terminal signal of the upstream
        /// and relay/replay it to current or future
        /// single observers.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The upstream single source to cache.</param>
        /// <param name="cancel">Called once when subscribing to the source
        /// upon the first subscriber.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static ISingleSource<T> Cache<T>(this ISingleSource<T> source, Action<IDisposable> cancel = null)
        {
            RequireNonNull(source, nameof(source));

            return new SingleCache<T>(source, cancel);
        }

        /// <summary>
        /// Delay the delivery of the terminal events from the
        /// upstream single source by the given time amount.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to delay signals of.</param>
        /// <param name="time">The time delay.</param>
        /// <param name="scheduler">The scheduler to use for the timed wait and signal emission.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> Delay<T>(this ISingleSource<T> source, TimeSpan time, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleDelay<T>(source, time, scheduler);
        }

        /// <summary>
        /// Delay the subscription to the main single source
        /// until the specified time elapsed.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to delay subscribing to.</param>
        /// <param name="time">The delay time.</param>
        /// <param name="scheduler">The scheduler to use for the timed wait and subscription.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DelaySubscription<T>(this ISingleSource<T> source, TimeSpan time, IScheduler scheduler)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(scheduler, nameof(scheduler));

            return new SingleDelaySubscriptionTime<T>(source, time, scheduler);
        }

        /// <summary>
        /// Delay the subscription to the main single source
        /// until the other source completes.
        /// </summary>
        /// <typeparam name="T">The success value type main source.</typeparam>
        /// <typeparam name="U">The success value type of the other source.</typeparam>
        /// <param name="source">The single source to delay subscribing to.</param>
        /// <param name="other">The source that should complete to trigger the main subscription.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> DelaySubscription<T, U>(this ISingleSource<T> source, ISingleSource<U> other)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(other, nameof(other));

            return new SingleDelaySubscription<T, U>(source, other);
        }

        /// <summary>
        /// Terminates when either the main or the other source terminates,
        /// disposing the other sequence.
        /// </summary>
        /// <typeparam name="T">The success value type main source.</typeparam>
        /// <typeparam name="U">The success value type of the other source.</typeparam>
        /// <param name="source">The main completable source to consume.</param>
        /// <param name="other">The other completable source that could stop the <paramref name="source"/>.</param>
        /// <returns>The new completable source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> TakeUntil<T, U>(this ISingleSource<T> source, ISingleSource<U> other)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(other, nameof(other));

            return new SingleTakeUntil<T, U>(source, other);
        }

        /// <summary>
        /// Terminates when either the main or the other source terminates,
        /// disposing the other sequence.
        /// </summary>
        /// <typeparam name="T">The success value type main source.</typeparam>
        /// <typeparam name="U">The success value type of the other source.</typeparam>
        /// <param name="source">The main completable source to consume.</param>
        /// <param name="other">The other observable that could stop the <paramref name="source"/>
        /// by emitting an item or completing.</param>
        /// <returns>The new completable source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> TakeUntil<T, U>(this ISingleSource<T> source, IObservable<U> other)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(other, nameof(other));

            return new SingleTakeUntilObservable<T, U>(source, other);
        }

        /// <summary>
        /// Maps the success value of the upstream single source
        /// into another value.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The result value type</typeparam>
        /// <param name="source">The upstream single source to map.</param>
        /// <param name="mapper">The function receiving the upstream success
        /// item and returns a new success item for the downstream.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<R> Map<T, R>(this ISingleSource<T> source, Func<T, R> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleMap<T, R>(source, mapper);
        }

        /// <summary>
        /// Tests the upstream's success value via a predicate
        /// and relays it if the predicate returns false,
        /// completed the downstream otherwise.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <param name="source">The upstream single source to map.</param>
        /// <param name="predicate">The function that receives the upstream
        /// success item and should return true if the success
        /// value should be passed along.</param>
        /// <returns>The new maybe source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static IMaybeSource<T> Filter<T>(this ISingleSource<T> source, Func<T, bool> predicate)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(predicate, nameof(predicate));

            return new SingleFilter<T>(source, predicate);
        }

        /// <summary>
        /// Maps the upstream success item into a single source,
        /// subscribes to it and relays its success or terminal signals
        /// to the downstream.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The value type of the inner single source.</typeparam>
        /// <param name="source">The single source to map onto another single source.</param>
        /// <param name="mapper">The function receiving the upstream success item
        /// and should return a single source to subscribe to.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<R> FlatMap<T, R>(this ISingleSource<T> source, Func<T, ISingleSource<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleFlatMapSingle<T, R>(source, mapper);
        }

        /// <summary>
        /// Maps the success value of the upstream
        /// single source onto an enumerable sequence
        /// and emits the items of this sequence.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <typeparam name="R">The element type of the enumerable sequence.</typeparam>
        /// <param name="source">The single source to map.</param>
        /// <param name="mapper">The function receiving the success item and
        /// should return an enumerable sequence.</param>
        /// <returns>The new observable sequence.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static IObservable<R> FlatMap<T, R>(this ISingleSource<T> source, Func<T, IEnumerable<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleFlatMapEnumerable<T, R>(source, mapper);
        }

        /// <summary>
        /// Maps the success value of the upstream
        /// single source onto an observable sequence
        /// and emits the items of this sequence.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <typeparam name="R">The element type of the enumerable sequence.</typeparam>
        /// <param name="source">The single source to map.</param>
        /// <param name="mapper">The function receiving the success item and
        /// should return an observable sequence.</param>
        /// <returns>The new observable sequence.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static IObservable<R> FlatMap<T, R>(this ISingleSource<T> source, Func<T, IObservable<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleFlatMapObservable<T, R>(source, mapper);
        }

        /// <summary>
        /// Hides the identity and disposable of the upstream from
        /// the downstream.
        /// </summary>
        /// <param name="source">The single source to hide.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static ISingleSource<T> Hide<T>(this ISingleSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleHide<T>(source);
        }


        // ------------------------------------------------
        // Leaving the reactive world
        // ------------------------------------------------

        /// <summary>
        /// Subscribes to this single source and suppresses exceptions
        /// throw by the OnXXX methods of the <paramref name="observer"/>.
        /// </summary>
        /// <param name="source">The single source to subscribe to safely.</param>
        /// <param name="observer">The unreliable observer.</param>
        /// <remarks>Since 0.0.11</remarks>
        public static void SubscribeSafe<T>(this ISingleSource<T> source, ISingleObserver<T> observer)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(observer, nameof(observer));

            source.Subscribe(new SingleSafeObserver<T>(observer));
        }

        /// <summary>
        /// Subscribe to this single source and call the
        /// appropriate action depending on the success or terminal signal received.
        /// </summary>
        /// <param name="source">The single source to observe.</param>
        /// <param name="onSuccess">Called with the success item when the single source succeeds.</param>
        /// <param name="onError">Called with the exception when the single source terminates with an error.</param>
        /// <returns>The disposable that allows canceling the source.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static IDisposable Subscribe<T>(this ISingleSource<T> source, Action<T> onSuccess = null, Action<Exception> onError = null)
        {
            RequireNonNull(source, nameof(source));

            var parent = new SingleLambdaObserver<T>(onSuccess, onError);
            source.Subscribe(parent);
            return parent;
        }

        /// <summary>
        /// Subscribes to the source and blocks until it terminated, then
        /// calls the appropriate , single observer method on the current
        /// thread.
        /// </summary>
        /// <param name="source">The upstream single source to block for.</param>
        /// <param name="observer">The single observer to call the methods on the current thread.</param>
        /// <remarks>Since 0.0.11</remarks>
        public static void BlockingSubscribe<T>(this ISingleSource<T> source, ISingleObserver<T> observer)
        {
            RequireNonNull(source, nameof(source));

            RequireNonNull(source, nameof(source));
            RequireNonNull(observer, nameof(observer));

            var parent = new SingleBlockingObserver<T>(observer);
            observer.OnSubscribe(parent);

            source.Subscribe(parent);

            parent.Run();
        }

        /// <summary>
        /// Subscribes to the source and blocks until it terminated, then
        /// calls the appropriate single observer method on the current
        /// thread.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The upstream single source to block for.</param>
        /// <param name="onSuccess">Action called with the success item.</param>
        /// <param name="onError">Action called with the exception when the upstream fails.</param>
        /// <param name="onSubscribe">Action called with a disposable just before subscribing to the upstream
        /// and allows disposing the sequence and unblocking this method call.</param>
        /// <remarks>Since 0.0.11</remarks>
        public static void BlockingSubscribe<T>(this ISingleSource<T> source, Action<T> onSuccess = null, Action<Exception> onError = null, Action<IDisposable> onSubscribe = null)
        {
            RequireNonNull(source, nameof(source));

            var parent = new SingleBlockingConsumer<T>(onSuccess, onError);
            onSubscribe?.Invoke(parent);

            source.Subscribe(parent);

            parent.Run();
        }

        /// <summary>
        /// Wait until the upstream terminates and
        /// return its success value or rethrow any exception it
        /// signaled.
        /// </summary>
        /// <param name="source">The single source to wait for.</param>
        /// <param name="timeoutMillis">The maximum time to wait for termination.</param>
        /// <param name="cts">The means to cancel the wait from outside.</param>
        /// <returns>The success value.</returns>
        /// <exception cref="TimeoutException">If a timeout happens, which also cancels the upstream.</exception>
        /// <remarks>Since 0.0.11</remarks>
        public static T Wait<T>(this ISingleSource<T> source, int timeoutMillis = int.MaxValue, CancellationTokenSource cts = null)
        {
            RequireNonNull(source, nameof(source));

            var parent = new SingleWaitValue<T>();
            source.Subscribe(parent);

            return parent.Wait(timeoutMillis, cts);
        }

        /// <summary>
        /// Subscribes a single observer (subclass) to the single
        /// source and returns this observer instance as well.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <typeparam name="U">The observer type.</typeparam>
        /// <param name="source">The single source to subscribe to.</param>
        /// <param name="observer">The single observer (subclass) to subscribe with.</param>
        /// <returns>The <paramref name="observer"/> provided as parameter.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static U SubscribeWith<T, U>(this ISingleSource<T> source, U observer) where U : ISingleObserver<T>
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(observer, nameof(observer));

            source.Subscribe(observer);
            return observer;
        }

        //-------------------------------------------------
        // Interoperation with other reactive types
        //-------------------------------------------------

        /// <summary>
        /// Maps the upstream observable sequence items into single sources, runs them one
        /// after the other relaying their success item in order and
        /// optionally delays errors from all sources until all of them terminate.
        /// </summary>
        /// <typeparam name="T">The element type of the upstream observable sequence.</typeparam>
        /// <typeparam name="R">The success value type of the inner single sources.</typeparam>
        /// <param name="source">The source of items to map into single sources.</param>
        /// <param name="mapper">The function that receives the upstream item and should
        /// return a single source to relay the success item of.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.13</remarks>
        public static IObservable<R> ConcatMap<T, R>(this IObservable<T> source, Func<T, ISingleSource<R>> mapper, bool delayErrors = false)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleConcatMap<T, R>(source, mapper, delayErrors);
        }

        /// <summary>
        /// Runs some or all single sources, provided by
        /// mapping the source observable sequence into
        /// single sources, at once but emits
        /// their success item in order and optionally delays
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The value type of the source sequence.</typeparam>
        /// <typeparam name="R">The success value type of the inner single sources.</typeparam>
        /// <param name="source">The observable sequence to map into single sources.</param>
        /// <param name="mapper">The function receiving the upstream item and should return
        /// a single source to concatenate eagerly.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<R> ConcatMapEager<T, R>(this IObservable<T> source, Func<T, ISingleSource<R>> mapper, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleConcatMapEager<T, R>(source, mapper, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Maps the values of an observable sequence into single sources,
        /// runs and merges some or all single sources
        /// into one observable sequence and optionally delays all errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The value type of the source observable sequence.</typeparam>
        /// <typeparam name="R">The success value type of the inner single sources.</typeparam>
        /// <param name="source">The observable sequence to map into single sources.</param>
        /// <param name="mapper">The function receiving the upstream item and should return
        /// a single source to concatenate eagerly.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <param name="maxConcurrency">The maximum number of active inner single sources to run at once.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.13</remarks>
        public static IObservable<R> FlatMap<T, R>(this IObservable<T> source, Func<T, ISingleSource<R>> mapper, bool delayErrors = false, int maxConcurrency = int.MaxValue)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));
            RequirePositive(maxConcurrency, nameof(maxConcurrency));

            return new SingleFlatMapMany<T, R>(source, mapper, delayErrors, maxConcurrency);
        }

        /// <summary>
        /// Maps the upstream success item into a maybe source,
        /// subscribes to it and relays its success or failure signals
        /// to the downstream.
        /// </summary>
        /// <typeparam name="T">The upstream value type.</typeparam>
        /// <typeparam name="R">The value type of the inner single source.</typeparam>
        /// <param name="source">The single source to map onto another maybe source.</param>
        /// <param name="mapper">The function receiving the upstream success item
        /// and should return a maybe source to subscribe to.</param>
        /// <returns>The new maybe source instance.</returns>
        /// <remarks>Since 0.0.11<br/>
        /// Note that the result type remains ISingleSource because the
        /// <paramref name="source"/> may be empty and thus the resulting
        /// sequence must be able to represent emptiness.
        /// </remarks>
        public static IMaybeSource<R> FlatMap<T, R>(this ISingleSource<T> source, Func<T, IMaybeSource<R>> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleFlatMapMaybe<T, R>(source, mapper);
        }

        /// <summary>
        /// Maps the success value of the upstream single source
        /// into a completable source and signals its terminal
        /// events to the downstream.
        /// </summary>
        /// <typeparam name="T">The element type of the single source.</typeparam>
        /// <param name="source">The single source to map into a completable source.</param>
        /// <param name="mapper">The function that takes the success value from the upstream
        /// and returns a completable source to subscribe to and relay terminal events of.</param>
        /// <returns>The new completable source instance.</returns>
        /// <remarks>Since 0.0.10</remarks>
        public static ICompletableSource FlatMap<T>(this ISingleSource<T> source, Func<T, ICompletableSource> mapper)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new CompletableFlatMapSingle<T>(source, mapper);
        }

        /// <summary>
        /// Maps the upstream observable sequence into single sources and switches to
        /// the next inner source when it becomes mapped, disposing the previous inner
        /// source if it is still running, optionally delaying
        /// errors until all sources terminate.
        /// </summary>
        /// <typeparam name="T">The element type of the observable sequence.</typeparam>
        /// <typeparam name="R">The success value type of the inner single sources.</typeparam>
        /// <param name="source">The observable sequence to map into single sources.</param>
        /// <param name="mapper">The function receiving the upstream item and should return
        /// a single source to switch to.</param>
        /// <param name="delayErrors">If true, errors are delayed until all sources terminate.</param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.13</remarks>
        public static IObservable<R> SwitchMap<T, R>(this IObservable<T> source, Func<T, ISingleSource<R>> mapper, bool delayErrors = false)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNull(mapper, nameof(mapper));

            return new SingleSwitchMap<T, R>(source, mapper, delayErrors);
        }

        /// <summary>
        /// Ignores the success signal of the single source and
        /// completes the downstream completable observer instead.
        /// </summary>
        /// <typeparam name="T">The success value type of the source.</typeparam>
        /// <param name="source">The source to ignore the success value of.</param>
        /// <returns>The new completable source instance.</returns>
        /// <remarks>Since 0.0.9</remarks>
        public static ICompletableSource IgnoreElement<T>(this ISingleSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new CompletableIgnoreElementSingle<T>(source);
        }

        /// <summary>
        /// Signals the first as the
        /// success value or fails with an IndexOutOfRangeException if the observable
        /// sequence is empty.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The observable sequence to get the first element from.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> FirstOrError<T>(this IObservable<T> source)
        {
            return ElementAtOrError(source, 0L);
        }

        /// <summary>
        /// Signals the only element of the observable sequence,
        /// fails with an IndexOutOfRangeException if the sequence is empty or fails
        /// with an error if the source contains more than one element.
        /// </summary>
        /// <typeparam name="T">The value type of the source observable.</typeparam>
        /// <param name="source">The source observable sequence to get the single element from.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> SingleOrError<T>(this IObservable<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleSingleOrError<T>(source);
        }

        /// <summary>
        /// Signals the last element of the observable sequence
        /// or fails with an IndexOutOfRangeException if the sequence is empty.
        /// </summary>
        /// <typeparam name="T">The value type of the source observable.</typeparam>
        /// <param name="source">The source observable sequence.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> LastOrError<T>(this IObservable<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleLastOrError<T>(source);
        }

        /// <summary>
        /// Signals the element at the specified index as the
        /// success value or fails with an IndexOutOfRangeException if the observable
        /// sequence is shorter than the specified index.
        /// </summary>
        /// <typeparam name="T">The element type of the sequence.</typeparam>
        /// <param name="source">The observable sequence to get an element from.</param>
        /// <param name="index">The index of the element to get (zero based).</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static ISingleSource<T> ElementAtOrError<T>(this IObservable<T> source, long index)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNegative(index, nameof(index));

            return new SingleElementAtOrError<T>(source, index);
        }

        /// <summary>
        /// Subscribe to a single source and expose the terminal
        /// signal as a <see cref="Task"/>.
        /// </summary>
        /// <param name="source">The source single to convert.</param>
        /// <param name="cts">The cancellation token source to watch for external cancellation.</param>
        /// <returns>The new task instance.</returns>
        /// <remarks>Since 0.0.11</remarks>
        public static Task<T> ToTask<T>(this ISingleSource<T> source, CancellationTokenSource cts = null)
        {
            RequireNonNull(source, nameof(source));

            var parent = new SingleToTask<T>();
            parent.Init(cts);
            source.Subscribe(parent);
            return parent.Task;
        }

        /// <summary>
        /// Converts an ongoing or already terminated task to a single source 
        /// and relays its value or error to observers.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="task">The task to observe as a single source.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.11<br/>
        /// Note that the <see cref="Task{TResult}"/> API uses an <see cref="AggregateException"/>
        /// to signal there were one or more errors.
        /// </remarks>
        public static ISingleSource<T> ToSingle<T>(this Task<T> task)
        {
            RequireNonNull(task, nameof(task));

            return new SingleFromTask<T>(task);
        }

        /// <summary>
        /// Converts a single source into a maybe source.
        /// </summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="source">The single source to expose as a maybe source.</param>
        /// <returns>The new maybe source instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IMaybeSource<T> ToMaybe<T>(this ISingleSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleToMaybe<T>(source);
        }

        /// <summary>
        /// Exposes a single source as a legacy observable.
        /// </summary>
        /// <typeparam name="T">The element type of the single and observable sequence.</typeparam>
        /// <param name="source">The single source to expose as an <see cref="IObservable{T}"/></param>
        /// <returns>The new observable instance.</returns>
        /// <remarks>Since 0.0.12</remarks>
        public static IObservable<T> ToObservable<T>(this ISingleSource<T> source)
        {
            RequireNonNull(source, nameof(source));

            return new SingleToObservable<T>(source);
        }

        /// <summary>
        /// Signals the first item of the upstream observable sequence
        /// or signals the default item if the source is empty.
        /// </summary>
        /// <typeparam name="T">The element type of the observable sequence.</typeparam>
        /// <param name="source">The upstream observable sequence.</param>
        /// <param name="defaultItem">The item to signal if the source is empty.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.14</remarks>
        public static ISingleSource<T> FirstOrDefault<T>(this IObservable<T> source, T defaultItem)
        {
            RequireNonNull(source, nameof(source));

            return ElementAtOrDefault(source, 0L, defaultItem);
        }

        /// <summary>
        /// Signals the only item of the upstream observable sequence
        /// or signals the default item if the source is empty,
        /// fails with an IndexOutOfRangeException if the source has
        /// more than one element.
        /// </summary>
        /// <typeparam name="T">The element type of the observable sequence.</typeparam>
        /// <param name="source">The upstream observable sequence.</param>
        /// <param name="defaultItem">The item to signal if the source is empty.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.14</remarks>
        public static ISingleSource<T> SingleOrDefault<T>(this IObservable<T> source, T defaultItem)
        {
            RequireNonNull(source, nameof(source));

            return new SingleSingleOrDefault<T>(source, defaultItem);
        }

        /// <summary>
        /// Signals the last item of the upstream observable sequence
        /// or signals the default item if the source is empty.
        /// </summary>
        /// <typeparam name="T">The element type of the observable sequence.</typeparam>
        /// <param name="source">The upstream observable sequence.</param>
        /// <param name="defaultItem">The item to signal if the source is empty.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.14</remarks>
        public static ISingleSource<T> LastOrDefault<T>(this IObservable<T> source, T defaultItem)
        {
            RequireNonNull(source, nameof(source));

            return new SingleLastOrDefault<T>(source, defaultItem);
        }

        /// <summary>
        /// Signals the item at a specified zero-based index of the upstream 
        /// observable sequence or signals the default item if the source is
        /// shorter.
        /// </summary>
        /// <typeparam name="T">The element type of the observable sequence.</typeparam>
        /// <param name="source">The upstream observable sequence.</param>
        /// <param name="index">The element index.</param>
        /// <param name="defaultItem">The item to signal if the source is sorter than the index.</param>
        /// <returns>The new single source instance.</returns>
        /// <remarks>Since 0.0.14</remarks>
        public static ISingleSource<T> ElementAtOrDefault<T>(this IObservable<T> source, long index, T defaultItem)
        {
            RequireNonNull(source, nameof(source));
            RequireNonNegative(index, nameof(index));

            return new SingleElementAtOrDefault<T>(source, index, defaultItem);

        }

    }
}
