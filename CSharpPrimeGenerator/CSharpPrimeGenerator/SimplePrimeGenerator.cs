using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPrimeGenerator
{
    public class SimplePrimeGenerator
    {
        public async Task<List<uint>> GeneratePrimeAsync(int max)
        {
            var result = await Task.Run(() =>
            {
                if (max < 2)
                {
                    return new List<uint>(); //return an empty list
                }
                if (max == 2)
                {
                    return new List<uint>() { 2 };
                }
                if (max < 5)
                {
                    return new List<uint>() { 2, 3 };
                }

                List<uint> primeList = new List<uint>() { 2, 3, 5 };
                for (uint i = 7; i <= max; i++)
                {
                    uint sqrtPrime = (uint)Math.Sqrt((double)i);

                    foreach (uint prime in primeList)
                    {
                        if (prime <= sqrtPrime)
                        {
                            if (i % prime == 0)
                            {
                                //i is not a prime number
                                break;
                            }
                        }
                        else
                        {
                            primeList.Add(i);
                            break;
                        }
                    }
                }
                return primeList;

            });

            return result;
        }
    }
}
