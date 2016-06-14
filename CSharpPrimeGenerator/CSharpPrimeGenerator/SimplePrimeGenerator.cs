using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpPrimeGenerator
{
    public class SimplePrimeGenerator
    {
        //Use the running flag to prevent GeneratePrimeAsync from running concurrent tasks.
        private bool running = false;

        //This has the max prime number generated from GeneratePrimeAsync.
        //A lock will not be put on this on assigning it.  Assigning a uint value is an atomic action.
        //A lock would enforce the order of the read and write but this is not important.  Performance is more important.
        private uint maxPrime = 2;

        public uint GetMaxPrime()
        {
            return maxPrime;
        }

        public async Task<List<uint>> GeneratePrimeAsync(uint max, CancellationToken ct)
        {
            lock (this)
            {
                if (running)
                {
                    //Exit if another task is running
                    return null;
                }
                else
                {
                    //Set running to true so that another concurrent process is prevented from running.
                    running = true;
                }
            }

            List<uint> results = null;

            if (ct.IsCancellationRequested == true)
            {
                ct.ThrowIfCancellationRequested();
                return null;
            }

            results = await Task.Run(() =>
            {
                //Exit early if the max is less than 5.
                if (max < 2)
                {
                    return null;
                }
                if (max == 2)
                {
                    return new List<uint>() { 2 };
                }
                if (max < 5)
                {
                    return new List<uint>() { 2, 3 };
                }

                //"2" is not included in the initial prime list because the event numbers are skipped.
                //Use List<uint> instead of uint[] for the collection because List<unit> doubles its capacity automatically.
                //There is an overhead on copying the old list into the new list when the capacity is increased.
                List<uint> primeList = new List<uint>() { 3, 5 };

                try
                {
                    //A number 'i' is prime if it is not divisible by every prime number less than or equal to the square root of 'i'.
                    //Skip even numbers.
                    for (uint i = 7; i <= max; i = i + 2)
                    {
                        uint sqrtPrime = (uint)Math.Sqrt((double)i);

                        foreach (uint prime in primeList)
                        {
                            if (ct.IsCancellationRequested)
                            {
                                //exit both the outer and inner loops
                                i = max + 1;
                                break;
                            }

                            //test divisibility for only the prime numbers that are less than the square root of 'i'
                            if (prime <= sqrtPrime)
                            {
                                //'i' is divisible by a prime if 'i' modulo prime is 0 (i/prime and its remainder is 0)
                                if (i % prime == 0)
                                {
                                    //'i' is not a prime number; exit the inner loop and test the next 'i'.
                                    break;
                                }
                            }
                            else
                            {
                                //Since 'i' is not divisible by the primes, it is a prime number.
                                maxPrime = i;
                                primeList.Add(i);
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    System.Console.WriteLine("Time is up!  Stop calculating prime numbers.");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"There is an error: {ex.Message}");
                }

                //Insert 2 as the first prime number
                primeList.Insert(0, 2);
                return primeList;
            }, ct);

            //Toggle the running flag so that this async method can be run again.
            lock (this)
            {
                running = false;
            }

            return results;
        }
    }
}
