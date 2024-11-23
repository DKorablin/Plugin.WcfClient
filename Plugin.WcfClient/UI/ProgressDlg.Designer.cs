namespace Plugin.WcfClient.UI
{
	partial class ProgressDlg
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.cancelButton = new System.Windows.Forms.Button();
			this.actionLabel = new System.Windows.Forms.Label();
			this.lblTimeout = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(12, 10);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(380, 20);
			this.progressBar.TabIndex = 0;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(317, 39);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// actionLabel
			// 
			this.actionLabel.Location = new System.Drawing.Point(12, 8);
			this.actionLabel.Name = "actionLabel";
			this.actionLabel.Size = new System.Drawing.Size(0, 13);
			this.actionLabel.TabIndex = 2;
			this.actionLabel.Visible = false;
			// 
			// lblTimeout
			// 
			this.lblTimeout.AutoSize = true;
			this.lblTimeout.Location = new System.Drawing.Point(12, 44);
			this.lblTimeout.Name = "lblTimeout";
			this.lblTimeout.Size = new System.Drawing.Size(43, 13);
			this.lblTimeout.TabIndex = 3;
			this.lblTimeout.Text = "0:00:20";
			// 
			// ProgressDlg
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(404, 70);
			this.Controls.Add(this.lblTimeout);
			this.Controls.Add(this.actionLabel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.progressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ProgressDlg";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
		private System.ComponentModel.BackgroundWorker backgroundWorker;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Label actionLabel;
		private System.Windows.Forms.Label lblTimeout;
	}
}