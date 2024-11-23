using System;

namespace Plugin.WcfClient.Parser
{
	internal class RowCol
	{
		public Int32 Col { get; private set; }

		public Int32 Row { get; private set; }

		public RowCol(Int32 row, Int32 col)
		{
			this.Row = row;
			this.Col = col;
		}
	}
}