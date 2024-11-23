using System;

namespace Plugin.WcfClient.Parser
{
	public class StringPair
	{
		internal String String1 { get; private set; }

		internal String String2 { get; private set; }

		internal StringPair(String string1, String string2)
		{
			this.String1 = string1;
			this.String2 = string2;
		}

		public override Boolean Equals(Object o)
		{
			StringPair stringPair = o as StringPair;
			return stringPair != null && stringPair.String1.Equals(this.String1) && stringPair.String2.Equals(this.String2);
		}

		public override Int32 GetHashCode()
			=> this.String1.GetHashCode() ^ this.String2.GetHashCode();
	}
}