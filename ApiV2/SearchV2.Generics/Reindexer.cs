using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SearchV2.Generics
{
    public sealed class Reindexer
    {
        volatile CancellationTokenSource _cancellationTokenSource;
        volatile ActionBlock<State> _actionBlock;

        State _state;
        readonly SemaphoreSlim _stateSemaphore = new SemaphoreSlim(1);

        public event EventHandler<SuccessfulUpdateEventArgs> OnSuccessfulUpdate;
        public event EventHandler<FailedUpdateEventArgs> OnFailedUpdate;

        readonly Parameters _parameters;
        readonly ISearchIndex _searchIndex;
        readonly DataSourceDelegate _dataSourceDelegate;
        
        /// <summary>
        /// eliminates null check and try-catch clause
        /// </summary>
        void InvokeSafe<T>(EventHandler<T> handler, T eventArgs)
        {
            try
            {
                handler?.Invoke(this, eventArgs);
            }
            catch
            {

            }
        }

        async Task<bool> WithoutExeption(Func<Task> func)
        {
            try
            {
                await func();
                return true;
            }
            catch (Exception e)
            {
                InvokeSafe(OnFailedUpdate, new FailedUpdateEventArgs { Exception = e });
                return false;
            }
        }

        ActionBlock<State> CreateActionBlock(CancellationToken cancellationToken)
        {
            ActionBlock<State> block = null;

            async Task Step(State @in)
            {
                var (since, skip) = (@in.Since, @in.Skip);
                await Task.Delay(skip == 0 ? _parameters.WaitBetweenBatches : _parameters.WaitBetweenChecksForNewItems);

                ISearchIndexItemWithTime[] newItems = null;
                var state = _state;
                if (await WithoutExeption(async () => newItems = (await _dataSourceDelegate(since, skip, _parameters.BatchSize)).ToArray()))
                {
                    if (newItems.Length > 0)
                    {
                        var maxLastUpdated = newItems.Max(i => i.LastUpdated);
                        var countOfItemsWithMaxLastUpdated = newItems.Count(i => i.LastUpdated == maxLastUpdated);

                        try
                        {
                            await _stateSemaphore.WaitAsync();
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }
                            if (await WithoutExeption(() => _searchIndex.Add(newItems)))
                            {
                                InvokeSafe(OnSuccessfulUpdate, new SuccessfulUpdateEventArgs
                                {
                                    NewestRecordDateTime = since,
                                    ProcessedItemsCountForCurrentDate = countOfItemsWithMaxLastUpdated
                                });

                                state = _state = new State { Since = maxLastUpdated, Skip = maxLastUpdated == since ? skip + countOfItemsWithMaxLastUpdated : countOfItemsWithMaxLastUpdated };
                            }
                        }
                        finally
                        {
                            _stateSemaphore.Release();
                        }
                    }
                }
                if (!cancellationToken.IsCancellationRequested)
                {
                    _actionBlock.Post(state);
                }
            }

            block = new ActionBlock<State>(Step);
            return block;
        }

        public Reindexer(ISearchIndex index, DataSourceDelegate dataSourceDelegate, Parameters parameters = null)
        {
            _searchIndex = index;
            _dataSourceDelegate = dataSourceDelegate;
            _parameters = parameters;
        }

        public async Task Start(DateTime startDateTime, int skip)
        {
            await _stateSemaphore.WaitAsync();

            if (_cancellationTokenSource != null)
            {
                _stateSemaphore.Release();
                throw new InvalidOperationException("Reindexer is already started");
            }

            _state = new State { Since = startDateTime, Skip = skip };
            _cancellationTokenSource = new CancellationTokenSource();
            _actionBlock = CreateActionBlock(_cancellationTokenSource.Token);
            _actionBlock.Post(_state);

            _stateSemaphore.Release();
        }

        public async Task<State> StopAndGetFinalState()
        {
            await _stateSemaphore.WaitAsync();

            using (_cancellationTokenSource)
            {
                _cancellationTokenSource.Cancel();
            }

            _cancellationTokenSource = null;
            _actionBlock = null;

            var state = _state;
            _stateSemaphore.Release();

            return state;
        }

        #region declared types
        public struct State
        {
            public DateTime Since;
            public int Skip;
        }

        public delegate Task<IEnumerable<ISearchIndexItemWithTime>> DataSourceDelegate(DateTime since, int skip = 0, int? maxCount = null);

        public class Parameters
        {
            public TimeSpan WaitBetweenBatches { get; set; } = TimeSpan.FromSeconds(10);
            public TimeSpan WaitBetweenChecksForNewItems { get; set; } = TimeSpan.FromMinutes(5);
            public int BatchSize { get; set; } = 10000;

            public static Parameters Default { get; } = new Parameters();
        }

        public class SuccessfulUpdateEventArgs : EventArgs
        {
            public DateTime NewestRecordDateTime { get; set; }
            public int ProcessedItemsCountForCurrentDate { get; set; }
        }

        public class FailedUpdateEventArgs : EventArgs
        {
            public Exception Exception { get; set; }
        }
        #endregion
    }
}
