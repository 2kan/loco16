using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loco16
{
	public static class Codec
	{
		#region Decode
		/// <summary>
		/// Decodes chunk data using run-length decoding
		/// </summary>
		/// <param name="a_input">Chunk data to decode, without header.</param>
		/// <returns>Run-length decoded data</returns>
		public static byte[] RunLengthDecode ( byte[] a_chunkData )
		{
			byte[] decoded = new byte[a_chunkData.Length * 8];

			int curSize = 0;

			for ( int i = 0; i < a_chunkData.Length; ++i )
			{
				int offset = 0;

				if ( a_chunkData[i] <= 0x7f )
				{
					// Copy next N+1 bytes
					for ( int k = 0; k < a_chunkData[i] + 1; ++k )
					{
						decoded[curSize] = a_chunkData[i + 1 + k];
						++curSize;
						++offset;
					}
				}
				else
				{
					// Copy the next byte N times
					for ( int k = 0; k < 257 - a_chunkData[i]; ++k )
					{
						decoded[curSize] = a_chunkData[i + 1];
						++curSize;
					}
					++offset;
				}

				i += offset;
			}

			return decoded.Take( curSize ).ToArray();
		}

		/// <summary>
		/// Unrotates chunk data by using circular bit shifting
		/// </summary>
		/// <param name="a_chunkData">Chunk data to decode, without header.</param>
		/// <returns>Data that has been un-rotated</returns>
		public static byte[] Unrotate ( byte[] a_chunkData )
		{
			byte[] decoded = new byte[a_chunkData.Length];

			byte[] rotation = new byte[] { 1, 3, 5, 7 };
			int rotateIndex = 0;

			for ( int i = 0; i < a_chunkData.Length; ++i )
			{
				// Rotate right
				decoded[i] = (byte) (
					( a_chunkData[i] >> rotation[rotateIndex] ) |
					( a_chunkData[i] << ( 8 - rotation[rotateIndex] ) )
				);

				rotateIndex = ( rotateIndex + 1 ) % rotation.Length;
			}

			return decoded;
		}

		/// <summary>
		/// Returns the bytes, unchanged. Will probably remove this
		/// </summary>
		/// <param name="a_chunkData">Chunk data to decode, without header.</param>
		/// <returns>The input</returns>
		public static byte[] CopyBytes ( byte[] a_chunkData )
		{
			return a_chunkData;
		}

		/// <summary>
		/// Decodes chunk data from LZ compression (it's not actually LZ, but it's pretty close)
		/// </summary>
		/// <param name="a_chunkData">Chunk data to decode, without header.</param>
		/// <returns>Byte array of decoded data</returns>
		public static byte[] LZDecode ( byte[] a_chunkData )
		{
			byte[] decoded = new byte[a_chunkData.Length * 8];

			int curSize = 0;

			for ( int i = 0; i < a_chunkData.Length; ++i )
			{
				if ( a_chunkData[i] == 0xff )
				{
					decoded[curSize] = a_chunkData[i + 1];
					++curSize;
					++i; // Skip the next byte because we just used it
				}
				else
				{
					var upper = a_chunkData[i] >> 3; // Get upper 5 bits
					var lower = a_chunkData[i] & 0x7; // Get lower 3 bits

					// Upper defines the offset of the selected byte
					// -- selected byte = 32 - upper
					// Lower defines how many times to repeat the selected data + 1

					int timesRepeated = lower + 1;
					int byteOffset = 32 - upper;

					for ( int k = 0; k < timesRepeated; ++k )
					{
						decoded[curSize] = decoded[curSize - byteOffset];
						++curSize;
					}
				}
			}

			return decoded.Take( curSize ).ToArray();
		}
		#endregion

		#region Encode
		/// <summary>
		/// Run-length encodes given data according to Loco's specs.
		/// </summary>
		/// <param name="a_data">Data to encode</param>
		/// <returns>Run-length encoded data</returns>
		public static byte[] RunLengthEncode ( byte[] a_data )
		{
			byte[] encoded = new byte[a_data.Length];
			int curSize = 0;
			//int maxCopyNext = 127;
			//int maxRepeat = 128;

			for ( int i = 0; i < a_data.Length; ++i )
			{
				byte currentByte = a_data[i];

				int numInSequence = GetSequenceLength( currentByte, a_data, i );

				if ( numInSequence > 1 )
				{
					encoded[curSize++] = (byte) ( 257 - numInSequence );
					encoded[curSize++] = currentByte;

					i += numInSequence - 1;
				}
				else
				{
					int numUnsequenced = GetUnsequencedLength( a_data, i );

					// Set the length of unsequences bytes, minus one because loco's RLE
					// copies the next set of bytes from [unsequenced length] + 1
					encoded[curSize++] = (byte) ( numUnsequenced - 1 );

					while ( numUnsequenced > 0 )
					{
						encoded[curSize++] = a_data[i++];
						--numUnsequenced;
					}

					--i;
				}
			}

			return encoded.Take( curSize ).ToArray();
		}

		/// <summary>
		/// Unfinished because I'm not in the mood to make a "limited distance/offset" dictionary.
		/// I'm also interested to see if loco will still treat a file as being valid if the encoding
		/// type in the header is different to what it was originally.
		/// </summary>
		/// <param name="a_data"></param>
		/// <returns></returns>
		public static byte[] LZEncode ( byte[] a_data )
		{
			byte[] encoded = new byte[a_data.Length];
			int curSize = 0;

			int maxOffset = -31;
			int maxStringLength = 0xFE;

			//Dictionary<byte[], >

			for (int i = 0; i<a_data.Length; ++i )
			{
				int byteOffset = GetOffsetOfClosest(a_data[i], encoded, i);

				if (byteOffset < 0 && byteOffset > maxOffset)
				{
					encoded[curSize++] = 0; // set prefix
					encoded[curSize++] = a_data[i];
				}
				else
				{

				}
			}

			return encoded;
		}

		private static int GetOffsetOfClosest(byte a_needle, byte[] a_haystack, int a_needleIndex)
		{
			int offset = 0;

			for (int i = a_needleIndex - 1; i >= a_haystack.Length; --i )
			{
				++offset;
				if ( a_haystack[i] == a_needle )
				{
					return offset;
				}
			}

			return 0;
		}

		private static int GetSequenceLength ( int a_value, byte[] a_data, int a_index )
		{
			int numInSequence = 0;
			for ( int i = 0; i < a_data.Length; ++i )
			{
				if ( i + a_index >= a_data.Length )
					break;

				if ( a_data[a_index + i] == a_value )
				{
					++numInSequence;
				}
				else
				{
					break;
				}
			}

			return numInSequence;
		}

		private static int GetUnsequencedLength ( byte[] a_data, int a_index )
		{
			int numUnsequenced = 1;

			byte lastByte = a_data[a_index];

			for ( int i = 1; i < a_data.Length; ++i )
			{
				if ( i + a_index >= a_data.Length )
					break;

				if ( a_data[i + a_index] != lastByte )
				{
					lastByte = a_data[i + a_index];
					++numUnsequenced;
				}
				else
				{
					--numUnsequenced;
					break;
				}
			}

			return numUnsequenced;
		}
		#endregion
	}
}
