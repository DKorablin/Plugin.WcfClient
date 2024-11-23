namespace Plugin.WcfClient
{
	partial class DocumentSvcTestMethod
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.SplitContainer splitFormatted;
			System.Windows.Forms.Label label2;
			System.Windows.Forms.Label label3;
			System.Windows.Forms.Label label4;
			System.Windows.Forms.ToolStrip tsMain;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentSvcTestMethod));
			this.lblRequest = new System.Windows.Forms.Label();
			this.gridInput = new Plugin.WcfClient.UI.VirtualTreeControl2();
			this.txtElapsed = new System.Windows.Forms.TextBox();
			this.gridOutput = new Plugin.WcfClient.UI.VirtualTreeControl2();
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tabFormatted = new System.Windows.Forms.TabPage();
			this.tabXml = new System.Windows.Forms.TabPage();
			this.splitXml = new System.Windows.Forms.SplitContainer();
			this.txtXmlInput = new System.Windows.Forms.TextBox();
			this.txtXmlOutput = new System.Windows.Forms.TextBox();
			this.bwInvokeService = new System.ComponentModel.BackgroundWorker();
			this.bwLoadService = new System.ComponentModel.BackgroundWorker();
			this.tsbnInvoke = new System.Windows.Forms.ToolStripSplitButton();
			this.tsmiProxy = new System.Windows.Forms.ToolStripMenuItem();
			this.tsbnValues = new System.Windows.Forms.ToolStripDropDownButton();
			this.error = new System.Windows.Forms.ErrorProvider(this.components);
			this.tsmiValuesLoad = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiValuesSave = new System.Windows.Forms.ToolStripMenuItem();
			splitFormatted = new System.Windows.Forms.SplitContainer();
			label2 = new System.Windows.Forms.Label();
			label3 = new System.Windows.Forms.Label();
			label4 = new System.Windows.Forms.Label();
			tsMain = new System.Windows.Forms.ToolStrip();
			splitFormatted.Panel1.SuspendLayout();
			splitFormatted.Panel2.SuspendLayout();
			splitFormatted.SuspendLayout();
			tsMain.SuspendLayout();
			this.tabMain.SuspendLayout();
			this.tabFormatted.SuspendLayout();
			this.tabXml.SuspendLayout();
			this.splitXml.Panel1.SuspendLayout();
			this.splitXml.Panel2.SuspendLayout();
			this.splitXml.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.error)).BeginInit();
			this.SuspendLayout();
			// 
			// splitFormatted
			// 
			splitFormatted.Dock = System.Windows.Forms.DockStyle.Fill;
			splitFormatted.Location = new System.Drawing.Point(3, 3);
			splitFormatted.Name = "splitFormatted";
			splitFormatted.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitFormatted.Panel1
			// 
			splitFormatted.Panel1.Controls.Add(this.lblRequest);
			splitFormatted.Panel1.Controls.Add(this.gridInput);
			// 
			// splitFormatted.Panel2
			// 
			splitFormatted.Panel2.Controls.Add(this.txtElapsed);
			splitFormatted.Panel2.Controls.Add(label2);
			splitFormatted.Panel2.Controls.Add(this.gridOutput);
			splitFormatted.Size = new System.Drawing.Size(749, 496);
			splitFormatted.SplitterDistance = 214;
			splitFormatted.TabIndex = 0;
			// 
			// lblRequest
			// 
			this.lblRequest.AutoSize = true;
			this.lblRequest.Location = new System.Drawing.Point(10, 8);
			this.lblRequest.Name = "lblRequest";
			this.lblRequest.Size = new System.Drawing.Size(50, 13);
			this.lblRequest.TabIndex = 0;
			this.lblRequest.Text = "&Request:";
			// 
			// gridInput
			// 
			this.gridInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridInput.HasGridLines = true;
			this.gridInput.HasHorizontalGridLines = true;
			this.gridInput.HasVerticalGridLines = true;
			this.gridInput.LabelEditSupport = Microsoft.VisualStudio.VirtualTreeGrid.VirtualTreeLabelEditActivationStyles.ImmediateSelection;
			this.gridInput.Location = new System.Drawing.Point(0, 26);
			this.gridInput.Name = "gridInput";
			this.gridInput.Size = new System.Drawing.Size(749, 186);
			this.gridInput.TabIndex = 1;
			// 
			// txtElapsed
			// 
			this.txtElapsed.Location = new System.Drawing.Point(74, 7);
			this.txtElapsed.Name = "txtElapsed";
			this.txtElapsed.ReadOnly = true;
			this.txtElapsed.Size = new System.Drawing.Size(75, 20);
			this.txtElapsed.TabIndex = 6;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new System.Drawing.Point(10, 10);
			label2.Name = "label2";
			label2.Size = new System.Drawing.Size(58, 13);
			label2.TabIndex = 4;
			label2.Text = "&Response:";
			// 
			// gridOutput
			// 
			this.gridOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.gridOutput.HasGridLines = true;
			this.gridOutput.HasHorizontalGridLines = true;
			this.gridOutput.HasVerticalGridLines = true;
			this.gridOutput.LabelEditSupport = Microsoft.VisualStudio.VirtualTreeGrid.VirtualTreeLabelEditActivationStyles.ImmediateSelection;
			this.gridOutput.Location = new System.Drawing.Point(0, 30);
			this.gridOutput.Name = "gridOutput";
			this.gridOutput.Size = new System.Drawing.Size(749, 248);
			this.gridOutput.TabIndex = 5;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Location = new System.Drawing.Point(10, 8);
			label3.Name = "label3";
			label3.Size = new System.Drawing.Size(50, 13);
			label3.TabIndex = 0;
			label3.Text = "&Request:";
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Location = new System.Drawing.Point(10, 10);
			label4.Name = "label4";
			label4.Size = new System.Drawing.Size(58, 13);
			label4.TabIndex = 2;
			label4.Text = "&Response:";
			// 
			// tsMain
			// 
			tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbnValues,
			this.tsbnInvoke});
			tsMain.Location = new System.Drawing.Point(0, 0);
			tsMain.Name = "tsMain";
			tsMain.Size = new System.Drawing.Size(763, 25);
			tsMain.TabIndex = 1;
			// 
			// tabMain
			// 
			this.tabMain.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabMain.Controls.Add(this.tabFormatted);
			this.tabMain.Controls.Add(this.tabXml);
			this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabMain.Location = new System.Drawing.Point(0, 25);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(763, 528);
			this.tabMain.TabIndex = 1;
			this.tabMain.SelectedIndexChanged += new System.EventHandler(this.tabMain_SelectedIndexChanged);
			// 
			// tabFormatted
			// 
			this.tabFormatted.Controls.Add(splitFormatted);
			this.tabFormatted.Location = new System.Drawing.Point(4, 4);
			this.tabFormatted.Name = "tabFormatted";
			this.tabFormatted.Padding = new System.Windows.Forms.Padding(3);
			this.tabFormatted.Size = new System.Drawing.Size(755, 502);
			this.tabFormatted.TabIndex = 0;
			this.tabFormatted.Text = "Formatted";
			this.tabFormatted.UseVisualStyleBackColor = true;
			// 
			// tabXml
			// 
			this.tabXml.Controls.Add(this.splitXml);
			this.tabXml.Location = new System.Drawing.Point(4, 4);
			this.tabXml.Name = "tabXml";
			this.tabXml.Padding = new System.Windows.Forms.Padding(3);
			this.tabXml.Size = new System.Drawing.Size(755, 502);
			this.tabXml.TabIndex = 1;
			this.tabXml.Text = "XML";
			this.tabXml.UseVisualStyleBackColor = true;
			// 
			// splitXml
			// 
			this.splitXml.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitXml.Location = new System.Drawing.Point(3, 3);
			this.splitXml.Name = "splitXml";
			this.splitXml.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitXml.Panel1
			// 
			this.splitXml.Panel1.Controls.Add(label3);
			this.splitXml.Panel1.Controls.Add(this.txtXmlInput);
			// 
			// splitXml.Panel2
			// 
			this.splitXml.Panel2.Controls.Add(label4);
			this.splitXml.Panel2.Controls.Add(this.txtXmlOutput);
			this.splitXml.Size = new System.Drawing.Size(749, 496);
			this.splitXml.SplitterDistance = 214;
			this.splitXml.TabIndex = 0;
			// 
			// txtXmlInput
			// 
			this.txtXmlInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtXmlInput.Location = new System.Drawing.Point(0, 26);
			this.txtXmlInput.Multiline = true;
			this.txtXmlInput.Name = "txtXmlInput";
			this.txtXmlInput.ReadOnly = true;
			this.txtXmlInput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtXmlInput.Size = new System.Drawing.Size(749, 186);
			this.txtXmlInput.TabIndex = 1;
			this.txtXmlInput.WordWrap = false;
			this.txtXmlInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtXml_KeyDown);
			// 
			// txtXmlOutput
			// 
			this.txtXmlOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtXmlOutput.Location = new System.Drawing.Point(0, 30);
			this.txtXmlOutput.Multiline = true;
			this.txtXmlOutput.Name = "txtXmlOutput";
			this.txtXmlOutput.ReadOnly = true;
			this.txtXmlOutput.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtXmlOutput.Size = new System.Drawing.Size(749, 248);
			this.txtXmlOutput.TabIndex = 3;
			this.txtXmlOutput.WordWrap = false;
			this.txtXmlOutput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtXml_KeyDown);
			// 
			// bwInvokeService
			// 
			this.bwInvokeService.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwInvokeService_DoWork);
			this.bwInvokeService.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwInvokeService_RunWorkerCompleted);
			// 
			// bwLoadService
			// 
			this.bwLoadService.WorkerReportsProgress = true;
			this.bwLoadService.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwLoadService_DoWork);
			this.bwLoadService.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwLoadService_RunWorkerCompleted);
			// 
			// tsbnInvoke
			// 
			this.tsbnInvoke.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnInvoke.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiProxy});
			this.tsbnInvoke.Image = global::Plugin.WcfClient.Properties.Resources.iconDebug;
			this.tsbnInvoke.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnInvoke.Name = "tsbnInvoke";
			this.tsbnInvoke.Size = new System.Drawing.Size(32, 22);
			this.tsbnInvoke.ToolTipText = "&Invoke method";
			this.tsbnInvoke.ButtonClick += new System.EventHandler(this.tsbnInvoke_Click);
			// 
			// tsmiProxy
			// 
			this.tsmiProxy.CheckOnClick = true;
			this.tsmiProxy.Name = "tsmiProxy";
			this.tsmiProxy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.tsmiProxy.Size = new System.Drawing.Size(173, 22);
			this.tsmiProxy.Text = "&New proxy";
			// 
			// tsbnValues
			// 
			this.tsbnValues.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnValues.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.tsbnValues.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiValuesLoad,
            this.tsmiValuesSave});
			this.tsbnValues.Image = global::Plugin.WcfClient.Properties.Resources.iconOpen;
			this.tsbnValues.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnValues.Name = "tsbnValues";
			this.tsbnValues.Size = new System.Drawing.Size(29, 22);
			this.tsbnValues.Text = "Values";
			this.tsbnValues.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsbnValues_DropDownItemClicked);
			// 
			// error
			// 
			this.error.ContainerControl = this;
			// 
			// tsmiValuesLoad
			// 
			this.tsmiValuesLoad.Name = "tsmiValuesLoad";
			this.tsmiValuesLoad.Size = new System.Drawing.Size(152, 22);
			this.tsmiValuesLoad.Text = "&Load";
			// 
			// tsmiValuesSave
			// 
			this.tsmiValuesSave.Name = "tsmiValuesSave";
			this.tsmiValuesSave.Size = new System.Drawing.Size(152, 22);
			this.tsmiValuesSave.Text = "&Save";
			// 
			// DocumentSvcTestMethod
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.tabMain);
			this.Controls.Add(tsMain);
			this.Name = "DocumentSvcTestMethod";
			this.Size = new System.Drawing.Size(763, 553);
			splitFormatted.Panel1.ResumeLayout(false);
			splitFormatted.Panel1.PerformLayout();
			splitFormatted.Panel2.ResumeLayout(false);
			splitFormatted.Panel2.PerformLayout();
			splitFormatted.ResumeLayout(false);
			tsMain.ResumeLayout(false);
			tsMain.PerformLayout();
			this.tabMain.ResumeLayout(false);
			this.tabFormatted.ResumeLayout(false);
			this.tabXml.ResumeLayout(false);
			this.splitXml.Panel1.ResumeLayout(false);
			this.splitXml.Panel1.PerformLayout();
			this.splitXml.Panel2.ResumeLayout(false);
			this.splitXml.Panel2.PerformLayout();
			this.splitXml.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.error)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.ComponentModel.BackgroundWorker bwInvokeService;
		private System.Windows.Forms.TabPage tabFormatted;
		private System.Windows.Forms.TabPage tabXml;
		private Plugin.WcfClient.UI.VirtualTreeControl2 gridInput;
		private Microsoft.VisualStudio.VirtualTreeGrid.VirtualTreeControl gridOutput;
		private System.Windows.Forms.TextBox txtXmlInput;
		private System.Windows.Forms.TextBox txtXmlOutput;
		private System.Windows.Forms.SplitContainer splitXml;
		private System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.ErrorProvider error;
		private System.Windows.Forms.TextBox txtElapsed;
		private System.Windows.Forms.ToolStripSplitButton tsbnInvoke;
		private System.Windows.Forms.ToolStripMenuItem tsmiProxy;
		private System.Windows.Forms.Label lblRequest;
		private System.ComponentModel.BackgroundWorker bwLoadService;
		private System.Windows.Forms.ToolStripDropDownButton tsbnValues;
		private System.Windows.Forms.ToolStripMenuItem tsmiValuesLoad;
		private System.Windows.Forms.ToolStripMenuItem tsmiValuesSave;
	}
}