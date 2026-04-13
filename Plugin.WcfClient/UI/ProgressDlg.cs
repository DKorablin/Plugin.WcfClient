using System;
using System.ComponentModel;
using System.Windows.Forms;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient.UI
{
	internal partial class ProgressDlg : Form
	{
		private readonly TimeSpan _timeout;

		public ProgressDlg(String title, String labelText, TimeSpan timeout, BackgroundWorker backgroundWorker)
		{
			this.InitializeComponent();

			this._timeout = timeout;
			this.Text = title;
			this.actionLabel.Text = labelText;
			this.backgroundWorker = backgroundWorker;
			this.backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
		}

		public static Boolean Prompt(AddServiceInputs.ActionType action, String labelText, TimeSpan timeout, BackgroundWorker backgroundWorker)
		{
			String title;
			switch(action)
			{
			case AddServiceInputs.ActionType.Download:
				title = "Downloading...";
				break;
			case AddServiceInputs.ActionType.Open:
				title = "Opening...";
				break;
			default:
				throw new NotImplementedException(action.ToString());
			}

			using(ProgressDlg progressDialog = new ProgressDlg(title, labelText, new TimeSpan(0, 0, 0, 20), backgroundWorker))
				return progressDialog.ShowDialog() == DialogResult.OK;
		}

		private void backgroundWorker_ProgressChanged(Object sender, ProgressChangedEventArgs e)
		{
			this.progressBar.Value = e.ProgressPercentage;
			if(this.progressBar.Value == this.progressBar.Maximum)
			{
				base.DialogResult = DialogResult.OK;
				base.Close();
			}
		}
	}
}