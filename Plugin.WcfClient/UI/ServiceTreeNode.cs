using System;
using System.Drawing;
using AlphaOmega.Windows.Forms;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient.UI
{
	internal class ServiceTreeNode : TreeNode2<ServiceTreeNode.TagExtender>
	{
		private static readonly Color ExceptionColor = SystemColors.GrayText;

		/// <summary>Tree node images</summary>
		public enum TreeImageList
		{
			/// <summary>Folder</summary>
			Folder = 0,
			Application = 1,
			/// <summary>Service</summary>
			Endpoint = 2,

			Contract = 3,
			/// <summary>Configuration file</summary>
			File = 4,
			/// <summary>Method</summary>
			Operation = 5,
			/// <summary>Error node</summary>
			Error = 6,
		}

		internal class TagExtender
		{
			public SettingsDataSet.TreeRow Row { get; set; }
			public ServiceProject Project { get; set; }
			public ClientEndpointInfo Endpoint { get; set; }
			public ServiceMethodWrapper Method { get; set; }
			public String ConfigPath { get; set; }
		}

		public new ServiceTreeNode Parent => (ServiceTreeNode)base.Parent;

		public ServiceProject Project
		{
			get => this.Tag.Project;
			private set => this.Tag.Project = value;
		}

		public ServiceMethodWrapper Method
		{
			get => this.Tag.Method;
			private set
			{
				if(this.NodeType == TreeImageList.Operation)
					this.Tag.Method = value;
				else
					throw new InvalidOperationException("Method can only be set to operation node");
			}
		}

		public ClientEndpointInfo Endpoint
		{
			get => this.Tag.Endpoint;
			private set
			{
				_ = value ?? throw new ArgumentNullException(nameof(value));

				if(this.NodeType == TreeImageList.Endpoint)//TODO: This type of node can contain either the link itself or the service type...
					this.Tag.Endpoint = value;
				else
					throw new InvalidOperationException("Invalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Get a number of tree item settings</summary>
		public SettingsDataSet.TreeRow Settings
		{
			get { return this.Tag.Row; }
			set
			{
				_ = value ?? throw new ArgumentNullException(nameof(value));

				if(this.NodeType==TreeImageList.Folder || this.NodeType == TreeImageList.Endpoint)//TODO: This type of node can contain either the link itself or the service type...
					this.Tag.Row = value;
				else
					throw new InvalidOperationException("Invalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Service configuration file</summary>
		public String ConfigPath
		{
			get { return this.Tag.ConfigPath; }
			private set
			{
				if(String.IsNullOrEmpty(value))
					throw new ArgumentNullException(nameof(value));
				else if(this.NodeType == TreeImageList.File)
					this.Tag.ConfigPath = value;
				else
					throw new InvalidOperationException("Invalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Get the node type</summary>
		public TreeImageList NodeType
		{
			get => (TreeImageList)base.ImageIndex;
			set => base.SelectedImageIndex = base.ImageIndex = (Int32)value;
		}

		/// <summary>Checking if the tree contains an error</summary>
		/// <returns>The node contains the error text</returns>
		public Boolean IsError => base.ForeColor == ServiceTreeNode.ExceptionColor;

		public ServiceTreeNode()
			=> this.Tag = new TagExtender();

		public ServiceTreeNode(String configPath)
			: this()
		{
			if(String.IsNullOrEmpty(configPath))
				throw new ArgumentNullException(nameof(configPath));

			this.Text = "Config File";
			this.NodeType = TreeImageList.File;
			this.ConfigPath = configPath;
		}

		public ServiceTreeNode(ClientEndpointInfo endpoint)
			: this()
		{
			this.NodeType = TreeImageList.Endpoint;
			this.Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
			this.Text = endpoint.EndpointName;
		}

		public ServiceTreeNode(ServiceMethodWrapper method)
			: this()
		{
			this.NodeType = TreeImageList.Operation;
			this.Method = method ?? throw new ArgumentNullException(nameof(method));
			this.Text = method.MethodName + "()";
		}

		public ServiceTreeNode(SettingsDataSet.TreeRow row)
			: this()
		{
			_ = row ?? throw new ArgumentNullException(nameof(row));
			this.Text = row.Name;

			switch(row.ElementType)
			{
			case ElementType.Tree:
				this.NodeType = TreeImageList.Folder;
				break;
			case ElementType.Client:
				this.NodeType = TreeImageList.Endpoint;
				this.Nodes.Add(new ServiceTreeNode());//Adding dummy node
				break;
			default:
				throw new NotImplementedException($"Element with type {row.ElementType} not implemented");
			}

			this.Settings = row;
		}

		/// <summary>TODO: Bugs. NodeType=Endpoint can be either a service or a link.</summary>
		/// <returns></returns>
		public Boolean IsRealEndpoint()
		=> this.NodeType == TreeImageList.Endpoint && this.Settings != null;

		/// <summary>The project is unloaded.</summary>
		/// <returns></returns>
		public Boolean IsProjectUnloaded()
		=> this.IsRealEndpoint() && this.Project == null;

		/// <summary>Unload the project from the UI.</summary>
		public void UnloadProject()
		{
			this.Project = null;
			base.Nodes.Clear();
			base.Nodes.Add(new ServiceTreeNode());
			base.Collapse();
		}

		/// <summary>Load project into UI</summary>
		/// <param name="project">Project to load into UI</param>
		public void LoadProject(ServiceProject project)
		{
			if(this.NodeType != TreeImageList.Endpoint)
				throw new InvalidOperationException("Project can be loaded only inside endpoint");

			base.Nodes.Clear();
			this.Project = project ?? throw new ArgumentNullException(nameof(project));

			ServiceType type = this.Settings.ServiceRow.ServiceType;
			foreach(ClientEndpointInfo client in project.Endpoints)
			{
				switch(type)
				{
				case ServiceType.WS:
					foreach(ServiceMethodWrapper method in client.Methods)
					{
						ServiceTreeNode nodeMethod = new ServiceTreeNode(method);
						this.Nodes.Add(nodeMethod);
					}
					break;
				case ServiceType.WCF:
					ServiceTreeNode nodeEndpoint = new ServiceTreeNode(client);
					this.Nodes.Add(nodeEndpoint);
					if(!client.Valid)
					{
						String errorMessage = client.InvalidReason
							?? "This service contract is not supported in the WCF Test Client. Refer to product documentation for a list of supported features.";
						this.SetError(null);
						this.ToolTipText = errorMessage;
					}

					foreach(ServiceMethodWrapper method in client.Methods)
					{
						ServiceTreeNode nodeMethod = new ServiceTreeNode(method);
						nodeEndpoint.Nodes.Add(nodeMethod);
						if(!client.Valid || !method.Valid)
						{
							String errorMessage = client.InvalidReason
								?? "This operation is not supported in the WCF Test Client.";
							nodeMethod.SetError(null);
							nodeMethod.ToolTipText = errorMessage;
						}
					}
					break;
				}
			}

			if(project.Info.ConfigFilePath != null)
				this.Nodes.Add(new ServiceTreeNode(project.Info.ConfigFilePath));
			/*if(!String.IsNullOrEmpty(serviceProject.ProxyPath))
			{
				TreeNode nodeProxy = new TreeNode("Proxy Code");
				nodeProxy.SetImageIndex(TreeNodeExtender.WcfTreeIcon.File);
				nodeProxy.Tag = serviceProject.ProxyPath;
				serviceProjectNode.Nodes.Add(nodeProxy);
			}*/
			base.ExpandAll();
		}

		/// <summary>Write an error to a tree node</summary>
		/// <param name="errorMessage">Error text</param>
		public void SetError(String errorMessage)
		{
			base.ForeColor = ServiceTreeNode.ExceptionColor;
			//this.NodeType = TreeImageList.Error;
			if(errorMessage != null)
				base.Text = errorMessage;
		}

		/// <summary>Recursive project search</summary>
		/// <returns></returns>
		public ServiceProject FindProject()
		{
			if(this.Project != null)
				return this.Project;
			else if(this.Parent != null)
				return this.Parent.FindProject();
			else
				return null;
		}

		/// <summary>Recursively search the tree to the root settings</summary>
		/// <param name="node">Tree node</param>
		/// <returns>Service settings</returns>
		public SettingsDataSet.TreeRow FindSettings()
		{
			ServiceTreeNode node = this;
			while(node != null)
			{
				TreeImageList imageIndex = node.NodeType;
				if(imageIndex == TreeImageList.Endpoint || imageIndex == TreeImageList.Folder)
					break;
				node = node.Parent;//I get to the root iteratively
			}

			return node?.Tag.Row;
		}
	}
}