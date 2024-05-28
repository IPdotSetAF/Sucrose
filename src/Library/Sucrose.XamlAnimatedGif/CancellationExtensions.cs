﻿namespace Sucrose.XamlAnimatedGif
{
    internal static class CancellationExtensions
    {
        public static async Task WithCancellationToken(this Task task, CancellationToken cancellationToken)
        {
            await await Task.WhenAny(task, cancellationToken.WhenCanceled());
        }

        public static async Task<T> WithCancellationToken<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            Task firstTaskToFinish = await Task.WhenAny(task, cancellationToken.WhenCanceled());
            if (firstTaskToFinish == task)
            {
                return await task;
            }

            await firstTaskToFinish;

            // Will never be reached because the previous statement will throw, but necessary to satisfy the compiler
            throw new OperationCanceledException(cancellationToken);
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            TaskCompletionSource<int> tcs = new();
            CancellationTokenRegistration registration = default;
            registration = cancellationToken.Register(o =>
            {
                ((TaskCompletionSource<int>)o).TrySetCanceled();
                // ReSharper disable once AccessToModifiedClosure
                registration.Dispose();
            }, tcs);
            return tcs.Task;
        }
    }
}