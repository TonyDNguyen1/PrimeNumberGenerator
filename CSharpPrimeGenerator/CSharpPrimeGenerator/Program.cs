using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPrimeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var simplePrimeGenerator = new SimplePrimeGenerator();
            var taskSimplePrimeGenerator = simplePrimeGenerator.GeneratePrimeAsync(100000000);
            Task.WhenAll(taskSimplePrimeGenerator);
            var primeList = taskSimplePrimeGenerator.Result;

            //List<uint> primeList = simplePrimeGenerator.GeneratePrimeAsync(1000000000).GetAwaiter().GetResult();
            /*foreach(var prime in primeList)
            {
                System.Console.WriteLine(prime);
            }*/
            System.Console.WriteLine(primeList.Count());
            System.Console.Read();
        }
    }
}
