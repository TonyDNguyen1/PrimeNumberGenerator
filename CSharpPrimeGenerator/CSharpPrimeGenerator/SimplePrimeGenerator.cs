using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSharpPrimeGenerator
{
    public class SimplePrimeGenerator
    {
        //Use the running flag to prevent GeneratePrimeAsync from running concurrently.
        private bool running = false;

        //This has the max prime number generated from GeneratePrime.
        //A lock will not be used on assigning it.  Assigning a uint value is an atomic action.
        //A lock would enforce the order of the read and write but this is not important.  Performance is more important.
        private uint maxPrime = 2;

        public uint GetMaxPrime()
        {
            return maxPrime;
        }

        //Use List<uint> instead of uint[] for the collection because List<unit> doubles its capacity automatically.
        //There is an overhead on copying the old list into the new list when the capacity is increased.
        private List<uint> primeList = null;

        public long ElapsedTime { get; set; }
        public int GCCount { get; set; }

        public SimplePrimeGenerator()
        {
            primeList = new List<uint>();
        }

        public List<uint> GeneratePrime(uint max, CancellationToken ct)
        {
            lock (this)
            {
                if (running)
                {
                    //Exit if another task is running
                    return primeList;
                }
                else
                {
                    //Set running to true so that another concurrent process is prevented from running.
                    running = true;
                }
            }


            if (ct.IsCancellationRequested == true)
            {
                return primeList;
            }

            //Exit early if the max is less than 5.
            if (max < 2)
            {
                return primeList;
            }
            if (max == 2)
            {
                primeList.Add(2);
                return primeList;
            }
            if (max < 5)
            {
                primeList.Add(2);
                primeList.Add(3);
                return primeList;
            }

            //"2" is not included in the initial prime list because the event numbers are skipped.            
            primeList.Add(3);
            primeList.Add(5);

            //n is a prime if it is not divisible by every prime number less than or equal to the square root of n.            
            maxPrime = 5;
            uint sqrtPrime;
            int gen0 = GC.CollectionCount(0);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (uint n = 7; n <= max; n += 2)  //Skip even numbers.
            {
                sqrtPrime = (uint)Math.Sqrt((double)n);

                foreach (uint prime in primeList)
                {
                    if (ct.IsCancellationRequested)
                    {
                        //exit both the outer and inner loops
                        n = max + n;
                        break;
                    }

                    //test divisibility for only the prime numbers that are less than or equal-to the square root of n
                    if (prime <= sqrtPrime)
                    {
                        //n is divisible by a prime if n modulo prime is 0 (n/prime and its remainder is 0)
                        if (n % prime == 0)
                        {
                            //n is not a prime number; exit the inner loop and test for the next (n+2).
                            break;
                        }
                    }
                    else
                    {
                        //Since n is not divisible by all primes <= the square root of n, n is a prime number.
                        maxPrime = n;
                        primeList.Add(n);
                        break;
                    }
                }
            }
            stopWatch.Stop();
            ElapsedTime = stopWatch.ElapsedMilliseconds;
            GCCount = GC.CollectionCount(0) - gen0;
            //Insert 2 as the first prime number
            primeList.Insert(0, 2);

            //Toggle the running flag so that this async method can be run again.
            lock (this)
            {
                running = false;
            }

            return primeList;
        }
    }
}
