using SearchV2.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SearchV2.Generics.Tests
{
    public class AsyncResultTests
    {
        [Fact]
        public async Task Count_CorrectStatusAndCountIfLoaded()
        {
            var s1 = new SemaphoreSlim(0, 1);
            var s3 = new SemaphoreSlim(0, 1);
            var s2 = new SemaphoreSlim(0, 1);
            var s4 = new SemaphoreSlim(0, 1);

            ISearchResult<string> o = new AsyncResult<string>(async updateState => {
                await s1.WaitAsync();
                var range = Enumerable.Range(0, 5).Select(i => i.ToString());

                var addTask = Task.Run(async () => {
                    await s3.WaitAsync();
                    await updateState(null, range);
                    s4.Release();
                });

                await updateState(addTask, range);
                s2.Release();
            });

            Assert.Equal(TaskStatus.WaitingForActivation, o.Count.Status);
            s1.Release();
            await s2.WaitAsync();
            Assert.Equal(TaskStatus.WaitingForActivation, o.Count.Status);
            s3.Release();
            await s4.WaitAsync();
            Assert.Equal(10, o.Count.Result);
        }

        [Fact]
        public async Task Results_CorrectAmountAreAvailableAndReturned()
        {
            var s1 = new SemaphoreSlim(0, 1);
            var s3 = new SemaphoreSlim(0, 1);
            var s2 = new SemaphoreSlim(0, 1);
            var s4 = new SemaphoreSlim(0, 1);

            ISearchResult<string> o = new AsyncResult<string>(async updateState => {
                await s1.WaitAsync();

                var addTask = Task.Run(async () => {
                    await s3.WaitAsync();
                    await updateState(null, Enumerable.Range(0, 5).Select(i => i.ToString()));
                });

                await updateState(addTask, Enumerable.Range(5, 5).Select(i => i.ToString()));
            });

            var iterationsCount = 0;

            var forEachTask = o.ForEachAsync(_ =>
            {
                iterationsCount++;
                if (iterationsCount == 5)
                {
                    s2.Release();
                };
                return Task.FromResult(true);
            });
            Assert.Equal(0, iterationsCount);
            Assert.Equal(TaskStatus.WaitingForActivation, forEachTask.Status);
            s1.Release();
            await s2.WaitAsync();
            Assert.Equal(5, iterationsCount);
            Assert.Equal(TaskStatus.WaitingForActivation, forEachTask.Status);
            s3.Release();
            await forEachTask;
            Assert.Equal(10, iterationsCount);

            iterationsCount = 0;

            await o.ForEachAsync(_ =>
            {
                iterationsCount++;
                return Task.FromResult(true);
            });
            Assert.Equal(10, iterationsCount);
        }
    }
}
