using System;
using System.Threading;

namespace ThreadMinFinder
{
    class Program
    {
        private static readonly int dim = 100000000;
        private static readonly int threadNum = 6;
        private readonly int[] arr = new int[dim];
        private readonly Thread[] threads = new Thread[threadNum];

        private int globalMin = int.MaxValue;
        private int globalMinIndex = -1;
        private int completedThreads = 0;

        private readonly object locker = new object();

        static void Main(string[] args)
        {
            Program p = new Program();
            p.GenerateArray();

            p.Parallel();

            Console.WriteLine($"Мінімальне значення: {p.globalMin}");
            Console.WriteLine($"Індекс мінімального значення: {p.globalMinIndex}");
            Console.ReadKey();
        }

        private void GenerateArray()
        {
            for (int i = 0; i < dim; i++)
            {
                arr[i] = i;
            }

            Random rand = new Random();
            int randomIndex = rand.Next(0, dim);
            arr[randomIndex] = -9999;
            Console.WriteLine(randomIndex + " --- value: " + arr[randomIndex]);
        }

        private void Parallel()
        {
            int arrPiece = dim / threadNum;

            for (int i = 0; i < threadNum; i++)
            {
                int start = i * arrPiece;
                int end = start + arrPiece;
                if(i == threadNum-1)
                {
                    //Console.WriteLine("i = " + i);
                    end = dim;
                }

                threads[i] = new Thread(() => FindMinInRange(start, end));
                threads[i].Start();
            }

            lock (locker)
            {
                while (completedThreads < threadNum)
                {
                    Monitor.Wait(locker);
                }
            }
        }

        private void FindMinInRange(int start, int end)
        {
            int localMin = int.MaxValue;
            int localMinIndex = -1;

            for (int i = start; i < end; i++)
            {
                if (arr[i] < localMin)
                {
                    localMin = arr[i];
                    localMinIndex = i;
                }
            }

            lock (locker)
            {
                if (localMin < globalMin)
                {
                    globalMin = localMin;
                    globalMinIndex = localMinIndex;
                }

                completedThreads++;
                Monitor.Pulse(locker);
            }
        }
    }
}
