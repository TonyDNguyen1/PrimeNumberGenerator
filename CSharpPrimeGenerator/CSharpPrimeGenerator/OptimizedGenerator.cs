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
        //Use the running flag to prevent GeneratePrime from running concurrently.
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
        private CancellationToken cancellationToken;

        //The number of milliseconds on running the loop for generating the prime numbers
        public long ElapsedTime { get; set; }

        //The number of times the garabage collector ran
        public int GCCount { get; set; }

        public OptimizedGenerator()
        {
            primeList = new List<uint>();
        }

        //return 0 if n is not a prime number else return n
        private uint isPrime(uint n)
        {
            uint sqrtPrime = (uint)Math.Sqrt((double)n);

            //It is thread safe to enumerate a List<T> if its content is not modified in the loop.
            //See https://social.msdn.microsoft.com/Forums/en-US/96fd1d18-6bb1-40f0-9ef0-017a614c3843/foreach-threadsafe-when-unterlying-connection-is-never-modified?forum=csharplanguage
            foreach (uint prime in primeList)
            {
                //A cancellation is called from the calling thread.  Time is up.
                if (cancellationToken.IsCancellationRequested)
                {
                    //exit the loop
                    return 0;
                }

                //Test divisibility for only the prime numbers that are less than or equal-to the square root of n
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
                    //Since n is not divisible by all primes <= the square root of n, n is a prime number.
                    return n;
                }
            }
            //If the last prime in primeList is less than sqrtPrime then n was not returned from the loop above.  It can be returned here.
            return n;
        }

        //This method generate the prime numbers from 2 to max.
        //The CancellationToken ct is used to stop the calculation early.
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

            //n is used to test for a prime number.  If n is a prime number, it would be added to the primeList.
            //n is enumerated by 2 for skipping the event numbers.          
            uint n = 7;
            maxPrime = 5;

            //Using concurrency, we can test multiple n's for prime numbers.
            //data will have multiple n's where maxPrime <= the square root of n.
            List<uint> data = new List<uint>();

            //We want to limit the number of items in data.
            //If data is too large and we received the cancellation call then we would waste the time used in running the last iteration.
            uint limit;
            int gen0 = GC.CollectionCount(0);
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while ((n <= max) && (cancellationToken.IsCancellationRequested == false))
            {
                data.Clear();
                limit = n + 65536;  //limit the number of items in data
                if (max < limit)
                {
                    limit = max;
                }
                while ((((uint)Math.Sqrt((double)n)) <= maxPrime) && (n <= limit))
                {
                    data.Add(n);
                    n += 2; //Skip even numbers.
                }

                //Run parrallel tasks to test for prime numbers
                var newPrimes = (
                    from prime in data.AsParallel()
                    select isPrime(prime)) //returns prime for prime number and 0 for non-prime
                    .Where(i => i > 0) //filter out the 0's
                    .ToList<uint>(); //put the prime numbers in a list so that the list can be sorted

                //If a cancellation call was received then the new prime numbers are not completed.  There can be some missing and we should not use these.               
                if ((ct.IsCancellationRequested == false) && (newPrimes.Count() > 0))
                {
                    //Sort the new prime numbers found
                    newPrimes.Sort();
                    //Add these to the primeList
                    primeList.AddRange(newPrimes);
                    //Set the maxPrime found
                    maxPrime = newPrimes.ElementAt(newPrimes.Count - 1);
                }
            }
            stopWatch.Stop();
            ElapsedTime = stopWatch.ElapsedMilliseconds; //set the elapsed time for running the loop
            GCCount = GC.CollectionCount(0) - gen0;

            //Insert 2 as the first prime number
            primeList.Insert(0, 2);

            //Toggle the running flag so that this method can be run again.
            lock (this)
            {
                running = false;
            }

            return primeList;
        }
    }
}
