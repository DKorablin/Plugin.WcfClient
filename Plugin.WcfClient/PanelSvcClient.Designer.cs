namespace Plugin.WcfClient
{
	partial class PanelSvcTestClient
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
			System.Windows.Forms.ToolStrip tsMain;
			System.Windows.Forms.ImageList ilServiceItem;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PanelSvcTestClient));
			this.tsbnAdd = new System.Windows.Forms.ToolStripSplitButton();
			this.tsmiAddClient = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiAddNode = new System.Windows.Forms.ToolStripMenuItem();
			this.tsbnDelete = new System.Windows.Forms.ToolStripButton();
			this.tvService = new Plugin.WcfClient.UI.ServiceTreeView();
			this.cmsService = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.tsmiServiceOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceUpdate = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceBrowse = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsmiServiceCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceCopyPath = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceCopyUrl = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceRemove = new System.Windows.Forms.ToolStripMenuItem();
			this.tsmiServiceProperties = new System.Windows.Forms.ToolStripMenuItem();
			this.gridSearch = new AlphaOmega.Windows.Forms.SearchGrid();
			this.bgAddService = new System.ComponentModel.BackgroundWorker();
			this.pgSettings = new System.Windows.Forms.PropertyGrid();
			this.splitMain = new System.Windows.Forms.SplitContainer();
			tsMain = new System.Windows.Forms.ToolStrip();
			ilServiceItem = new System.Windows.Forms.ImageList(this.components);
			tsMain.SuspendLayout();
			this.cmsService.SuspendLayout();
			this.splitMain.Panel1.SuspendLayout();
			this.splitMain.Panel2.SuspendLayout();
			this.splitMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// tsMain
			// 
			tsMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			tsMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbnAdd,
            this.tsbnDelete});
			tsMain.Location = new System.Drawing.Point(0, 0);
			tsMain.Name = "tsMain";
			tsMain.Size = new System.Drawing.Size(177, 25);
			tsMain.TabIndex = 0;
			// 
			// tsbnAdd
			// 
			this.tsbnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnAdd.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiAddClient,
            this.tsmiAddNode});
			this.tsbnAdd.Image = global::Plugin.WcfClient.Properties.Resources.iconOpen;
			this.tsbnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnAdd.Name = "tsbnAdd";
			this.tsbnAdd.Size = new System.Drawing.Size(32, 22);
			this.tsbnAdd.ToolTipText = "Add Service...";
			this.tsbnAdd.ButtonClick += new System.EventHandler(this.tsbnAdd_Click);
			this.tsbnAdd.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsbnAdd_DropDownItemClicked);
			// 
			// tsmiAddClient
			// 
			this.tsmiAddClient.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.tsmiAddClient.Name = "tsmiAddClient";
			this.tsmiAddClient.Size = new System.Drawing.Size(131, 22);
			this.tsmiAddClient.Text = "Add &Client";
			// 
			// tsmiAddNode
			// 
			this.tsmiAddNode.Name = "tsmiAddNode";
			this.tsmiAddNode.Size = new System.Drawing.Size(131, 22);
			this.tsmiAddNode.Text = "Add &Node";
			// 
			// tsbnDelete
			// 
			this.tsbnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbnDelete.Enabled = false;
			this.tsbnDelete.Image = global::Plugin.WcfClient.Properties.Resources.iconDelete;
			this.tsbnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbnDelete.Name = "tsbnDelete";
			this.tsbnDelete.Size = new System.Drawing.Size(23, 22);
			this.tsbnDelete.ToolTipText = "Remove selected node";
			this.tsbnDelete.Click += new System.EventHandler(this.tsbnDelete_Click);
			// 
			// ilServiceItem
			// 
			ilServiceItem.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilServiceItem.ImageStream")));
			ilServiceItem.TransparentColor = System.Drawing.Color.Magenta;
			ilServiceItem.Images.SetKeyName(0, "i.Folder.bmp");
			ilServiceItem.Images.SetKeyName(1, "WCF.ApplicationIcon.ico");
			ilServiceItem.Images.SetKeyName(2, "WCF.Endpoint.bmp");
			ilServiceItem.Images.SetKeyName(3, "WCF.Contract.bmp");
			ilServiceItem.Images.SetKeyName(4, "WCF.File.bmp");
			ilServiceItem.Images.SetKeyName(5, "WCF.Operation.bmp");
			ilServiceItem.Images.SetKeyName(6, "WCF.Error.bmp");
			// 
			// tvService
			// 
			this.tvService.AllowDrop = true;
			this.tvService.ContextMenuStrip = this.cmsService;
			this.tvService.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvService.HideSelection = false;
			this.tvService.ImageIndex = 0;
			this.tvService.ImageList = ilServiceItem;
			this.tvService.LabelEdit = true;
			this.tvService.Location = new System.Drawing.Point(0, 0);
			this.tvService.Name = "tvService";
			this.tvService.SelectedImageIndex = 0;
			this.tvService.SelectedNode = null;
			this.tvService.Size = new System.Drawing.Size(177, 174);
			this.tvService.TabIndex = 1;
			this.tvService.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvService_AfterLabelEdit);
			this.tvService.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvService_BeforeExpand);
			this.tvService.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvService_AfterSelect);
			this.tvService.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvService_KeyDown);
			this.tvService.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tvService_MouseClick);
			this.tvService.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tvService_MouseDoubleClick);
			// 
			// cmsService
			// 
			this.cmsService.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiServiceOpen,
            this.tsmiServiceUpdate,
            this.tsmiServiceBrowse,
            this.toolStripSeparator1,
            this.tsmiServiceCopy,
            this.tsmiServiceRemove,
            this.tsmiServiceProperties});
			this.cmsService.Name = "cmsService";
			this.cmsService.Size = new System.Drawing.Size(128, 142);
			this.cmsService.Opening += new System.ComponentModel.CancelEventHandler(this.cmsService_Opening);
			this.cmsService.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.cmsService_ItemClicked);
			// 
			// tsmiServiceOpen
			// 
			this.tsmiServiceOpen.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
			this.tsmiServiceOpen.Name = "tsmiServiceOpen";
			this.tsmiServiceOpen.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceOpen.Text = "&Open";
			// 
			// tsmiServiceUpdate
			// 
			this.tsmiServiceUpdate.Name = "tsmiServiceUpdate";
			this.tsmiServiceUpdate.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceUpdate.Text = "&Update";
			// 
			// tsmiServiceBrowse
			// 
			this.tsmiServiceBrowse.Name = "tsmiServiceBrowse";
			this.tsmiServiceBrowse.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceBrowse.Text = "&Browse";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(124, 6);
			// 
			// tsmiServiceCopy
			// 
			this.tsmiServiceCopy.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiServiceCopyPath,
            this.tsmiServiceCopyUrl});
			this.tsmiServiceCopy.Name = "tsmiServiceCopy";
			this.tsmiServiceCopy.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceCopy.Text = "Copy";
			this.tsmiServiceCopy.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.tsmiServiceCopy_DropDownItemClicked);
			// 
			// tsmiServiceCopyPath
			// 
			this.tsmiServiceCopyPath.Name = "tsmiServiceCopyPath";
			this.tsmiServiceCopyPath.Size = new System.Drawing.Size(98, 22);
			this.tsmiServiceCopyPath.Text = "&Path";
			// 
			// tsmiServiceCopyUrl
			// 
			this.tsmiServiceCopyUrl.Name = "tsmiServiceCopyUrl";
			this.tsmiServiceCopyUrl.Size = new System.Drawing.Size(98, 22);
			this.tsmiServiceCopyUrl.Text = "&Url";
			// 
			// tsmiServiceRemove
			// 
			this.tsmiServiceRemove.Name = "tsmiServiceRemove";
			this.tsmiServiceRemove.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceRemove.Text = "&Remove";
			// 
			// tsmiServiceProperties
			// 
			this.tsmiServiceProperties.Name = "tsmiServiceProperties";
			this.tsmiServiceProperties.Size = new System.Drawing.Size(127, 22);
			this.tsmiServiceProperties.Text = "&Properties";
			// 
			// gridSearch
			// 
			this.gridSearch.DataGrid = null;
			this.gridSearch.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gridSearch.EnableFindCase = true;
			this.gridSearch.EnableFindHilight = true;
			this.gridSearch.EnableFindPrevNext = true;
			this.gridSearch.EnableSearchHilight = false;
			this.gridSearch.ListView = null;
			this.gridSearch.Location = new System.Drawing.Point(3, 3);
			this.gridSearch.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.gridSearch.Name = "gridSearch";
			this.gridSearch.Size = new System.Drawing.Size(440, 29);
			this.gridSearch.TabIndex = 1;
			this.gridSearch.TreeView = null;
			this.gridSearch.Visible = false;
			// 
			// bgAddService
			// 
			this.bgAddService.WorkerReportsProgress = true;
			this.bgAddService.WorkerSupportsCancellation = true;
			this.bgAddService.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwAddService_DoWork);
			this.bgAddService.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwAddService_RunWorkerCompleted);
			// 
			// pgSettings
			// 
			this.pgSettings.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pgSettings.Location = new System.Drawing.Point(0, 0);
			this.pgSettings.Name = "pgSettings";
			this.pgSettings.Size = new System.Drawing.Size(150, 46);
			this.pgSettings.TabIndex = 0;
			// 
			// splitMain
			// 
			this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitMain.Location = new System.Drawing.Point(0, 25);
			this.splitMain.Name = "splitMain";
			this.splitMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitMain.Panel1
			// 
			this.splitMain.Panel1.Controls.Add(this.gridSearch);
			this.splitMain.Panel1.Controls.Add(this.tvService);
			// 
			// splitMain.Panel2
			// 
			this.splitMain.Panel2.Controls.Add(this.pgSettings);
			this.splitMain.Panel2Collapsed = true;
			this.splitMain.Size = new System.Drawing.Size(177, 174);
			this.splitMain.SplitterDistance = 87;
			this.splitMain.TabIndex = 2;
			this.splitMain.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.splitMain_MouseDoubleClick);
			// 
			// PanelSvcTestClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitMain);
			this.Controls.Add(tsMain);
			this.Name = "PanelSvcTestClient";
			this.Size = new System.Drawing.Size(177, 199);
			tsMain.ResumeLayout(false);
			tsMain.PerformLayout();
			this.cmsService.ResumeLayout(false);
			this.splitMain.Panel1.ResumeLayout(false);
			this.splitMain.Panel2.ResumeLayout(false);
			this.splitMain.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private Plugin.WcfClient.UI.ServiceTreeView tvService;
		private System.ComponentModel.BackgroundWorker bgAddService;
		private System.Windows.Forms.ContextMenuStrip cmsService;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceCopyPath;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceCopy;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceRemove;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceUpdate;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceBrowse;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceCopyUrl;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceOpen;
		private AlphaOmega.Windows.Forms.SearchGrid gridSearch;
		private System.Windows.Forms.ToolStripSplitButton tsbnAdd;
		private System.Windows.Forms.ToolStripMenuItem tsmiAddClient;
		private System.Windows.Forms.ToolStripMenuItem tsmiAddNode;
		private System.Windows.Forms.ToolStripButton tsbnDelete;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem tsmiServiceProperties;
		private System.Windows.Forms.PropertyGrid pgSettings;
		private System.Windows.Forms.SplitContainer splitMain;
	}
}
