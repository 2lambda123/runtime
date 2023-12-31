// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Security.Cryptography.Tests
{
    public class RandomNumberGeneratorTests
    {
        [Fact]
        public static void Create_ReturnsSingleton()
        {
            RandomNumberGenerator rng1 = RandomNumberGenerator.Create();
            RandomNumberGenerator rng2 = RandomNumberGenerator.Create();

            Assert.Same(rng1, rng2);
        }

        [Fact]
        public static void Singleton_NoopsDispose()
        {
            byte[] random = new byte[1024];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);
            RandomDataGenerator.VerifyRandomDistribution(random);
            rng.Dispose(); // should no-op if called once

            random = new byte[1024];
            rng.GetBytes(random); // should still work even after earlier Dispose call
            RandomDataGenerator.VerifyRandomDistribution(random);
            rng.Dispose(); // should no-op if called twice
        }

        [Theory]
        [InlineData(2048)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void RandomDistribution(int arraySize)
        {
            byte[] random = new byte[arraySize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
            }

            RandomDataGenerator.VerifyRandomDistribution(random);
        }

        [Fact]
        public static void IdempotentDispose()
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            for (int i = 0; i < 10; i++)
            {
                rng.Dispose();
            }
        }

        [Fact]
        public static void NullInput()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                Assert.Throws<ArgumentNullException>(() => rng.GetBytes(null));
            }
        }

        [Fact]
        public static void ZeroLengthInput()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                // While this will do nothing, it's not something that throws.
                rng.GetBytes(Array.Empty<byte>());
            }
        }

        [ConditionalFact(typeof(PlatformDetection), nameof(PlatformDetection.IsThreadingSupported))]
        public static void ConcurrentAccess()
        {
            const int ParallelTasks = 3;
            const int PerTaskIterationCount = 20;
            const int RandomSize = 1024;

            Task[] tasks = new Task[ParallelTasks];
            byte[][] taskArrays = new byte[ParallelTasks][];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            using (ManualResetEvent sync = new ManualResetEvent(false))
            {
                for (int iTask = 0; iTask < ParallelTasks; iTask++)
                {
                    taskArrays[iTask] = new byte[RandomSize];
                    byte[] taskLocal = taskArrays[iTask];

                    tasks[iTask] = Task.Run(
                        () =>
                        {
                            sync.WaitOne();

                            for (int i = 0; i < PerTaskIterationCount; i++)
                            {
                                rng.GetBytes(taskLocal);
                            }
                        });
                }

                // Ready? Set() Go!
                sync.Set();
                Task.WaitAll(tasks);
            }

            for (int i = 0; i < ParallelTasks; i++)
            {
                // The Real test would be to ensure independence of data, but that's difficult.
                // The other end of the spectrum is to test that they aren't all just new byte[RandomSize].
                // Middle ground is to assert that each of the chunks has random data.
                RandomDataGenerator.VerifyRandomDistribution(taskArrays[i]);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void GetNonZeroBytes_Array(int arraySize)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                AssertExtensions.Throws<ArgumentNullException>("data", () => rng.GetNonZeroBytes(null));

                // Array should not have any zeros
                byte[] rand = new byte[arraySize];
                rng.GetNonZeroBytes(rand);
                Assert.Equal(-1, Array.IndexOf<byte>(rand, 0));
            }
        }

        [Theory]
        [InlineData(400)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void GetBytes_Offset(int arraySize)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] rand = new byte[arraySize];

                // Set canary bytes
                rand[99] = 77;
                rand[399] = 77;

                rng.GetBytes(rand, 100, 200);

                // Array should not have been touched outside of 100-299
                Assert.Equal(99, Array.IndexOf<byte>(rand, 77, 0));
                Assert.Equal(399, Array.IndexOf<byte>(rand, 77, 300));

                // Ensure 100-300 has random bytes; not likely to ever fail here by chance (256^200)
                Assert.True(rand.Skip(100).Take(200).Sum(b => b) > 0);
            }
        }

        [Fact]
        public static void GetBytes_Array_Offset_ZeroCount()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] rand = new byte[1] { 1 };

                // A count of 0 should not do anything
                rng.GetBytes(rand, 0, 0);
                Assert.Equal(1, rand[0]);

                // Having an offset of Length is allowed if count is 0
                rng.GetBytes(rand, rand.Length, 0);
                Assert.Equal(1, rand[0]);

                // Zero-length array should not throw
                rand = Array.Empty<byte>();
                rng.GetBytes(rand, 0, 0);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void DifferentSequential_Array(int arraySize)
        {
            // Ensure that the RNG doesn't produce a stable set of data.
            byte[] first = new byte[arraySize];
            byte[] second = new byte[arraySize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(first);
                rng.GetBytes(second);
            }

            // Random being random, there is a chance that it could produce the same sequence.
            // The smallest test case that we have is 10 bytes.
            // The probability that they are the same, given a Truly Random Number Generator is:
            // Pmatch(byte0) * Pmatch(byte1) * Pmatch(byte2) * ... * Pmatch(byte9)
            // = 1/256 * 1/256 * ... * 1/256
            // = 1/(256^10)
            // = 1/1,208,925,819,614,629,174,706,176
            Assert.NotEqual(first, second);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void DifferentParallel(int arraySize)
        {
            // Ensure that two RNGs don't produce the same data series (such as being implemented via new Random(1)).
            byte[] first = new byte[arraySize];
            byte[] second = new byte[arraySize];

            using (RandomNumberGenerator rng1 = RandomNumberGenerator.Create())
            using (RandomNumberGenerator rng2 = RandomNumberGenerator.Create())
            {
                rng1.GetBytes(first);
                rng2.GetBytes(second);
            }

            // Random being random, there is a chance that it could produce the same sequence.
            // The smallest test case that we have is 10 bytes.
            // The probability that they are the same, given a Truly Random Number Generator is:
            // Pmatch(byte0) * Pmatch(byte1) * Pmatch(byte2) * ... * Pmatch(byte9)
            // = 1/256 * 1/256 * ... * 1/256
            // = 1/(256^10)
            // = 1/1,208,925,819,614,629,174,706,176
            Assert.NotEqual(first, second);
        }

        [Fact]
        public static void GetBytes_InvalidArgs()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                AssertExtensions.Throws<ArgumentNullException>("data", () => rng.GetNonZeroBytes(null));
                GetBytes_InvalidArgs_Helper(rng);
            }
        }

        [Fact]
        public static void GetBytes_InvalidArgs_Base()
        {
            using (var rng = new RandomNumberGeneratorMininal())
            {
                Assert.Throws<NotImplementedException>(() => rng.GetNonZeroBytes(null));
                GetBytes_InvalidArgs_Helper(rng);
            }
        }

        [Fact]
        public static void GetBytes_Int_Negative()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () =>
                RandomNumberGenerator.GetBytes(-1));
        }

        [Fact]
        public static void GetBytes_Int_Empty()
        {
            byte[] result = RandomNumberGenerator.GetBytes(0);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(2048)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void GetBytes_Int_RandomDistribution(int arraySize)
        {
            byte[] result = RandomNumberGenerator.GetBytes(arraySize);
            RandomDataGenerator.VerifyRandomDistribution(result);
        }

        [Theory]
        [InlineData(2048)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void GetBytes_Int_NotSame(int arraySize)
        {
            byte[] result1 = RandomNumberGenerator.GetBytes(arraySize);
            byte[] result2 = RandomNumberGenerator.GetBytes(arraySize);
            Assert.NotEqual(result1, result2);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void DifferentSequential_Span(int arraySize)
        {
            // Ensure that the RNG doesn't produce a stable set of data.
            var first = new byte[arraySize];
            var second = new byte[arraySize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes((Span<byte>)first);
                rng.GetBytes((Span<byte>)second);
            }

            // Random being random, there is a chance that it could produce the same sequence.
            // The smallest test case that we have is 10 bytes.
            // The probability that they are the same, given a Truly Random Number Generator is:
            // Pmatch(byte0) * Pmatch(byte1) * Pmatch(byte2) * ... * Pmatch(byte9)
            // = 1/256 * 1/256 * ... * 1/256
            // = 1/(256^10)
            // = 1/1,208,925,819,614,629,174,706,176
            Assert.NotEqual(first, second);
        }

        [Fact]
        public static void GetBytes_Span_ZeroCount()
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                var rand = new byte[1] { 1 };
                rng.GetBytes(new Span<byte>(rand, 0, 0));
                Assert.Equal(1, rand[0]);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(256)]
        [InlineData(65536)]
        [InlineData(1048576)]
        public static void GetNonZeroBytes_Span(int arraySize)
        {
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                var rand = new byte[arraySize];
                rng.GetNonZeroBytes(new Span<byte>(rand));
                Assert.Equal(-1, Array.IndexOf<byte>(rand, 0));
            }
        }

        [Fact]
        public static void Fill_ZeroLengthSpan()
        {
            byte[] rand = { 1 };
            RandomNumberGenerator.Fill(new Span<byte>(rand, 0, 0));
            Assert.Equal(1, rand[0]);
        }

        [Fact]
        public static void Fill_SpanLength1()
        {
            byte[] rand = { 1 };
            bool replacedValue = false;

            for (int i = 0; i < 10; i++)
            {
                RandomNumberGenerator.Fill(rand);

                if (rand[0] != 1)
                {
                    replacedValue = true;
                    break;
                }
            }

            Assert.True(replacedValue, "Fill eventually wrote a different byte");
        }

        [Fact]
        public static void Fill_RandomDistribution()
        {
            byte[] random = new byte[2048];
            RandomNumberGenerator.Fill(random);

            RandomDataGenerator.VerifyRandomDistribution(random);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData(10, 9)]
        [InlineData(-10, -10)]
        [InlineData(-10, -11)]
        public static void GetInt32_LowerAndUpper_InvalidRange(int fromInclusive, int toExclusive)
        {
            Assert.Throws<ArgumentException>(() => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public static void GetInt32_Upper_InvalidRange(int toExclusive)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => RandomNumberGenerator.GetInt32(toExclusive));
        }

        [Theory]
        [InlineData(1 << 1)]
        [InlineData(1 << 4)]
        [InlineData(1 << 16)]
        [InlineData(1 << 24)]
        public static void GetInt32_PowersOfTwo(int toExclusive)
        {
            for (int i = 0; i < 10; i++)
            {
                int result = RandomNumberGenerator.GetInt32(toExclusive);
                Assert.InRange(result, 0, toExclusive - 1);
            }
        }

        [Theory]
        [InlineData((1 << 1) + 1)]
        [InlineData((1 << 4) + 1)]
        [InlineData((1 << 16) + 1)]
        [InlineData((1 << 24) + 1)]
        public static void GetInt32_PowersOfTwoPlusOne(int toExclusive)
        {
            for (int i = 0; i < 10; i++)
            {
                int result = RandomNumberGenerator.GetInt32(toExclusive);
                Assert.InRange(result, 0, toExclusive - 1);
            }
        }

        [Fact]
        public static void GetInt32_FullRange()
        {
            int result = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            Assert.NotEqual(int.MaxValue, result);
        }

        [Fact]
        public static void GetInt32_DoesNotProduceSameNumbers()
        {
            int result1 = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            int result2 = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
            int result3 = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);

            // The changes of this happening are (2^32 - 1) * 3.
            Assert.False(result1 == result2 && result2 == result3, "Generated the same number three times in a row.");
        }

        [Fact]
        public static void GetInt32_FullRange_DistributesBitsEvenly()
        {
            // This test should work since we are selecting random numbers that are a
            // Power of two minus one so no bit should favored.
            int numberToGenerate = 512;
            byte[] bytes = new byte[numberToGenerate * 4];
            Span<byte> bytesSpan = bytes.AsSpan();
            for (int i = 0, j = 0; i < numberToGenerate; i++, j += 4)
            {
                int result = RandomNumberGenerator.GetInt32(int.MinValue, int.MaxValue);
                Span<byte> slice = bytesSpan.Slice(j, 4);
                BinaryPrimitives.WriteInt32LittleEndian(slice, result);
            }
            RandomDataGenerator.VerifyRandomDistribution(bytes);
        }

        [Fact]
        public static void GetInt32_CoinFlipLowByte()
        {
            int numberToGenerate = 2048;
            Span<int> generated = stackalloc int[numberToGenerate];

            for (int i = 0; i < numberToGenerate; i++)
            {
                generated[i] = RandomNumberGenerator.GetInt32(0, 2);
            }
            VerifyAllInRange(generated, 0, 2);
            VerifyDistribution<int>(generated, 0.5);
        }


        [Fact]
        public static void GetInt32_CoinFlipOverByteBoundary()
        {
            int numberToGenerate = 2048;
            Span<int> generated = stackalloc int[numberToGenerate];

            for (int i = 0; i < numberToGenerate; i++)
            {
                generated[i] = RandomNumberGenerator.GetInt32(255, 257);
            }
            VerifyAllInRange(generated, 255, 257);
            VerifyDistribution<int>(generated, 0.5);
        }

        [Fact]
        public static void GetInt32_NegativeBounds1000d20()
        {
            int numberToGenerate = 10_000;
            Span<int> generated = new int[numberToGenerate];

            for (int i = 0; i < numberToGenerate; i++)
            {
                generated[i] = RandomNumberGenerator.GetInt32(-4000, -3979);
            }
            VerifyAllInRange(generated, -4000, -3979);
            VerifyDistribution<int>(generated, 0.05);
        }

        [Fact]
        public static void GetInt32_1000d6()
        {
            int numberToGenerate = 10_000;
            Span<int> generated = new int[numberToGenerate];

            for (int i = 0; i < numberToGenerate; i++)
            {
                generated[i] = RandomNumberGenerator.GetInt32(1, 7);
            }
            VerifyAllInRange(generated, 1, 7);
            VerifyDistribution<int>(generated, 0.16);
        }

        [Theory]
        [InlineData(int.MinValue, int.MinValue + 3)]
        [InlineData(-257, -129)]
        [InlineData(-100, 5)]
        [InlineData(254, 512)]
        [InlineData(-1_073_741_909, - 1_073_741_825)]
        [InlineData(65_534, 65_539)]
        [InlineData(16_777_214, 16_777_217)]
        public static void GetInt32_MaskRangeCorrect(int fromInclusive, int toExclusive)
        {
            int numberToGenerate = 10_000;
            Span<int> generated = new int[numberToGenerate];

            for (int i = 0; i < numberToGenerate; i++)
            {
                generated[i] = RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
            }

            double expectedDistribution = 1d / (toExclusive - fromInclusive);
            VerifyAllInRange(generated, fromInclusive, toExclusive);
            VerifyDistribution<int>(generated, expectedDistribution);
        }

        [Fact]
        public static void GetItems_Choices_Empty_ArgumentException()
        {
            AssertExtensions.Throws<ArgumentException>("choices",
                () => RandomNumberGenerator.GetItems(ReadOnlySpan<int>.Empty, 6));
            AssertExtensions.Throws<ArgumentException>("choices",
                () => RandomNumberGenerator.GetItems(ReadOnlySpan<int>.Empty, stackalloc int[6]));
        }

        [Fact]
        public static void GetString_Choices_Empty_ArgumentException()
        {
            AssertExtensions.Throws<ArgumentException>("choices",
                () => RandomNumberGenerator.GetString(ReadOnlySpan<char>.Empty, 6));
        }

        [Fact]
        public static void GetItems_NegativeLength_ArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length",
                () => RandomNumberGenerator.GetItems<int>(new int[1], -1));
        }

        [Fact]
        public static void GetString_NegativeLength_ArgumentOutOfRangeException()
        {
            AssertExtensions.Throws<ArgumentOutOfRangeException>("length", () => RandomNumberGenerator.GetString("a", -1));
        }

        [Fact]
        public static void GetItems_Empty()
        {
            ReadOnlySpan<int> choices = stackalloc int[] { 1 };
            Span<int> values = RandomNumberGenerator.GetItems(choices, 0);
            Assert.True(values.IsEmpty, "values.IsEmpty");
        }

        [Fact]
        public static void GetString_Empty()
        {
            const string Choices = "a";
            string result = RandomNumberGenerator.GetString(Choices, 0);
            Assert.Empty(result);
        }

        [Fact]
        public static void GetItems_SingleChoice()
        {
            ReadOnlySpan<int> choice = stackalloc int[] { 1 };
            Span<int> values = RandomNumberGenerator.GetItems(choice, 256);
            AssertExtensions.FilledWith(1, values);

            values.Clear();
            RandomNumberGenerator.GetItems(choice, values);
            AssertExtensions.FilledWith(1, values);
        }

        [Fact]
        public static void GetString_SingleChoice()
        {
            string result = RandomNumberGenerator.GetString("!", 42);
            Assert.Equal(new string('!', 42), result);
        }

        [Fact]
        public static void GetItems_CoinFlip_Int_RandomDistribution()
        {
            Span<int> generated = RandomNumberGenerator.GetItems<int>(stackalloc int[] { 1, 2 }, 10_000);
            VerifyAllInRange(generated, fromInclusive: 1, toExclusive: 3);
            VerifyDistribution<int>(generated, 0.5);

            generated.Clear();
            RandomNumberGenerator.GetItems<int>(stackalloc int[] { 1, 2 }, generated);
            VerifyAllInRange(generated, fromInclusive: 1, toExclusive: 3);
            VerifyDistribution<int>(generated, 0.5);
        }

        [Fact]
        public static void GetItems_CoinFlip_Bool_RandomDistribution()
        {
            ReadOnlySpan<bool> choices = stackalloc bool[] { true, false };
            Span<bool> generated = RandomNumberGenerator.GetItems(choices, 10_000);
            VerifyDistribution<bool>(generated, 0.5);

            generated.Clear();
            RandomNumberGenerator.GetItems(choices, generated);
            VerifyDistribution<bool>(generated, 0.5);
        }

        [Fact]
        public static void GetItems_Bool_NoDeduplication()
        {
            ReadOnlySpan<bool> choices = stackalloc bool[] { true, true, false };
            Span<bool> generated = RandomNumberGenerator.GetItems(choices, 10_000);
            VerifyDistribution(generated, new Dictionary<bool, double>() { [true] = 0.66, [false] = 0.33 });

            generated.Clear();
            RandomNumberGenerator.GetItems(choices, generated);
            VerifyDistribution(generated, new Dictionary<bool, double>() { [true] = 0.66, [false] = 0.33 });
        }

        [Fact]
        public static void GetItems_Int_NoDeduplication()
        {
            ReadOnlySpan<int> choices = stackalloc int[] { 5, 5, 5, 20 };
            Span<int> generated = RandomNumberGenerator.GetItems(choices, 10_000);
            VerifyDistribution(generated, new Dictionary<int, double>() { [5] = 0.75, [20] = 0.25 });

            generated.Clear();
            RandomNumberGenerator.GetItems(choices, generated);
            VerifyDistribution(generated, new Dictionary<int, double>() { [5] = 0.75, [20] = 0.25 });
        }

        [Fact]
        public static void GetString_RandomDistribution()
        {
            const string Choices = "abcdefghijklmnopqrstuvwxyz";
            for (int i = 1; i <= Choices.Length; i++)
            {
                string generated = RandomNumberGenerator.GetString(Choices.AsSpan(0, i), 10_000);
                VerifyDistribution<char>(generated, 1f / i);
                VerifyAllInRange(generated, 'a', (char)('a' + i));
            }
        }

        [Fact]
        public static void GetString_NoDeduplication()
        {
            const string Choices = "chess";
            string generated = RandomNumberGenerator.GetString(Choices, 10_000);
            VerifyDistribution<char>(
                generated,
                new Dictionary<char, double>() { ['c'] = 0.2, ['h'] = 0.2, ['e'] = 0.2, ['s'] = 0.4 });
        }

        [Fact]
        public static void GetHexString_Empty()
        {
            string result = RandomNumberGenerator.GetHexString(stringLength: 0);
            Assert.Empty(result);

            Span<char> buffer = Span<char>.Empty;
            RandomNumberGenerator.GetHexString(buffer); // Shouldn't throw.
        }

        [Theory]
        [MemberData(nameof(GetHexStringLengths))]
        public static void GetHexString_Allocating_Random(int length)
        {
            // We generate 256 strings and verify that there is no offset into the strings at which all strings contain
            // the same character. The odds of throwing 256 d16's and all of them landing the same is nearly nothing so
            // this would be a good signal that the random buffer is not being filled correctly.
            string[] samples = new string[256];
            HashSet<char> hashSet = new HashSet<char>(samples.Length);

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = RandomNumberGenerator.GetHexString(length);
            }

            for (int index = 0; index < length; index++)
            {
                foreach (string sample in samples)
                {
                    hashSet.Add(sample[index]);
                }

                AssertExtensions.GreaterThan(hashSet.Count, 1, $"Random character at position {index}.");
                hashSet.Clear();
            }
        }
        [Theory]
        [MemberData(nameof(GetHexStringLengths))]
        public static void GetHexString_Buffer_Random(int length)
        {
            // We generate 256 strings and verify that all positions in those 256 strings don't contain identical
            // characters. The odds of throwing 256 d16's and all of them landing the same is nearly nothing so
            // this would be a good signal that the random buffer is not being filled correctly.
            char[][] samples = new char[256][];
            HashSet<char> hashSet = new HashSet<char>(samples.Length);

            for (int i = 0; i < samples.Length; i++)
            {
                char[] buffer = new char[length];
                RandomNumberGenerator.GetHexString(buffer);
                samples[i] = buffer;
            }

            for (int index = 0; index < length; index++)
            {
                foreach (char[] sample in samples)
                {
                    hashSet.Add(sample[index]);
                }

                AssertExtensions.GreaterThan(hashSet.Count, 1, $"Random character at position {index}.");
                hashSet.Clear();
            }
        }

        [Theory]
        [MemberData(nameof(GetHexStringLengths))]
        public static void GetHexString_Conformance(int length)
        {
            bool[] casing = new bool[] { true, false };

            foreach (bool lowercase in casing)
            {
                string result = RandomNumberGenerator.GetHexString(length, lowercase);
                Span<char> spanResult = new char[length];
                spanResult.Fill('!'); // Fill it with something non-hex that is obvious.
                RandomNumberGenerator.GetHexString(spanResult, lowercase);

                AssertHexString(result, spanResult, lowercase);
            }

            static void AssertHexString(string allocatedString, Span<char> bufferString, bool generatedLowercase)
            {
                Assert.Equal(allocatedString.Length, bufferString.Length);

                for (int i = 0; i < allocatedString.Length; i++)
                {
                    if (generatedLowercase)
                    {
                        Assert.True(
                            char.IsAsciiHexDigitLower(bufferString[i]),
                            $"Non-lower hex character at position {i} in string {bufferString}");

                        Assert.True(
                            char.IsAsciiHexDigitLower(allocatedString[i]),
                            $"Non-lower hex character at position {i} in string {allocatedString}");
                    }
                    else
                    {
                        Assert.True(
                            char.IsAsciiHexDigitUpper(bufferString[i]),
                            $"Non-upper hex character at position {i} in string {bufferString}");

                        Assert.True(
                            char.IsAsciiHexDigitUpper(allocatedString[i]),
                            $"Non-upper hex character at position {i} in string {allocatedString}");
                    }
                }
            }
        }

        [Fact]
        public static void Shuffle_Empty()
        {
            RandomNumberGenerator.Shuffle(Span<int>.Empty);
        }

        [Fact]
        public static void Shuffle_One()
        {
            int[] buffer = new int[] { 42 };
            RandomNumberGenerator.Shuffle<int>(buffer);
            Assert.Equal(new int[] { 42 }, buffer);
        }

        [Fact]
        public static void Shuffle_Two()
        {
            ReadOnlySpan<int> numbers = new int[] { 42, 56 };

            int[][] shuffles = new int[10_000][];

            for (int i = 0; i < shuffles.Length; i++)
            {
                shuffles[i] = numbers.ToArray(); // Copy it.
                RandomNumberGenerator.Shuffle<int>(shuffles[i]);
            }

            int[] firstNumber = shuffles.Select(x => x[0]).ToArray();
            int[] secondNumber = shuffles.Select(x => x[1]).ToArray();

            // For the first and second digit, in our shuffles, we expect half of them in each location.
            VerifyDistribution<int>(firstNumber, 0.5);
            VerifyDistribution<int>(secondNumber, 0.5);
        }

        [Fact]
        public static void Shuffle_Ten()
        {
            ReadOnlySpan<int> numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int[][] shuffles = new int[10_000][];

            for (int i = 0; i < shuffles.Length; i++)
            {
                shuffles[i] = numbers.ToArray(); // Copy it.
                RandomNumberGenerator.Shuffle<int>(shuffles[i]);
            }

            for (int index = 0; index < numbers.Length; index++)
            {
                int[] shuffledNumbers = shuffles.Select(x => x[index]).ToArray();
                VerifyDistribution<int>(shuffledNumbers, 0.1);
            }
        }

        public static IEnumerable<object[]> GetHexStringLengths()
        {
            // These lengths exercise various aspects of the the hex generator.
            // Fill an individual character (I) if the length is odd.
            // Fill the remaining in block (B) sizes of 64 bytes, or 128 hex characters.

            return new object[][]
            {
                new object[] { 1 }, // I: yes; B: 0
                new object[] { 12 }, // I: no; B: 1
                new object[] { 13 }, // I: yes; B: 1
                new object[] { 127 }, // I: yes; B: 1. Note: smallest possible "partial" block.
                new object[] { 128 }, // I: no; B: 1. Note: exactly 1 block.
                new object[] { 129 }, // I: yes; B: 1. Note: exactly 1 block.
                new object[] { 140 }, // I: no; B: 2. Note: 1 complete block another partial block of 12.
                new object[] { 141 }, // I: yes; B: 2. Note: 1 complete block another partial block of 12, plus an I.
                new object[] { 255 }, // I: yes; B: 2. Note: 1 complete block another partial block of 126, plus an I.
                new object[] { 256 }, // I: no; B: 2. Note: exactly two blocks.
                new object[] { 257 }, // I: yes; B: 2. Note: exactly two blocks plus an I.
                new object[] { 280 }, // I: no; B: 3. Note: two whole blocks and a partial block of 24.
                new object[] { 281 }, // I: yes; B: 3. Note: two whole blocks and a partial block of 24, plus an I.
                new object[] { 1024 }, // I: no; B 8. Note: exactly 8 blocks.
                new object[] { 1025 }, // I: yes; B 8. Note: exactly 8 blocks, plus an I.
                new object[] { 1026 }, // I: no; B: 9. Note: 8 blocks plus another partial block of 2.
                new object[] { 1027 }, // I: yes; B: 9. Note: 8 blocks plus another partial block of 2, and an I.
            };
        }

        private static void VerifyAllInRange<T>(ReadOnlySpan<T> numbers, T fromInclusive, T toExclusive) where T : INumber<T>
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                Assert.InRange<T>(numbers[i], fromInclusive, toExclusive - T.One);
            }
        }

        private static void VerifyDistribution<T>(ReadOnlySpan<T> values, double expected)
        {
            var observedValues = new Dictionary<T, int>(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                T value = values[i];
                if (!observedValues.TryAdd(value, 1))
                {
                    observedValues[value]++;
                }
            }

            const double tolerance = 0.07;
            foreach ((_, int occurrences) in observedValues)
            {
                double percentage = occurrences / (double)values.Length;
                double actual = Math.Abs(expected - percentage);
                Assert.True(actual < tolerance, $"Occurred number of times within threshold. Actual: {actual}");
            }
        }

        private static void VerifyDistribution<T>(ReadOnlySpan<T> values, Dictionary<T, double> distribution)
        {
            var observedValues = new Dictionary<T, int>(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                T value = values[i];
                if (!observedValues.TryAdd(value, 1))
                {
                    observedValues[value]++;
                }
            }

            const double tolerance = 0.07;
            foreach ((T value, int occurrences) in observedValues)
            {
                double expected = distribution[value];
                double percentage = occurrences / (double)values.Length;
                double actual = Math.Abs(expected - percentage);
                Assert.True(actual < tolerance, $"'{value}' occurred number of times within threshold. Actual: {actual}");
            }
        }

        private static void GetBytes_InvalidArgs_Helper(RandomNumberGenerator rng)
        {
            AssertExtensions.Throws<ArgumentNullException>("data", () => rng.GetBytes(null, 0, 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("offset", () => rng.GetBytes(Array.Empty<byte>(), -1, 0));
            AssertExtensions.Throws<ArgumentOutOfRangeException>("count", () => rng.GetBytes(Array.Empty<byte>(), 0, -1));
            AssertExtensions.Throws<ArgumentException>(null, () => rng.GetBytes(Array.Empty<byte>(), 0, 1));
            // GetBytes(null) covered in test NullInput()
        }

        private class RandomNumberGeneratorMininal : RandomNumberGenerator
        {
            public override void GetBytes(byte[] data)
            {
                // Empty; don't throw NotImplementedException
            }
        }
    }
}
