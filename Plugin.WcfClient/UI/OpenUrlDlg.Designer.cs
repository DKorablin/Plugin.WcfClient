namespace Plugin.WcfClient.UI
{
	partial class OpenUrlDlg
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.Button bnCancel;
			System.Windows.Forms.Label lblUrl;
			this.txtAddress = new System.Windows.Forms.TextBox();
			this.bnOk = new System.Windows.Forms.Button();
			this.ddlType = new System.Windows.Forms.ComboBox();
			this.error = new System.Windows.Forms.ErrorProvider(this.components);
			bnCancel = new System.Windows.Forms.Button();
			lblUrl = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.error)).BeginInit();
			this.SuspendLayout();
			// 
			// bnCancel
			// 
			bnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			bnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			bnCancel.Location = new System.Drawing.Point(257, 32);
			bnCancel.Name = "bnCancel";
			bnCancel.Size = new System.Drawing.Size(75, 23);
			bnCancel.TabIndex = 5;
			bnCancel.Text = "&Cancel";
			bnCancel.UseVisualStyleBackColor = true;
			// 
			// lblUrl
			// 
			lblUrl.AutoSize = true;
			lblUrl.Location = new System.Drawing.Point(12, 9);
			lblUrl.Name = "lblUrl";
			lblUrl.Size = new System.Drawing.Size(62, 13);
			lblUrl.TabIndex = 0;
			lblUrl.Text = "&Service Url:";
			// 
			// txtAddress
			// 
			this.txtAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtAddress.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
			this.txtAddress.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.AllUrl;
			this.txtAddress.Location = new System.Drawing.Point(80, 6);
			this.txtAddress.Name = "txtAddress";
			this.txtAddress.Size = new System.Drawing.Size(186, 20);
			this.txtAddress.TabIndex = 1;
			this.txtAddress.TextChanged += new System.EventHandler(this.txtUrl_TextChanged);
			// 
			// bnOk
			// 
			this.bnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.bnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.bnOk.Enabled = false;
			this.error.SetIconAlignment(this.bnOk, System.Windows.Forms.ErrorIconAlignment.MiddleLeft);
			this.bnOk.Location = new System.Drawing.Point(176, 32);
			this.bnOk.Name = "bnOk";
			this.bnOk.Size = new System.Drawing.Size(75, 23);
			this.bnOk.TabIndex = 4;
			this.bnOk.Text = "OK";
			this.bnOk.UseVisualStyleBackColor = true;
			// 
			// ddlType
			// 
			this.ddlType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ddlType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ddlType.FormattingEnabled = true;
			this.ddlType.Location = new System.Drawing.Point(272, 6);
			this.ddlType.Name = "ddlType";
			this.ddlType.Size = new System.Drawing.Size(60, 21);
			this.ddlType.TabIndex = 3;
			// 
			// error
			// 
			this.error.ContainerControl = this;
			// 
			// OpenUrlDlg
			// 
			this.AcceptButton = this.bnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = bnCancel;
			this.ClientSize = new System.Drawing.Size(344, 62);
			this.Controls.Add(this.ddlType);
			this.Controls.Add(bnCancel);
			this.Controls.Add(this.bnOk);
			this.Controls.Add(this.txtAddress);
			this.Controls.Add(lblUrl);
			this.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(640, 100);
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(200, 100);
			this.Name = "OpenUrlDlg";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add service";
			((System.ComponentModel.ISupportInitialize)(this.error)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtAddress;
		private System.Windows.Forms.Button bnOk;
		private System.Windows.Forms.ComboBox ddlType;
		private System.Windows.Forms.ErrorProvider error;
	}
}