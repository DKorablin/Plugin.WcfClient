using System;

namespace Plugin.WcfClient.Parser
{
	public class StringObjectPair
	{
		internal Object ObjectValue { get; private set; }

		internal String StringValue { get; private set; }

		internal StringObjectPair(String stringValue, Object objectValue)
		{
			this.StringValue = stringValue;
			this.ObjectValue = objectValue;
		}
		public override Boolean Equals(Object o)
		{
			StringObjectPair stringObjectPair = o as StringObjectPair;
			return stringObjectPair != null && stringObjectPair.StringValue.Equals(this.StringValue) && this.ObjectValue == stringObjectPair.ObjectValue;
		}
		public override Int32 GetHashCode()
			=> this.StringValue.GetHashCode() ^ this.ObjectValue.GetHashCode();
	}
}