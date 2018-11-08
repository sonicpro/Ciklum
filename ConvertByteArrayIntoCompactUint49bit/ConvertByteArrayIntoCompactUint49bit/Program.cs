using System;
using System.Linq;

namespace ConvertByteArrayIntoCompactUint49bit
{
	class Program
	{
		static void Main(string[] args)
		{
			// Let's encode the highest possible value for storing in 49bit compact Uint, that is 0x1FFFFFFFFFFFF, or 562,949,953,421,311 decimal.
			// The lowest byte must be 10000001 (129 decimal), rest bytes are 0xFF (255 decimal).
			byte[] compactLargeValueInBytes = new[] { (byte)129 };
			var byteArray = compactLargeValueInBytes.Concat(Enumerable.Repeat((byte)255, 6)).ToArray();
			CompactUint49bit result = ConvertToUint49bit(byteArray);
			if (result.Uint == 562949953421311L)
			{
				Console.WriteLine("Conversion is successful.");
			}
		}

		// Takes an array of bytes. The first 7 bytes must represent an unsigned integer in the range 0x0040000000000 through 0x1FFFFFFFFFFFF
		// encoded into 49 bit according to MS-FSSHTTPB specification.
		private static CompactUint49bit ConvertToUint49bit(byte[] bytes)
		{
			if (bytes.Length < 7)
			{
				throw new ArgumentException("The array must be at least 7-byte long.");
			}

			// First seven bits of the least significant byte should the flag denoting the 49-bit compact value type.
			// The flag must be equal 64 decimal. 64 decimal = 1000000 binary, let's test the hightest seven bit of the byte 0 to be "1000000".
			if ((bytes[0] & 254) != 128)
			{
				throw new NotSupportedException("The bytes assume to be encoded as 49-bit Uint type. The flag bits are incorrect.");
			}

			// Take the lowest bit of the least significant byte and store it for later. That bit will contribute into the compact integer value.
			// It is not clear from the MS-FSSHTTPB specification whether that bit should be the highest from the 49 meaningful bits or the lowest.
			// Lets assume that it is the highest bit and it will contribute to the final value 16^12=281,474,976,710,656.
			bool isMostSignificantBitSet = (bytes[0] & 1) == 1;

			// Rest six bytes are byte-ordered with the least significant byte stored in the memory location with the lowest address.
			// Let's evaluate the compact integer value ignoring the 49-th bit for the moment.
			ulong value = 0L;
			for (int i = 1; i != 7; i++)
			{
				var zeroBasedByteIndex = i - 1;
				ulong byteContribution = (ulong)(zeroBasedByteIndex == 0 ? 1 : (256L << (8 * (zeroBasedByteIndex - 1))));
				value += bytes[i] * byteContribution;
			}

			// Add the 49th bit contribution if needed.
			if (isMostSignificantBitSet)
			{
				value += 281474976710656;
			}
			return new CompactUint49bit
			{
				Type = 64,
				Uint = value
			};
		}
	}
}
