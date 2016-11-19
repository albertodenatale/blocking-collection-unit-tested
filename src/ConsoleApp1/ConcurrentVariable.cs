using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp1
{
    public class ConcurrentVariable : IProducerConsumerCollection<int?>
    {
        public int? Variable { get; set; }
        public object Sync { get; set; }

        public int Count
        {
            get
            {
                return Variable.HasValue ? 1 : 0;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public void CopyTo(Array array, int index)
        {
            Console.WriteLine("CopyTo Array called");
            array.SetValue(Variable, index);
        }

        public void CopyTo(int?[] array, int index)
        {
            Console.WriteLine("CopyTo int?[] called");
            array[index] = Variable.Value;
        }

        public IEnumerator<int?> GetEnumerator()
        {
            Console.WriteLine("GetEnumerator called");
            yield return Variable;
        }

        public int?[] ToArray()
        {
            Console.WriteLine("ToArray int?[] called");
            return new int?[] { Variable };
        }

        public bool TryAdd(int? item)
        {
            Console.WriteLine("TryAdd called");
            Thread.Sleep(1999);
            if (Variable.HasValue)
            {
                return false;
            }

            Variable = item;

            return true;
        }

        public bool TryTake(out int? item)
        {
            Console.WriteLine("TryTake called");
            if (!Variable.HasValue)
            {
                item = null;

                return false;
            }

            item = Variable;

            Variable = null;

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Console.WriteLine("GetEnumerator called");
            yield return Variable;
        }
    }
}
