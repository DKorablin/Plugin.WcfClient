using System;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient
{
	public class DocumentSvcTestMethodSettings
	{
		public Int32? TreeId { get; set; }
		public String EndpointName { get; set; }
		public String MethodName { get; set; }
		public VariableWrapper[] Variables { get; set; }
	}
}