using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CSharpPrimeGenerator
{
    public class OptimizedGenerator
    {
        //Use the running flag to prevent GeneratePrimeAsync from running concurrent tasks.
        private bool running = false;

        //This has the max prime number generated from GeneratePrime.
        //A lock will not be put on this on assigning it.  Assigning a uint value is an atomic action.
        //A lock would enforce the order of the read and write but this is not important.  Performance is more important.
        private uint maxPrime = 2;

        public uint GetMaxPrime()
        {
            return maxPrime;
        }

        //Use List<uint> instead of uint[] for the collection because List<unit> doubles its capacity automatically.
        //There is an overhead on copying the old list into the new list when the capacity is increased.
        private List<uint> primeList = null;
        private CancellationToken cancellationToken;
        public long ElapsedTime { get; set; }
        public int GCCount { get; set; }

        public OptimizedGenerator()
        {
            primeList = new List<uint>();
        }

        //return 0 if n is not a prime number else return n
        private uint isPrime(uint n)
        {
            uint sqrtPrime = (uint)Math.Sqrt((double)n);

            foreach (uint prime in primeList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //exit both the outer and inner loops
                    return 0;
                }

                //test divisibility for only the prime numbers that are less than the square root of n
                if (prime <= sqrtPrime)
                {
                    //n is divisible by a prime if n modulo prime is 0 (n/prime and its remainder is 0)
                    if (n % prime == 0)
                    {
                        //n is not a prime number; exit the loop
                        return 0;
                    }
                }
                else
                {
                    //Since n is not divisible by the primes, it is a prime number.
                    return n;
                }
            }
            return 0;
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
            cancellationToken = ct;

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

            //A number 'i' is prime if it is not divisible by every prime number less than or equal to the square root of 'i'.            
            uint i = 7;
            maxPrime = 5;
            uint sqrtPrime;
            uint sq;
            List<uint> data = new List<uint>();
            uint limit;
            int gen0 = GC.CollectionCount(0);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while ((i <= max) && (cancellationToken.IsCancellationRequested == false))
            {
                data.Clear();
                limit = i + 65536;  //limit the number of items in data
                if (max < limit)
                {
                    limit = max;
                }
                while ((((uint)Math.Sqrt((double)i)) <= maxPrime) && (i <= limit))
                {
                    data.Add(i);
                    i += 2; //Skip even numbers.
                }

                var newPrimes = (
                    from prime in data.AsParallel()
                    select isPrime(prime)
                    ).Where(n => n > 0).ToList<uint>();
                if ((newPrimes != null) && (newPrimes.Count() > 0))
                {
                    newPrimes.Sort();
                    primeList.AddRange(newPrimes);
                    maxPrime = newPrimes.ElementAt(newPrimes.Count - 1);
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
