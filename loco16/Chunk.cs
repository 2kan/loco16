using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loco16
{
	public class Chunk
	{
		private byte[] m_header;
		private byte[] m_chunkData;
		private byte[] m_decodedData;

		public Chunk ( byte[] a_header, byte[] a_chunkData )
		{
			m_header = a_header;
			m_chunkData = a_chunkData;
		}

		public void Decode ()
		{
			if ( m_header[0] == 0x00 )
			{
				m_decodedData = Codec.CopyBytes( m_chunkData );
			}
			else if ( m_header[0] == 0x01 )
			{
				m_decodedData = Codec.RunLengthDecode( m_chunkData );
			}
			else if ( m_header[0] == 0x02 )
			{
				m_decodedData = Codec.RunLengthDecode( m_chunkData );
				m_decodedData = Codec.LZDecode( m_decodedData );
			}
			else if ( m_header[0] == 0x03 )
			{
				m_decodedData = Codec.Unrotate( m_chunkData );
			}
		}

		public byte[] GetHeader()
		{
			return m_header;
		}

		public byte[] GetData()
		{
			return m_decodedData;
		}
	}
}
