using System;
using System.Globalization;
using System.Windows.Forms;

namespace Plugin.WcfClient.UI.WCF
{
	public static class RtlAwareMessageBox
	{
		public static DialogResult Show(String text, String caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
			=> RtlAwareMessageBox.Show(null, text, caption, buttons, icon, defaultButton, options);

		public static DialogResult Show(IWin32Window owner, String text, String caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			if(RtlAwareMessageBox.IsRightToLeft(owner))
				options |= (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);

			return MessageBox.Show(owner, text, caption, buttons, icon, defaultButton, options);
		}

		private static Boolean IsRightToLeft(IWin32Window owner)
		{
			Control control = owner as Control;
			return control == null
				? CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
				: control.RightToLeft == RightToLeft.Yes;
		}
	}
}