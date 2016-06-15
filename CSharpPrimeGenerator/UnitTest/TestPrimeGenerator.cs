using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using CSharpPrimeGenerator;
using System.Collections.Concurrent;

namespace UnitTest
{
    [TestClass]
    public class TestPrimeGenerator
    {
        //Got these prime numbers from http://www.infoplease.com/ipa/A0001737.html .
        private readonly uint[] PRIME_NUMBERS =
        {
            2,3,5,7,11,13,17,19,23,
            29,31,37,41,43,47,53,59,61,67,
            71,73,79,83,89,97,101,103,107,109,
            113,127,131,137,139,149,151,157,163,167,
            173,179,181,191,193,197,199,211,223,227,
            229,233,239,241,251,257,263,269,271,277,
            281,283,293,307,311,313,317,331,337,347,
            349,353,359,367,373,379,383,389,397,401,
            409,419,421,431,433,439,443,449,457,461,
            463,467,479,487,491,499,503,509,521,523,
            541,547,557,563,569,571,577,587,593,599,
            601,607,613,617,619,631,641,643,647,653,
            659,661,673,677,683,691,701,709,719,727,
            733,739,743,751,757,761,769,773,787,797,
            809,811,821,823,827,829,839,853,857,859,
            863,877,881,883,887,907,911,919,929,937,
            941,947,953,967,971,977,983,991,997
        };

        [TestMethod]
        public void TestBothGenerators_GeneratePrime()
        {
            var simplePrimeGenerator = new SimplePrimeGenerator();
            var optimizedGenerator = new OptimizedGenerator();

            //Create a cancellation token for stopping the prime number generation task.
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;
            var tasks = new ConcurrentBag<Task>();

            //Test for some prime numbers and compare these with a know prime number list.
            var limit = (uint) PRIME_NUMBERS[PRIME_NUMBERS.Length-1];
            var task1 = Task.Factory.StartNew(() => simplePrimeGenerator.GeneratePrime(limit, cancellationToken), cancellationToken);
            var task2 = Task.Factory.StartNew(() => optimizedGenerator.GeneratePrime(limit, cancellationToken), cancellationToken);
            tasks.Add(task1);
            tasks.Add(task2);

            //wait until all tasks are completed
            Task.WhenAll(tasks.ToArray());
            tokenSource.Dispose();

            //check if the task is not null
            Assert.IsTrue(task1 != null);
            Assert.IsTrue(task2 != null);

            var results1 = task1.Result.ToArray();
            var results2 = task2.Result.ToArray();

            //check if the number of prime numbers are the same.
            Assert.IsTrue(results1.Length == PRIME_NUMBERS.Length);
            Assert.IsTrue(results2.Length == PRIME_NUMBERS.Length);

            //check if every prime number are the same.
            for (int i = 0; i< PRIME_NUMBERS.Length; ++i)
            {
                Assert.IsTrue(results1[i] == PRIME_NUMBERS[i]);
                Assert.IsTrue(results2[i] == PRIME_NUMBERS[i]);
            }
        }

        [TestMethod]
        public void TestOptimizedGenerator_GeneratePrime()
        {
            var optimizedGenerator = new OptimizedGenerator();

            //Create a cancellation token for stopping the prime number generation task.
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            var limit = (uint)PRIME_NUMBERS[PRIME_NUMBERS.Length - 1];
            var resultList = optimizedGenerator.GeneratePrime(limit, cancellationToken);
            tokenSource.Dispose();

            Assert.IsTrue(resultList != null);

            var results = resultList.ToArray();

            //check if the number of prime numbers are the same.
            Assert.IsTrue(results.Length == PRIME_NUMBERS.Length);

            //check if every prime number are the same.
            for (int i = 0; i < PRIME_NUMBERS.Length; ++i)
            {
                Assert.IsTrue(results[i] == PRIME_NUMBERS[i]);
            }
        }

        [TestMethod]
        public void TestSimpleGenerator_GeneratePrime()
        {
            var optimizedGenerator = new SimplePrimeGenerator();

            //Create a cancellation token for stopping the prime number generation task.
            var tokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = tokenSource.Token;

            var limit = (uint)PRIME_NUMBERS[PRIME_NUMBERS.Length - 1];
            var resultList = optimizedGenerator.GeneratePrime(limit, cancellationToken);
            tokenSource.Dispose();

            Assert.IsTrue(resultList != null);

            var results = resultList.ToArray();

            //check if the number of prime numbers are the same.
            Assert.IsTrue(results.Length == PRIME_NUMBERS.Length);

            //check if every prime number are the same.
            for (int i = 0; i < PRIME_NUMBERS.Length; ++i)
            {
                Assert.IsTrue(results[i] == PRIME_NUMBERS[i]);
            }
        }
    }
}
