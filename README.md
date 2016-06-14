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
-Use Sieve of Eratosthenes to generate the prime numbers as it can be calculated using delta n.
-Calculate the first 4096 prime numbers.
-If this runs under 0.3 second then find the next 8192;
-If this runs between 0.3-0.5 second then find the next 4096;
-If this runs more than 0.5 second then find the next 2048.
-Continue to double or half the delta n so that each batch runs about 0.5 second.

Pseudocode
Given a max prime number n > 1.
Create a bool/bit array A of n/2 size.  Leave the initial values default to false.
for i=1,2,3,..< ((square root of n)-1)/2:
	if A[i] is false:
		v = i*2+1; v has only odds consisting of 3, 5, 7,..<(square root of n)
		for j = v*v, v*v + v, v*v + 2v, v*v + 3v,..<n:
			A[(j-1)/2] = true

Prime numbers: 2 and all (i*2+1) such that A[i] is false.
To add a delta n, we need to save the max i and max j.  Run the j loop for the delta n, and run the i loop for the delta sqrt n.

C# limitation:
-Use value type Uint for representing a prime number; UInt32.MaxValue: 4,294,967,295
-Use dyanamic arrary List<bool> for storing a list of primes.  The max list size is UInt32.MaxValue/2, 2GB.  The list capacity is doubled when it needs to increase.  There is an overhead on copying the old list to the new list.
-Use Dictionary<i,j> for caching the last j < n.

Unit test:
Use the primality test simple method to validate the output prime numbers.
