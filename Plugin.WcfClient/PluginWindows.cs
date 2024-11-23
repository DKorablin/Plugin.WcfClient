using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Parser;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.WcfClient
{
	public class PluginWindows : IPlugin, IPluginSettings<PluginSettings>
	{
		private TraceSource _trace;
		private PluginSettings _settings;
		private Dictionary<String, DockState> _documentTypes;
		private List<ServiceProject> _openedProjects;

		internal TraceSource Trace => this._trace ?? (this._trace = PluginWindows.CreateTraceSource<PluginWindows>());

		internal IHostWindows HostWindows { get; }

		/// <summary>Настройки для взаимодействия из хоста</summary>
		Object IPluginSettings.Settings => this.Settings;

		/// <summary>Настройки для взаимодействия из плагина</summary>
		public PluginSettings Settings
		{
			get
			{
				if(this._settings == null)
				{
					this._settings = new PluginSettings(this);
					this.HostWindows.Plugins.Settings(this).LoadAssemblyParameters(this._settings);
				}
				return this._settings;
			}
		}

		private IMenuItem MenuTest { get; set; }
		private IMenuItem MenuNetworkTest { get; set; }
		private IMenuItem MenuWcfTest { get; set; }

		private Dictionary<String, DockState> DocumentTypes
		{
			get
			{
				if(this._documentTypes == null)
					this._documentTypes = new Dictionary<String, DockState>()
					{
						{ typeof(PanelSvcTestClient).ToString(), DockState.DockRightAutoHide },
						{ typeof(DocumentSvcTestMethod).ToString(), DockState.Document },
					};
				return this._documentTypes;
			}
		}

		/// <summary>Открытый список проектов</summary>
		private List<ServiceProject> OpenedProjects
			=> this._openedProjects ?? (this._openedProjects = new List<ServiceProject>());

		internal event EventHandler<ServiceListChangedEventArgs> ServiceListChanged;

		public PluginWindows(IHostWindows hostWindows)
			=> this.HostWindows = hostWindows ?? throw new ArgumentNullException(nameof(hostWindows));

		public IWindow GetPluginControl(String typeName, Object args)
			=> this.CreateWindow(typeName, false, args);

		Boolean IPlugin.OnConnection(ConnectMode mode)
		{
			IMenuItem menuTools = this.HostWindows.MainMenu.FindMenuItem("Tools");
			if(menuTools == null)
			{
				this.Trace.TraceEvent(TraceEventType.Error, 10, "Menu item 'Tools' not found");
				return false;
			}

			this.MenuTest = menuTools.FindMenuItem("Test");
			if(this.MenuTest == null)
			{
				this.MenuTest = menuTools.Create("Test");
				this.MenuTest.Name = "Tools.Test";
				menuTools.Items.Add(this.MenuTest);
			}

			this.MenuNetworkTest = this.MenuTest.FindMenuItem("Network");
			if(this.MenuNetworkTest == null)
			{
				this.MenuNetworkTest = this.MenuTest.Create("Network");
				this.MenuNetworkTest.Name = "Tools.Test.Network";
				this.MenuTest.Items.Add(this.MenuNetworkTest);
			}

			this.MenuWcfTest = this.MenuNetworkTest.Create("Service Test Client");
			this.MenuWcfTest.Name = "Tools.Test.Network.ServiceTestClient";
			this.MenuWcfTest.Click += (sender, e)=> { this.CreateWindow(typeof(PanelSvcTestClient).ToString(), true); };
			this.MenuNetworkTest.Items.AddRange(new IMenuItem[] { this.MenuWcfTest, });
			return true;
		}

		Boolean IPlugin.OnDisconnection(DisconnectMode mode)
		{
			if(this.MenuWcfTest != null)
				this.HostWindows.MainMenu.Items.Remove(this.MenuWcfTest);
			if(this.MenuNetworkTest != null && this.MenuNetworkTest.Items.Count == 0)
				this.HostWindows.MainMenu.Items.Remove(this.MenuNetworkTest);
			if(this.MenuTest != null && this.MenuTest.Items.Count == 0)
				this.HostWindows.MainMenu.Items.Remove(this.MenuTest);
			return true;
		}

		internal IWindow CreateWindow(String typeName, Boolean searchForOpened, Object args = null)
			=> this.DocumentTypes.TryGetValue(typeName, out DockState state)
				? this.HostWindows.Windows.CreateWindow(this, typeName, searchForOpened, state, args)
				: null;

		private void OnServiceListChanged(SettingsDataSet.TreeRow row, ServiceListChangedEventArgs.ChangeStatus status)
			=> this.ServiceListChanged?.Invoke(this, new ServiceListChangedEventArgs(row, status));

		/// <summary>Получить информацию о открытом проекте</summary>
		/// <param name="row">Ряд идентифицирующий открытый сервис</param>
		/// <returns>Информацию о открытом сервисе</returns>
		internal ServiceProject GetOpenedProject(SettingsDataSet.TreeRow row)
			=> this.OpenedProjects.FirstOrDefault(p => p.Info.Row == row);//Поиск по открытым сервисам

		/// <summary>Получить информацию по открытому проекту исходя из идентификатора дерева</summary>
		/// <param name="treeId">Идентификатор дерева</param>
		/// <returns>Информация о открытом проекте</returns>
		internal ServiceProject GetOpenedProject(Int32 treeId)
			=> this.OpenedProjects.FirstOrDefault(p => p.Info.Row.TreeId == treeId);

		internal void LoadService(AddServiceOutputs outputs)
		{
			EventHandler<ServiceListChangedEventArgs> evt = this.ServiceListChanged;
			foreach(ServiceProject project in outputs.ServiceProjects)
			{
				this.OpenedProjects.Add(project);
				if(evt != null)
					this.ServiceListChanged(this, new ServiceListChangedEventArgs(project));
			}
		}

		internal void RemoveNode(SettingsDataSet.TreeRow root)
		{
			this.OnServiceListChanged(root, ServiceListChangedEventArgs.ChangeStatus.Removed);

			switch(root.ElementType)
			{
			case ElementType.Client:
				this.RemoveService(root);
				break;
			case ElementType.Tree:
				foreach(SettingsDataSet.TreeRow row in this.Settings.ServiceSettings.GetTreeNodes(root.TreeId))
					this.RemoveNode(row);
				this.Settings.ServiceSettings.RemoveNode(root);
				break;
			default:
				throw new NotImplementedException();
			}
			this.Settings.ServiceSettings.Save();
		}

		/// <summary>Удалить сервис и его настройки</summary>
		/// <param name="row">Ряд характеризующий настройку сервиса</param>
		private void RemoveService(SettingsDataSet.TreeRow row)
		{
			ServiceProject project = this.GetOpenedProject(row);//Поиск по открытым сервисам

			if(project == null)//Если проект не открыт, то только удалить его из настроек
				this.Settings.ServiceSettings.RemoveNode(row);
			else
			{
				IWindow[] windows = this.HostWindows.Windows.ToArray();
				for(Int32 loop = windows.Length - 1; loop >= 0; loop--)
				{//Закрыть все открытые окна с проектом
					IWindow wnd = windows[loop];
					DocumentSvcTestMethod ctrl = wnd.Control as DocumentSvcTestMethod;
					if(ctrl != null && ctrl.Settings.TreeId == row.TreeId)
						wnd.Close();
				}
				this.OpenedProjects.Remove(project);
				project.Remove();//Внутри произойдёт удаление из настроек
			}
		}

		/// <summary>Добавить сервис</summary>
		/// <param name="type">Тип добавляемого сервиса</param>
		/// <param name="address">Адрес сервиса</param>
		internal void AddService(ServiceType type, String address)
		{
			SettingsDataSet.TreeRow row = this.Settings.ServiceSettings.ModifyTreeNode(null, null, ElementType.Client, address);
			this.Settings.ServiceSettings.ModifyClient(row, type, this.Settings.ProxyUserName, this.Settings.ProxyPassword);
			this.OnServiceListChanged(row, ServiceListChangedEventArgs.ChangeStatus.Added);
		}

		/// <summary>Выгрузить проект</summary>
		/// <param name="row">Описатель проекта</param>
		/// <returns>Результат выгрузки проекта</returns>
		internal Boolean UnloadService(SettingsDataSet.TreeRow row)
		{
			ServiceProject projct = this.OpenedProjects.FirstOrDefault(p => p.Info.Row == row);//Поиск по открытым проектам
			if(projct != null)
			{
				projct.CloseService();
				this.OnServiceListChanged(row, ServiceListChangedEventArgs.ChangeStatus.Unloaded);
				this.OpenedProjects.Remove(projct);
			}
			return projct != null;
		}

		internal void OnServicePropertiesChanged(SettingsDataSet.TreeRow row)
			=> this.OnServiceListChanged(row, ServiceListChangedEventArgs.ChangeStatus.Changed);

		internal void RefreshWcfConfig(ServiceProject project)
		{
			//this.GlobalStatusLabel.Text = StringResources.StatusLoadingConfig;
			project.RefreshConfig();
			//this.GlobalStatusLabel.Text = StringResources.StatusLoadingConfigFailed;
			//else
			{
				this.Trace.TraceEvent(TraceEventType.Verbose, 1, "Refresh not implemented");
				/*this.serviceTreeView.BeginUpdate();
				TreeNode treeNode = this.serviceTreeView.Nodes[0];
				foreach(TreeNode node in tree.Nodes)
				{
					ServiceProject objA = node.Tag as ServiceProject;
					if(Object.ReferenceEquals(objA, serviceProject))
					{
						node.Nodes.Clear();
						this.AddNodesForServiceProject(serviceProject, node);
						break;
					}
				}
				this.serviceTreeView.EndUpdate();*/
				//this.GlobalStatusLabel.Text = StringResources.StatusLoadingConfigCompleted;
			}
		}

		private static TraceSource CreateTraceSource<T>(String name = null) where T : IPlugin
		{
			TraceSource result = new TraceSource(typeof(T).Assembly.GetName().Name + name);
			result.Switch.Level = SourceLevels.All;
			result.Listeners.Remove("Default");
			result.Listeners.AddRange(System.Diagnostics.Trace.Listeners);
			return result;
		}
	}
}