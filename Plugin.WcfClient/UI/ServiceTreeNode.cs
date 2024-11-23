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

		/// <summary>Изображения узлов дерева</summary>
		public enum TreeImageList
		{
			/// <summary>Папка</summary>
			Folder = 0,
			Application = 1,
			/// <summary>Сервис</summary>
			Endpoint = 2,

			Contract = 3,
			/// <summary>Конфигурационный файл</summary>
			File = 4,
			/// <summary>Метод</summary>
			Operation = 5,
			/// <summary>Узел с ошибкой</summary>
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

				if(this.NodeType == TreeImageList.Endpoint)//TODO: В этом типе нода может быть как сама ссылка, так и тип сервиса...
					this.Tag.Endpoint = value;
				else
					throw new InvalidOperationException("Ivalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Получить ряд настроек элемента дерева</summary>
		public SettingsDataSet.TreeRow Settings
		{
			get { return this.Tag.Row; }
			set
			{
				_ = value ?? throw new ArgumentNullException(nameof(value));

				if(this.NodeType==TreeImageList.Folder || this.NodeType == TreeImageList.Endpoint)//TODO: В этом типе нода может быть как сама ссылка, так и тип сервиса...
					this.Tag.Row = value;
				else
					throw new InvalidOperationException("Invalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Конфигурационный файл сервиса</summary>
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
					throw new InvalidOperationException("Ivalid node type " + this.NodeType.ToString());
			}
		}

		/// <summary>Получить тип узла</summary>
		public TreeImageList NodeType
		{
			get => (TreeImageList)base.ImageIndex;
			set => base.SelectedImageIndex = base.ImageIndex = (Int32)value;
		}

		/// <summary>Проверка на содержание ошибки в дереве</summary>
		/// <returns>В узле содержится текст ошибки</returns>
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
				throw new NotImplementedException(String.Format("Element with type {0} not implemented", row.ElementType));
			}

			this.Settings = row;
		}

		/// <summary>TODO: Косяк. В NodeType=Endpoint может быть как сервис, так и ссылка</summary>
		/// <returns></returns>
		public Boolean IsRealEndpoint()
			=> this.NodeType == TreeImageList.Endpoint && this.Settings != null;

		/// <summary>Проект находится в выгруженно сосотоянии</summary>
		/// <returns></returns>
		public Boolean IsProjectUnloaded()
			=> this.IsRealEndpoint() && this.Project == null;

		/// <summary>Выгрузить впроекта из UI</summary>
		public void UnloadProject()
		{
			this.Project = null;
			base.Nodes.Clear();
			base.Nodes.Add(new ServiceTreeNode());
			base.Collapse();
		}

		/// <summary>Загрузить проект в UI</summary>
		/// <param name="project">Проект для загрузки в UI</param>
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
						String errorMessage = client.InvalidReason == null
							? "This service contract is not supported in the WCF Test Client. Refer to product documentation for a list of supported features."
							: client.InvalidReason;
						this.SetError(null);
						this.ToolTipText = errorMessage;
					}

					foreach(ServiceMethodWrapper method in client.Methods)
					{
						ServiceTreeNode nodeMethod = new ServiceTreeNode(method);
						nodeEndpoint.Nodes.Add(nodeMethod);
						if(!client.Valid || !method.Valid)
						{
							String errorMessage = client.InvalidReason == null
								? "This operation is not supported in the WCF Test Client."
								: client.InvalidReason;
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

		/// <summary>Написать ошибку на узел дерева</summary>
		/// <param name="errorMessage">Текст ошибки</param>
		public void SetError(String errorMessage)
		{
			base.ForeColor = ServiceTreeNode.ExceptionColor;
			//this.NodeType = TreeImageList.Error;
			if(errorMessage != null)
				base.Text = errorMessage;
		}

		/// <summary>Рекурсивный поиск проекта</summary>
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

		/// <summary>Рекурсивно поискать по дереву до рута настройки</summary>
		/// <param name="node">Узел дерева</param>
		/// <returns>Настройки сервиса</returns>
		public SettingsDataSet.TreeRow FindSettings()
		{
			ServiceTreeNode node = this;
			while(node != null)
			{
				TreeImageList imageIndex = node.NodeType;
				if(imageIndex == TreeImageList.Endpoint || imageIndex == TreeImageList.Folder)
					break;
				node = node.Parent;//Итеративно добираюсь до рута
			}

			return node?.Tag.Row;
		}
	}
}