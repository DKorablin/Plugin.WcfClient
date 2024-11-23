using System;
using System.ComponentModel;
using System.Windows.Forms;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.UI
{
	public partial class OpenUrlDlg : Form
	{
		/// <summary>Uri адрес сервиса</summary>
		internal String Address
		{
			get => txtAddress.Text;
			set => txtAddress.Text = value;
		}

		/// <summary>Тип сервиса</summary>
		internal ServiceType Type
		{
			get => (ServiceType)ddlType.SelectedItem;
			set => ddlType.SelectedItem = value;
		}

		public OpenUrlDlg()
		{
			InitializeComponent();
			ddlType.DataSource = Enum.GetValues(typeof(ServiceType));
			ddlType.SelectedItem = ServiceType.WCF;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if(base.DialogResult == DialogResult.OK)
			{
				Boolean cancel = false;
				if(txtAddress.Text.Trim().Length == 0)
				{
					error.SetError(bnOk, String.Format("URL '{0}' is not a valid URL for {1} Service.", txtAddress.Text, this.Type));
					cancel = true;
				}
				e.Cancel = cancel;
			}
			base.OnClosing(e);
		}

		private void txtUrl_TextChanged(Object sender, EventArgs e)
		{
			bnOk.Enabled = this.Address.Length > 0;
			if(this.Address.Contains(".asmx"))
				this.Type = ServiceType.WS;
			else if(this.Address.Contains(".svc"))
				this.Type = ServiceType.WCF;
		}
	}
}