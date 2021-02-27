using Microsoft.VisualStudio.TestTools.UnitTesting;
using Radial.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace Radial.Tests
{
    [TestClass]
    public class ConcurrentListTests
    {
        [TestMethod]
        public async Task ConcurrentListTest()
        {
            var list = new ConcurrentList<int>();
            for (var i = 0; i < 500_000; i++)
            {
                list.Add(i);
            }

            var cts = new CancellationTokenSource();
            var token = cts.Token;

            _ = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    foreach (var item in list)
                    {
                        _ = list.IndexOf(item);
                    }
                }
            });

            _ = Task.Run(() =>
            {
                for (var i = 0; i < 5; i++)
                {
                    list.Remove(500 + i);
                    Thread.Sleep(100);
                    list.RemoveAt(100_000);
                    Thread.Sleep(100);
                    list.Add(42);
                    Thread.Sleep(100);
                    list.Insert(200_000, 100);
                    Thread.Sleep(100);
                }
            });

            await Task.Delay(3000);
            cts.Cancel();

            Assert.IsFalse(list.Contains(500));
        }
    }
}
