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
			return false;//В любом случае бросаем исключение выше
			//return base.DisplayException(exception);
		}

		protected override void WndProc(ref Message m)
		{
			if(m.Msg == 61 && m.LParam.ToInt64() > Int32.MaxValue)
				return;//HACK: В базовой сборке LParam (IntPtr) приводится к Int32, а в x64 он туда может не влезть

			base.WndProc(ref m);
		}
	}
}