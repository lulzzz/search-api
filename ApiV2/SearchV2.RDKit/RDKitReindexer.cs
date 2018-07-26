using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SearchV2.RDKit
{
    public sealed class RDKitReindexer
    {
        public interface ISearchIndexItemWithTime : ISearchIndexItem
        {
            DateTime LastUpdated { get; }
        }

        public delegate Task<IEnumerable<ISearchIndexItemWithTime>> DataSourceDelegate(DateTime since, int? maxCount = null);

        private RDKitReindexer(ISearchIndex index, DateTime startDateTime, DataSourceDelegate dataSourceDelegate)
        {
            var cancellationToken = CancellationToken = new CancellationToken(false);

            async Task CheckForNewAndAddForever(DateTime since)
            {
                while (cancellationToken.IsCancellationRequested)
                {
                    while (true)
                    {
                        var newItems = (await dataSourceDelegate(since)).ToArray();
                        if (newItems.Length == 0)
                        {
                            break;
                        }
                        since = newItems.Max(i => i.LastUpdated);
                        await index.Add(newItems);
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                    OnSuccessfulUpdate?.Invoke(this, new SuccessfulUpdateEventArgs { NewestRecordDateTime = since });
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }

            Task.Run(() => CheckForNewAndAddForever(startDateTime));
        }

        public CancellationToken CancellationToken { get; }

        public event EventHandler<SuccessfulUpdateEventArgs> OnSuccessfulUpdate;

        public class SuccessfulUpdateEventArgs : EventArgs
        {
            public DateTime NewestRecordDateTime { get; set; }
        }
    }
}
