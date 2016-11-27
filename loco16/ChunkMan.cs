using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loco16
{
	public class ChunkMan
	{
		private List<Chunk> m_chunks;

		public ChunkMan ()
		{
			m_chunks = new List<Chunk>();
		}

		/// <summary>
		/// Separates each chunk from raw save data.
		/// </summary>
		/// <param name="a_encodedData">Raw save data</param>
		public void SplitChunks ( byte[] a_encodedData )
		{
			// Last 4 bytes are a checksum, so don't bother reading over them
			for ( int i = 0; i < a_encodedData.Length - 4; )
			{

				// Header is first 5 bytes of chunk
				byte[] header = a_encodedData.Skip( i ).Take( 5 ).ToArray();
				// Get chunk size
				int chunkSize = GetChunkSize( a_encodedData.Skip( i ).Take( 5 ).ToArray() );

				// Finds the data of the chunk.
				// Skips header and gets the rest of the data in the chunk
				byte[] chunkData = a_encodedData.Skip( i + header.Length ).Take( chunkSize ).ToArray();

				m_chunks.Add( new Chunk( header, chunkData ) );

				// Skip ahead to next chunk
				i += header.Length + chunkSize;
			}
		}

		/// <summary>
		/// Decodes all chunks.
		/// </summary>
		public void DecodeChunks ()
		{
			for ( int i = 0; i < m_chunks.Count; ++i )
			{
				//try
				{
					m_chunks[i].Decode();
				}
				//catch ( IndexOutOfRangeException e )
				{
					//	Console.WriteLine( "Could not decode chunk " + i );
				}
			}
		}

		/// <summary>
		/// Returns specified chunk.
		/// </summary>
		/// <param name="a_chunkId">Chunk number to get</param>
		/// <returns>Specified chunk</returns>
		public Chunk GetChunk ( int a_chunkId )
		{
			return m_chunks[a_chunkId];
		}

		public void WriteFormattedChunksToFile ()
		{
			string filedir = "savedgame";
			// Create the directory if it doesn't already exist
			System.IO.Directory.CreateDirectory( filedir );

			System.IO.FileStream fs = null;
			for ( int i = 0; i < m_chunks.Count; ++i )
			{
				byte[] header = m_chunks[i].GetHeader();
				byte[] data = m_chunks[i].GetData();

				fs = new System.IO.FileStream( filedir + "\\chunk" + i + ".header", System.IO.FileMode.Create, System.IO.FileAccess.Write );
				fs.Write( header, 0, header.Length );
				fs.Close();

				fs = new System.IO.FileStream( filedir + "\\chunk" + i + ".data", System.IO.FileMode.Create, System.IO.FileAccess.Write );
				fs.Write( data, 0, data.Length );
				fs.Close();
			}
		}

		public void WriteUncompressedToFile ( string a_filename, byte[] a_checksum )
		{
			System.IO.FileStream fs = new System.IO.FileStream( a_filename, System.IO.FileMode.Append, System.IO.FileAccess.Write );
			//System.IO.StreamWriter a = new System.IO.StreamWriter( a_filename, true );
			int dataOffset = 0;

			for ( int i = 0; i < m_chunks.Count; ++i )
			{
				byte[] header = m_chunks[i].GetHeader();
				byte[] data = m_chunks[i].GetData();

				header[0] = 0x0; // Set the encoding type to unencoded data

				fs.Write( header, dataOffset, header.Length );
				dataOffset += header.Length;

				fs.Write( data, dataOffset, data.Length );
				dataOffset += data.Length;
			}

			fs.Write( a_checksum, 0, a_checksum.Length );

			fs.Close();
		}

		public uint CalculateChecksum ()
		{
			uint checksum = 0x0;
			for ( int i = 0; i < m_chunks.Count; ++i )
			{
				byte[] header = m_chunks[i].GetHeader();
				for ( int k = 0; k < header.Length; ++k )
					checksum += header[k];

				byte[] data = m_chunks[i].GetData();
				for ( int k = 0; k < data.Length; ++k )
					checksum += data[k];
			}

			return checksum;
		}

		private int GetChunkSize ( byte[] a_header )
		{
			// Convert the last 4 bytes of the header to an integer
			// Header is 1 byte encoding type, 4 bytes chunk size
			return BitConverter.ToInt32( a_header.Skip( 1 ).Take( 4 ).ToArray(), 0 );
		}
	}
}
