using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Visualization.VisualizationCommon
{
  public static class TaskExtensions
  {
    public static void WithRetry<T>(this Task<T> task, TaskCompletionSource<T> source, Func<Task<T>> taskCreator, CancellationToken token, int retryCount, int delay, Action<TaskCompletionSource<T>, Exception> exceptionHandler = null, Action<TaskCompletionSource<T>> cancellationHandler = null, Action cleanupHandler = null, Predicate<Exception> retryCheck = null)
    {
      task.ContinueWith((Action<Task<T>>) (t =>
      {
        if (cleanupHandler != null)
          cleanupHandler();
        if (t.IsFaulted)
        {
          if (retryCount <= 0 || retryCheck != null && !retryCheck((Exception) task.Exception))
          {
            if (exceptionHandler != null)
              exceptionHandler(source, (Exception) t.Exception);
            else
              source.TrySetException((Exception) t.Exception);
          }
          else
            Task.Delay(delay, token).ContinueWith((Action<Task>) (td =>
            {
              if (td.IsCanceled)
              {
                if (cancellationHandler != null)
                  cancellationHandler(source);
                else
                  source.TrySetCanceled();
              }
              else
                TaskExtensions.WithRetry<T>(taskCreator(), source, taskCreator, token, --retryCount, delay, exceptionHandler, cancellationHandler, cleanupHandler, retryCheck);
            }));
        }
        else if (t.IsCanceled)
        {
          if (cancellationHandler != null)
            cancellationHandler(source);
          else
            source.TrySetCanceled();
        }
        else
        {
          if (!t.IsCompleted)
            return;
          source.TrySetResult(task.Result);
        }
      }));
    }
  }
}
