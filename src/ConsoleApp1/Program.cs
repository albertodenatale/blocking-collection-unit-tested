using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var internalCollection = new ConcurrentVariable();
            internalCollection.TryAdd(1);
            BlockingCollection<int?> collection =
                new BlockingCollection<int?>(internalCollection, 1);

            var reader = Task.Run(() =>
            {
                foreach (int? item in collection.GetConsumingEnumerable())
                {
                    if (item.HasValue)
                    {
                        Console.WriteLine(string.Format("Read {0}", item));
                    }
                }
            });

            Task.Run(() =>
            {
                while (!collection.IsAddingCompleted)
                {
                    string input = Console.ReadLine();

                    if(string.IsNullOrEmpty(input))
                    {
                        collection.CompleteAdding();
                    }
                    else
                    {
                        int i = 0;

                        if(int.TryParse(input, out i))
                        {
                            Console.WriteLine(string.Concat("Adding", i));
                            collection.Add(i);
                            Console.WriteLine(string.Concat("Added", i));
                        }
                    }
                }
            }).Wait();
        }
    }
}
