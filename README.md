# PrimeNumberGenerator

Requirements:
1.  Find a max prime number and display it with a timer progress.
2.  Repeat for 60 seconds.

Assumptions:
1.  Find all prime numbers up-to the max prime number.
2.  Do not need to display every prime number with a timer progress.  A max prime number can be displayed every second with a counter as a timer progress.

Algorithms:
1.  Sieve of Eratosthenes (https://en.wikipedia.org/wiki/Sieve_of_Eratosthenes)
2.  Sieve of Atkin (https://en.wikipedia.org/wiki/Sieve_of_Atkin)
3.  primality test - simple method (https://en.wikipedia.org/wiki/Primality_test)

Tool:
Visual Studio 2015 C# .NET

Approach:
-Use primality test as it does not have the overhead on running like a batch.  The other 2 algorithms are batch like and cannot be easily cancelled when 60 sec is up.

Pseudocode
Given a number max > 1.
Array A has the found prime numbers starting with {3,5}.
for n=7,9,11,...max (increment by 2 skipping evens)
	for i=0,1,2,..A.Length
		if A[i] <= square root of n
			if n % A[i] == 0 (n is divisible by A[i])
				n is not a prime
				break inner loop and test next (n+2)
		else
			n is prime;
			Append n to A[];
			break inner loop and test next (n+2)

C# limitation:
-Use a value type uint for representing a prime number; UInt32.MaxValue: 4,294,967,295
-Use a dynamic arrary List<uint> for storing a list of primes.  The max list size is UInt32.MaxValue/2, 2GB.  The list capacity is doubled when it needs to increase.  There is an overhead on copying the old list to the new list.

Unit test:
Use the primality test with known prime numbers to validate the output.
