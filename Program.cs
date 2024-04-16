using System.Diagnostics;

namespace snus_zadatak3;

internal class Program
{
    private static async Task Main()
    {
        const int arraySize = 50000000;
        const int taskCount = 5;
        const int minValue = 0;
        const int maxValue = 1000;

        var array = new int[arraySize];
        var random = new Random();
        for (var i = 0; i < arraySize; i++) array[i] = random.Next(minValue, maxValue);

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var sumSync = CalculateSum(array);
        stopwatch.Stop();
        Console.WriteLine($"Sync time: {stopwatch.ElapsedMilliseconds} ms. \t Sum: {sumSync}");

        stopwatch.Restart();
        var sumAsync = await CalculateSumAsync(array, taskCount);
        stopwatch.Stop();
        Console.WriteLine($"Async time: {stopwatch.ElapsedMilliseconds} ms. \t Sum: {sumAsync}");
    }

    private static long CalculateSum(IEnumerable<int> array)
    {
        return array.Aggregate<int, long>(0, (current, num) => current + num);
    }

    private static async Task<long> CalculateSumAsync(IReadOnlyList<int> array, int taskCount)
    {
        var tasks = new Task<long>[taskCount];
        var batchSize = array.Count / taskCount;

        for (var i = 0; i < taskCount; i++)
        {
            var start = i * batchSize;
            var end = i == taskCount - 1 ? array.Count : (i + 1) * batchSize;
            tasks[i] = Task.Run(() => PartialSum(array, start, end));
        }

        long totalSum = 0;
        while (tasks.Length > 0)
        {
            var task = await Task.WhenAny(tasks);
            totalSum += await task;
            tasks = tasks.Where(t => t != task).ToArray();
        }

        return totalSum;
    }

    private static long PartialSum(IReadOnlyList<int> array, int start, int end)
    {
        long sum = 0;
        for (var i = start; i < end; i++) sum += array[i];
        return sum;
    }
}