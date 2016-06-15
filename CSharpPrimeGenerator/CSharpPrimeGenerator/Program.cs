using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace CSharpPrimeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create a cancellation token for stopping the prime number generation task.
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;


            //var primeGenerator = new SimplePrimeGenerator();
            var primeGenerator = new OptimizedGenerator();

            //List<T> has a max capacity at uint.MaxValue/2.  Pass it as the limit to be calculated if there is no time limit.
            var limit = uint.MaxValue / 2;
            var task = Task.Factory.StartNew(() => primeGenerator.GeneratePrime(limit, cancellationToken), cancellationToken);

            //Get the process/program start time.
            //The start time can be a significant delay if this is a cold start (run for the first time).
            //The JIL creates the native code on the first run.
            //Run this test twice to get a better results, or use NGen to precompile the .NET assemblies to native code.
            //Use RyuJIT instead of JIT.  RyuJIT is faster than JIT. https://gist.github.com/richlander/d29dc4fe4e8032de5c92

            //This loop will output a counter number and a max prime number for every second.
            //Do not output too many statements as these can affect the performance.
            var startTime = DateTime.Now; //Process.GetCurrentProcess().StartTime;
            TimeSpan duration = DateTime.Now - startTime;
            var numberOfSecondsRan = Math.Round(duration.TotalMilliseconds / 1000d);
            while (numberOfSecondsRan < 60d) //exit the loop if the program has ran for more than 60 seconds.
            {
                var nextStop = numberOfSecondsRan + 1d;
                int waitTime = (int)((nextStop * 1000d) - duration.TotalMilliseconds);
                Thread.Sleep(waitTime);
                System.Console.WriteLine($"Time(sec): {((int)nextStop).ToString("D2")} ----- Max Prime #: {primeGenerator.GetMaxPrime().ToString("N0")}");
                duration = DateTime.Now - startTime;
                numberOfSecondsRan = Math.Round(duration.TotalMilliseconds / 1000d);
            }

            //Stop generating the prime numbers
            tokenSource.Cancel();
            try
            {

                Task.WhenAll(task);  //when the task is completed, its continuation is below.

                if (task != null)
                {
                    var primeList = task.Result;
                    //Please note, some additional prime numbers are calculated after 60 seconds before the task was cancelled.                    
                    System.Console.WriteLine($"Number of primes: {primeList.Count().ToString("N0")}");
                    System.Console.WriteLine($"Last prime found: {primeGenerator.GetMaxPrime().ToString("N0")}");
                    System.Console.WriteLine($"Elapsed time (milliseconds): {primeGenerator.ElapsedTime.ToString("N0")}");
                    System.Console.WriteLine($"GC Count: {primeGenerator.GCCount.ToString("N0")}");
                }
                System.Console.WriteLine("Hit a key to exit.");
                System.Console.ReadKey(true);

            }
            catch (AggregateException e)
            {
                Console.WriteLine("\nAggregateException thrown with the following inner exceptions:");
                // Display information about each exception. 
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        Console.WriteLine("   TaskCanceledException: Task {0}",
                                          ((TaskCanceledException)v).Task.Id);
                    else
                        Console.WriteLine("   Exception: {0}", v.GetType().Name);
                }
                Console.WriteLine();
            }
            finally
            {
                tokenSource.Dispose();
            }
        }
    }
}
