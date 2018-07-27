using SearchV2.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static SearchV2.Generics.Reindexer;

namespace SearchV2.Generics.Tests
{
    public class ReindexerTests
    {
        class ListIndex : ISearchIndex
        {
            public List<ISearchIndexItem> Items { get; } = new List<ISearchIndexItem>();

            int counter = 0;

            Task ISearchIndex.Add(IEnumerable<ISearchIndexItem> items)
            {
                if (++counter % 3 == 0)
                {
                    throw new ApplicationException("fake exception");
                }
                Items.AddRange(items);
                return Task.CompletedTask;
            }

            Task ISearchIndex.Remove(IEnumerable<string> refs)
            {
                throw new NotImplementedException();
            }
        }

        class SearchIndexItemWithTime : ISearchIndexItemWithTime
        {
            public DateTime LastUpdated { get; set; }
            public string Smiles { get; set; }
            public string Ref { get; set; }
        }

        static IEnumerable<ISearchIndexItemWithTime> MakeItems(DateTime startDate, int count)
        {
            return Enumerable.Range(0, count)
                .Select(i => new SearchIndexItemWithTime
                {
                    Ref = i.ToString(),
                    LastUpdated = startDate.Subtract(TimeSpan.FromSeconds(count / (i % 4 + 1)))
                }).ToArray();
        }

        [Fact]
        public async Task Reindexer()
        {
            var parameters = new Reindexer.Parameters {
                BatchSize = 10,
                WaitBetweenBatches = TimeSpan.FromMilliseconds(0),
                WaitBetweenChecksForNewItems = TimeSpan.FromMilliseconds(0)
            };

            var itemsSemaphore = new SemaphoreSlim(1);
            var items = new List<ISearchIndexItemWithTime>();
            var index = new ListIndex();

            var reindexer = new Reindexer(
                index,
                async (since, skip, maxCount) =>
                {
                    try
                    {
                        await itemsSemaphore.WaitAsync();
                        return items
                            .OrderBy(item => item.LastUpdated)
                            .Where(item => item.LastUpdated > since)
                            .Skip(skip)
                            .Take(maxCount.Value)
                            .ToArray();
                    }
                    finally
                    {
                        itemsSemaphore.Release();
                    }
                },
                parameters);

            var tscs = Enumerable.Range(0, 4).Select(_ => new TaskCompletionSource<bool>()).ToArray();
            var c = 0;

            reindexer.OnSuccessfulUpdate += (s, a) =>
            {
                tscs[c].SetResult(true);
                c++;
            };

            await itemsSemaphore.WaitAsync();
            await reindexer.Start(new DateTime(), 0);

            items.AddRange(MakeItems(DateTime.Now - TimeSpan.FromDays(1), 40));
            itemsSemaphore.Release();
            await tscs[0].Task;

            var indexItems = index.Items.OrderBy(i => i.Ref, StringComparer.Ordinal).ToArray();

            Assert.Equal(items.OrderBy(i => i.LastUpdated).Take(indexItems.Length).OrderBy(i => i.Ref, StringComparer.Ordinal), indexItems);

            await itemsSemaphore.WaitAsync();
            
        }
    }
}
