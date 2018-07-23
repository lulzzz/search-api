using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public class AsyncResult<T> : ISearchResult<T>, IAsyncOperation where T : class
    {
        volatile Task _runningTaskUnsafe;
        readonly List<T> _loadedUnsafe = new List<T>();

        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        readonly TaskCompletionSource<int> _tsc = new TaskCompletionSource<int>();

        public delegate Task UpdateStateDelegate(Task newTask, IEnumerable<T> loadedBatch);

        async Task RunInMutex(Action<List<T>, Task> a)
        {
            await _semaphore.WaitAsync();
            try
            {
                a(_loadedUnsafe, _runningTaskUnsafe);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        async Task RunInMutex(Func<List<T>, Task, Task> a)
        {
            await _semaphore.WaitAsync();
            try
            {
                await a(_loadedUnsafe, _runningTaskUnsafe).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        Task UpdateState(Task newRunningTask, IEnumerable<T> newItems)
        => RunInMutex((loaded, runningTask) =>
            {
                _runningTaskUnsafe = newRunningTask; // the only place where _runningTaskUnsafe is set
                loaded.AddRange(newItems);
                if (newRunningTask == null)
                {
                    _tsc.SetResult(loaded.Count);
                }
            });

        public AsyncResult(Func<UpdateStateDelegate, Task> loader)
        {
            _runningTaskUnsafe = loader(UpdateState);
        }

        #region ISearchResult<MadfastResultItem>.ForEachAsync
        async Task ISearchResult<T>.ForEachAsync(Func<T, Task<bool>> body)
        {
            int counter = 0;

            async Task<int> ProcessLoadedItems(int startFrom)
            {
                T[] buffer = null;
                await RunInMutex((loaded, runningTask) =>
                {
                    buffer = new T[loaded.Count - startFrom];
                    loaded.CopyTo(startFrom, buffer, 0, buffer.Length);
                });
                foreach (var item in buffer)
                {
                    await body(item);
                    startFrom++;
                }
                return startFrom;
            }

            while (true)
            {
                counter = await ProcessLoadedItems(counter);

                Task runningTaskLocal = null;
                await RunInMutex((loaded, runningTask) => { runningTaskLocal = runningTask; });
                if (runningTaskLocal == null)
                {
                    break;
                }
                await runningTaskLocal;
            }

            counter = await ProcessLoadedItems(counter);
        }

        AsyncOperationStatus IAsyncOperation.Status
        {
            get
            {
                if (_runningTaskUnsafe == null)
                {
                    return AsyncOperationStatus.Finished;
                }
                if (_runningTaskUnsafe.IsFaulted)
                {
                    return AsyncOperationStatus.Faulted;
                }
                return AsyncOperationStatus.Running;
            }
        }

        Task<int> ISearchResult<T>.Count => _tsc.Task;
        #endregion
    }
}
