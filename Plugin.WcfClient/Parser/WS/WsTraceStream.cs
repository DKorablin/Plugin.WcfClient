using System;
using System.IO;

namespace Plugin.WcfClient.Parser.Ws
{
	/// <summary>Special switchable stream</summary>
	internal class WsTraceStream : Stream
	{
		private readonly Stream _outerStream;

		/// <summary>A link to the active inner stream</summary>
		public Stream InnerStream { get; private set; }

		/// <summary>Returns <c>true</c> if the active inner stream is a new stream, i.e. <see cref="SwitchToNewStream"/> has been called</summary>
		public Boolean IsNewStream => this.InnerStream != this._outerStream;

		/// <summary>Constructs an instance of the stream wrapping the original stream into it</summary>
		internal WsTraceStream(Stream originalStream)
			=> this.InnerStream = this._outerStream = originalStream;

		/// <summary>Creates a new memory stream and makes it active </summary>
		public void SwitchToNewStream()
			=> this.InnerStream = new MemoryStream();

		/// <summary>Copies data from the old stream to the new in-memory stream </summary>
		public void CopyOldToNew()
		{
			//innerStream = new MemoryStream((int)originalStream.Length);
			WsTraceStream.Copy(this._outerStream, this.InnerStream);
			this.InnerStream.Position = 0;
		}

		/// <summary>Copies data from the new stream to the old stream</summary>
		public void CopyNewToOld()
			=> WsTraceStream.Copy(this.InnerStream, this._outerStream);

		private static void Copy(Stream from, Stream to)
		{
			const Int32 size = 4096;
			Byte[] bytes = new Byte[4096];
			Int32 numBytes;
			while((numBytes = from.Read(bytes, 0, size)) > 0)
				to.Write(bytes, 0, numBytes);
		}

		public override IAsyncResult BeginRead(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
			=> this.InnerStream.BeginRead(buffer, offset, count, callback, state);

		public override IAsyncResult BeginWrite(Byte[] buffer, Int32 offset, Int32 count, AsyncCallback callback, Object state)
			=> this.InnerStream.BeginWrite(buffer, offset, count, callback, state);

		protected override void Dispose(Boolean disposing)
		{
			try
			{
				if(disposing)
					this.InnerStream.Close();
			} finally
			{
				base.Dispose(disposing);
			}
		}

		public override Int32 EndRead(IAsyncResult asyncResult)
			=> this.InnerStream.EndRead(asyncResult);

		public override void EndWrite(IAsyncResult asyncResult)
			=> this.InnerStream.EndWrite(asyncResult);

		public override void Flush()
			=> this.InnerStream.Flush();

		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
			=> this.InnerStream.Read(buffer, offset, count);

		public override Int32 ReadByte()
			=> this.InnerStream.ReadByte();

		public override Int64 Seek(Int64 offset, SeekOrigin origin)
			=> this.InnerStream.Seek(offset, origin);

		public override void SetLength(Int64 value)
			=> this.InnerStream.SetLength(value);

		public override void Write(Byte[] buffer, Int32 offset, Int32 count)
			=> this.InnerStream.Write(buffer, offset, count);

		public override void WriteByte(Byte value)
			=> this.InnerStream.WriteByte(value);

		// Properties
		public override Boolean CanRead => this.InnerStream.CanRead;
		public override Boolean CanSeek => this.InnerStream.CanSeek;
		public override Boolean CanWrite => this.InnerStream.CanWrite;
		public override Int64 Length => this.InnerStream.Length;
		public override Int64 Position
		{
			get => this.InnerStream.Position;
			set => this.InnerStream.Position = value;
		}
	}
}