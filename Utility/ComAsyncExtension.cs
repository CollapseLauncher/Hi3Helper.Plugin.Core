using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// An extension contains tools to marshal plugin's <see cref="Task"/> to <see cref="ComAsyncResult"/> and vice versa.
/// </summary>
public static partial class ComAsyncExtension
{
    /// <summary>
    /// A delegate which used to pass an <see cref="Exception"/> to a callback.
    /// </summary>
    /// <param name="ex">An exception to pass into callback.</param>
    private delegate void SetExceptionDelegate(Exception ex);

    /// <summary>
    /// Marshal a <see cref="Task"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="Task"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult(this Task task)
        => ComAsyncResult.Alloc(task);

    /// <summary>
    /// Marshal a <see cref="Task{T}"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="Task{T}"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult<T>(this Task<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(task);

    /// <summary>
    /// Marshal a <see cref="ValueTask"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="ValueTask"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult(this ValueTask task)
        => ComAsyncResult.Alloc(task);

    /// <summary>
    /// Marshal a <see cref="ValueTask{T}"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="ValueTask{T}"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult<T>(this ValueTask<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(task);

    /// <param name="resultP">A pointer of the <see cref="ComAsyncResult"/> struct.</param>
    extension(nint resultP)
    {
        /// <summary>
        /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="Task"/>.
        /// </summary>
        /// <returns>An awaitable managed <see cref="Task"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task AsTask()
            => new TaskRegisteredWaitHandleContext(resultP.EnsureTypeIsTask()).Task;

        /// <summary>
        /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="Task{T}"/>.
        /// </summary>
        /// <returns>An awaitable managed <see cref="Task{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> AsTask<T>()
            where T : unmanaged => (Task<T>)new TaskRegisteredWaitHandleContext<T>(resultP.EnsureTypeIsTask()).Task;

        /// <summary>
        /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="ValueTask"/>.
        /// </summary>
        /// <returns>An awaitable managed <see cref="ValueTask"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask AsValueTask()
            => ValueTaskRegisteredWaitHandleContext<nint>.CreateValueTaskNoReturn(resultP.EnsureTypeIsValueTask());

        /// <summary>
        /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="ValueTask{T}"/>.
        /// </summary>
        /// <returns>An awaitable managed <see cref="ValueTask{T}"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask<T> AsValueTask<T>()
            => ValueTaskRegisteredWaitHandleContext<T>.CreateValueTask(resultP.EnsureTypeIsValueTask());

        /// <summary>
        /// Perform a check whether the <see cref="ComAsyncResult"/> is correctly assigned for a <see cref="Task"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe nint EnsureTypeIsTask()
            => ((ComAsyncResult*)resultP)->IsValueTask
                ? throw new InvalidOperationException("This callee uses ValueTask or ValueTask<T>. Please use AsValueTask() instead!")
                : resultP;

        /// <summary>
        /// Perform a check whether the <see cref="ComAsyncResult"/> is correctly assigned for a <see cref="ValueTask"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe nint EnsureTypeIsValueTask()
            => !((ComAsyncResult*)resultP)->IsValueTask
                ? throw new InvalidOperationException("This callee uses Task or Task<T>. Please use AsTask() instead!")
                : resultP;
    }
}
