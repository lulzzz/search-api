using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchV2.Generics
{
    public class AsyncResult<T> : ISearchResult<T>, IAsyncOperation where T : class
    {
        volatile Task _runningTask;
        readonly List<T> loaded = new List<T>();
        readonly object _syncObj = new object();

        readonly TaskCompletionSource<int> _tsc = new TaskCompletionSource<int>(TaskCreationOptions.LongRunning);

        public delegate void UpdateStateDelegate(Task newTask, IEnumerable<T> loadedBatch);

        void UpdateState(Task runningTask, IEnumerable<T> newItems)
        {
            lock (_syncObj)
            {
                _runningTask = runningTask;
                loaded.AddRange(newItems);
                if (runningTask == null)
                {
                    _tsc.SetResult(loaded.Count);
                }
            }
        }

        public AsyncResult(Func<UpdateStateDelegate, Task> loader)
        {
            _runningTask = loader(UpdateState);
        }

        #region ISearchResult<MadfastResultItem>.ForEachAsync
        async Task ISearchResult<T>.ForEachAsync(Func<T, Task<bool>> body)
        {
            if (_runningTask == null)
            {
                foreach (var item in ReadyResult)
                {
                    var @continue = await body(item);
                    if (!@continue)
                    {
                        return;
                    }
                }
            }
            else
            {
                foreach (var itemTask in AsyncResultInternal)
                {
                    var item = await itemTask;
                    var @continue = item != null && await body(item);
                    if (!@continue)
                    {
                        return;
                    }
                }
            }
        }

        IEnumerable<T> ReadyResult
        {
            get
            {
                lock (_syncObj)
                {
                    if (_runningTask == null)
                    {
                        return loaded;
                    }
                }
                throw new InvalidOperationException("The result is not ready for synchronous consumption");
            }
        }

        Task<T> TryNext(int index, Task runningTask)
            => runningTask.ContinueWith(t =>
            {
                lock (_syncObj)
                {
                    if (_runningTask != null && _runningTask.IsFaulted)
                    {
                        throw new InvalidOperationException("Loading of results was faulted");
                    }
                    return index < loaded.Count
                        ? loaded[index]
                        : null;
                }
            });

        IEnumerable<Task<T>> AsyncResultInternal
        {
            get
            {
                int i = 0; // count of returned elements-1
                T[] ready;

                do
                {
                    lock (_syncObj)
                    {
                        var len = loaded.Count - i;
                        ready = new T[len];
                        loaded.CopyTo(i, ready, 0, len);
                    }

                    foreach (var item in ready)
                    {
                        i++;
                        yield return Task.FromResult(item);
                    }

                    Task running = null;

                    lock (_syncObj) // synchronizedCheck to determine if anything new has been loaded
                    {
                        if (i == loaded.Count) // if nothing new was loaded, then remember current running task to avoid race
                        {
                            running = _runningTask;
                        }
                    }

                    if (running != null) // if running task was remembered in the lock, then no ready elements left and we yield awaiting task
                    {
                        yield return TryNext(i, running);
                        i++;
                        break; // 
                    }
                } while (true);

                do
                {
                    Task running;
                    lock (_syncObj)
                    {
                        if (_runningTask == null)
                        {
                            break;
                        }
                        running = _runningTask;
                    }
                    yield return TryNext(i, running);
                    i++;
                } while (true);

                while (i < loaded.Count)
                {
                    yield return Task.FromResult(loaded[i]);
                    i++;
                }
            }
        }

        AsyncOperationStatus IAsyncOperation.Status
        {
            get
            {
                if (_runningTask == null)
                {
                    return AsyncOperationStatus.Finished;
                }
                if (_runningTask.IsFaulted)
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
