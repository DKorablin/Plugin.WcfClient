using System;
using System.IO;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;

namespace Plugin.WcfClient.Parser.Ws
{
	internal class WsResponseXmlInterceptor : SoapExtension
	{
		private WsTraceStream _traceStream;

		/// <summary>You just need to extract the GET XML from the stream. Then exit.</summary>
		public static Boolean IsExtractingXml { get; set; }

		public static String XmlRequest { get; private set; }
		public static String XmlResponse { get; private set; }

		public override Stream ChainStream(Stream stream)
		{
			this._traceStream = new WsTraceStream(stream);
			return this._traceStream;
		}

		public override Object GetInitializer(Type serviceType)
			=> null;

		public override Object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
			=> null;

		public override void Initialize(Object initializer)
		{
			WsResponseXmlInterceptor.XmlRequest = null;
			WsResponseXmlInterceptor.XmlResponse = null;
		}

		public override void ProcessMessage(SoapMessage message)
		{
			switch(message.Stage)
			{
			case SoapMessageStage.BeforeSerialize:
				this._traceStream.SwitchToNewStream();
				break;
			case SoapMessageStage.AfterSerialize:
				if(this._traceStream.IsNewStream)
				{
					this._traceStream.Position = 0;
					WsResponseXmlInterceptor.XmlRequest = this.ReadXml();

					if(WsResponseXmlInterceptor.IsExtractingXml)
						throw new ServiceExecutor.StopInvocationException(WsResponseXmlInterceptor.XmlRequest);
					else
					{
						this._traceStream.Position = 0;
						this._traceStream.CopyNewToOld();
					}
				}
				break;
			case SoapMessageStage.BeforeDeserialize:
				this._traceStream.SwitchToNewStream();
				this._traceStream.CopyOldToNew();
				WsResponseXmlInterceptor.XmlResponse = this.ReadXml();
				this._traceStream.Position = 0;
				break;
			}
		}

		private String ReadXml()
		{
			XmlTextReader reader = new XmlTextReader(this._traceStream.InnerStream);
			using(StringWriter writer = new StringWriter(new StringBuilder((Int32)(this._traceStream.InnerStream.Length * 1.51))))
			{
				XmlTextWriter xmlWriter = new XmlTextWriter(writer)
				{
					Formatting = Formatting.Indented,
					IndentChar = '\t',
					Indentation = 1
				};
				reader.MoveToContent();
				xmlWriter.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
				while(!reader.EOF)
					if(reader.NodeType != XmlNodeType.XmlDeclaration)
						xmlWriter.WriteNode(reader, false);

				return writer.ToString();
			}
		}
	}
}