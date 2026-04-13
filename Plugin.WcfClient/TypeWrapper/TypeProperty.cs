using System;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class TypeProperty
	{
		public Boolean IsArray { get; set; }
		public Boolean IsCollection { get; set; }
		public Boolean IsComposite { get; set; }
		public Boolean IsDictionary { get; set; }
		public Boolean IsKeyValuePair { get; set; }
		public Boolean IsNullable { get; set; }
		public Boolean IsStruct { get; set; }
	}
}