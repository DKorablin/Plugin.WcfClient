using System;
using System.Windows.Forms;
using Microsoft.VisualStudio.VirtualTreeGrid;

namespace Plugin.WcfClient.UI
{
	internal class VirtualTreeControl2 : VirtualTreeControl
	{
		protected override Boolean DisplayException(Exception exception)
		{
			/* Microsoft.VisualStudio.VirtualTreeGrid.VirtualTreeControl:
			 * private void OnWmGetObject(ref Message message)
				int accessibleObjectId = (int)message.LParam; <- OverflowException
			 */
			return false;//In any case, we throw the exception above
			//return base.DisplayException(exception);
		}

		protected override void WndProc(ref Message m)
		{
			if(m.Msg == 61 && m.LParam.ToInt64() > Int32.MaxValue)
				return;//HACK: In the base build, LParam (IntPtr) is cast to Int32, but in x64 it may not fit there.

			base.WndProc(ref m);
		}
	}
}