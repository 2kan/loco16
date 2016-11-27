using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace loco16
{
	public class Loco
	{
		private ChunkMan m_chunkMan;

		public Loco ()
		{
			m_chunkMan = new ChunkMan();
		}

		public bool ReadFile ( string a_filename )
		{
			//var original = new byte[] { 0x57, 0x65, 0x65, 0x65, 0x65, 0x20, 0x48, 0x61, 0x61, 0x61, 0x21 };
			//var decoded = Decoder.LZDecode( new byte[] { 0xFF, 0x57, 0xFF, 0x65, 0xFA, 0xFF, 0x20, 0xFF, 0x48, 0xFF, 0x61, 0xF9, 0xFF, 0x21 } );

			//var a = Codec.RunLengthEncode( new byte[] { 0x20, 0x20, 0x20, 0x20, 0x30, 0x30, 0x01, 0x02, 0x03, 0x04, 0x05, 0x05, 0x05, 0x20, 0x20, 0x20, 0x20, 0x30, 0x30, 0x01, 0x02, 0x03, 0x04 } );
			//var q = Codec.RunLengthDecode( a );

			byte[] encoded = System.IO.File.ReadAllBytes( a_filename );

			//uint zchecksum = 0;
			//for (int i =0; i<encoded.Length; ++i )
			//{
			//	zchecksum += encoded[i];
			//}

			m_chunkMan.SplitChunks( encoded );
			m_chunkMan.DecodeChunks();
			uint checksum = m_chunkMan.CalculateChecksum();
			//m_chunkMan.WriteFormattedChunksToFile();
			string[] split = a_filename.Split( '\\' );
			byte[] origChecksum = encoded.Skip( encoded.Length - 4 ).ToArray();
			m_chunkMan.WriteUncompressedToFile(split[split.Length - 1], origChecksum);


			return true;
		}

		private byte[] GetGameData ( byte[] a_fileContents )
		{
			byte[] data = new byte[32 * 36776];

			for ( int i = 47468; i < 47468 + 32 * 36776; ++i )
			{
				data[data.Length] = a_fileContents[i];
			}

			return data;
		}
	}
}
