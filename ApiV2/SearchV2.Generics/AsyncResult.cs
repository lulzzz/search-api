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

        readonly TaskCompletionSource<int> _countTcs = new TaskCompletionSource<int>();

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

        Task UpdateState(Task newRunningTask, IEnumerable<T> newItems)
            => RunInMutex((loaded, runningTask) =>
            {
                loaded.AddRange(newItems);
                Thread.MemoryBarrier(); // ensure items are added before setting _runningTaskUnsafe
                _runningTaskUnsafe = newRunningTask; // the only place where _runningTaskUnsafe is set
                if (newRunningTask == null)
                {
                    _countTcs.SetResult(loaded.Count);
                }
            });

        public AsyncResult(Func<UpdateStateDelegate, Task> loader)
        {
            _runningTaskUnsafe = loader(UpdateState);
        }

        #region ISearchResult<MadfastResultItem>.ForEachAsync
        async Task ISearchResult<T>.ForEachAsync(Func<T, Task<bool>> body)
        {
            if (_runningTaskUnsafe == null) // this means we can query loaded items directly, no inserts are running at the current time
            {
                foreach (var item in _loadedUnsafe)
                {
                    await body(item);
                }
            }
            else
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

                    var runningTaskLocal = _runningTaskUnsafe; // cache this value locally to avoid race between null check and await
                    if (runningTaskLocal == null)
                    {
                        break;
                    }
                    await runningTaskLocal;
                }

                counter = await ProcessLoadedItems(counter);
            }
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

        Task<int> ISearchResult<T>.Count => _countTcs.Task;
        #endregion
    }
}
