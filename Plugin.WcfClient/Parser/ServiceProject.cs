using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceModel.Configuration;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	/// <summary>Шина подключения к библиотеки сервиса</summary>
	internal class ServiceProject
	{
		private const String CommandLineFormatString = "\"{0}\"";
		private FileStream _fileLock;
		private FileSystemWatcher _configWatcher;
		
		public Boolean IsConfigChanged { get; private set; }
		
		public Boolean IsWorking { get; set; }

		/// <summary>Домен в который загружена сборка сервиса</summary>
		public AppDomain ClientDomain { get; private set; }
		
		public ICollection<ClientEndpointInfo> Endpoints { get; private set; }
		
		public ServiceProjectStartupInfo Info { get; }

		public String[] ReferencedFiles
			=> new String[]
				{
					this.Info.ConfigFilePath,
					this.Info.ProxyFilePath,
				};

		public ServiceProject(ServiceProjectStartupInfo info, ICollection<ClientEndpointInfo> endpoints, AppDomain clientDomain)
		{
			this.Info = info;
			this.Endpoints = endpoints;
			this.ClientDomain = clientDomain;
			this._fileLock = File.Open(info.AssemblyFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			this.UpdateAndValidateEndpointsInfo();

			if(!this.Info.Plugin.Settings.RegenerateConfigEnabled)
				this.CreateProxiesForEndpoints();

			if(File.Exists(info.ConfigFilePath))//WS service may be without config file
				this.RegisterFileWatcher(info.ConfigFilePath);
		}

		/// <summary>Обновить конфигурацию сервиса</summary>
		public void RefreshConfig()
		{
			this.IsConfigChanged = false;

			ServiceAnalyzer analyzer = ServiceAnalyzer.Create(this.Info, null, 0, 0);
			IDictionary<ChannelEndpointElement, ClientEndpointInfo> services = new Dictionary<ChannelEndpointElement, ClientEndpointInfo>();
			String errorMessage = null;
			ServiceModelSectionGroup config = analyzer.AnalyzeConfig(services, ref errorMessage);
			if(errorMessage != null)
				throw new ArgumentNullException(errorMessage);

			this.CloseService();

			this.ClientDomain = analyzer.AnalyzeProxy(services, config);
			this.Endpoints = services.Values;
			this.UpdateAndValidateEndpointsInfo();

			this.CreateProxiesForEndpoints();

			ConfigFileMappingManager instance = ConfigFileMappingManager.Instance;
			instance.AddConfigFileMapping(this.Info.Row.Name);
			if(this.Info.Row.ServiceRow.ServiceType == ServiceType.WCF)
				ServiceAnalyzer.CopyConfigFile(this.Info.ConfigFilePath, instance.GetSavedConfigPath(this.Info.Row.Name));

			if(File.Exists(this.Info.ConfigFilePath))
				this.RegisterFileWatcher(this.Info.ConfigFilePath);
		}

		/// <summary>Удалить проект</summary>
		public void Remove()
		{
			this.CloseService();
			this.DeleteProjectFolder();

			this.Info.Plugin.Settings.ServiceSettings.RemoveNode(this.Info.Row);
		}

		public void RestoreDefaultConfig(out String errorMessage)
		{
			errorMessage = null;
			String text = Path.Combine(this.Info.ProjectPath, "default.config");
			if(!File.Exists(text))
				errorMessage = "StringResources.DefaultConfigNotFoundDetail";

			if(this.Info.Row.ServiceRow.ServiceType == ServiceType.WCF)
				ServiceAnalyzer.CopyConfigFile(text, this.Info.ConfigFilePath);
		}

		/// <summary>Запуск процесса конфигурирования WCF конфиг файла</summary>
		/// <exception cref="FileNotFoundException">SVC Config editor not found</exception>
		public void StartSvcConfigEditor()
		{
			this.Info.Plugin.Trace.TraceInformation("Opening {0} editor...", Path.GetFileName(this.Info.ConfigFilePath));
			Process process = ToolingEnvironment.CreateProcess(
				ToolingEnvironment.SvcConfigEditorPath,
				String.Format(CultureInfo.CurrentUICulture, "\"{0}\"", this.Info.ConfigFilePath));
		}

		/// <summary>Закрываю сервис и все открытые ручки</summary>
		public void CloseService()
		{
			this.DeleteProxiesForEndpoints();
			ServiceAnalyzer.UnloadAppDomain(this.ClientDomain);
			this._fileLock.Close();
			if(this._configWatcher != null)
			{
				this._configWatcher.Dispose();
				this._configWatcher = null;
			}

			if(this.Info.Plugin.Settings.RegenerateConfigEnabled)
				ConfigFileMappingManager.Instance.DeleteConfigFileMapping(this.Info.Row.Name);
		}

		/// <summary>Зарегистрировать мониторинг конфигурационного файла</summary>
		/// <param name="filePath">Путь к файлу конфигурации</param>
		private void RegisterFileWatcher(String filePath)
		{
			if(this._configWatcher == null)
			{
				this._configWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath))
				{
					NotifyFilter = NotifyFilters.LastWrite
				};
				this._configWatcher.Changed += new FileSystemEventHandler(configWatcher_Changed);
				this._configWatcher.Disposed += new EventHandler(configWatcher_Disposed);
				this._configWatcher.EnableRaisingEvents = true;
			}
		}

		private void configWatcher_Changed(Object sender, FileSystemEventArgs e)
		{
			if(e.ChangeType == WatcherChangeTypes.Changed)
			{
				try
				{
					if(this._configWatcher.EnableRaisingEvents)
					{
						this._configWatcher.EnableRaisingEvents = false;
						this.IsConfigChanged = true;
						this.Info.Plugin.Trace.TraceInformation("Config file {0} updated", Path.GetFileName(this.Info.ConfigFilePath));

						this.RefreshConfig();//TODO: Возможен IOException
					}
				} finally
				{
					if(this._configWatcher != null)//TODO: Refresh config recreate all instances
						this._configWatcher.EnableRaisingEvents = true;
				}
			}
		}

		private void configWatcher_Disposed(Object sender, EventArgs e)
			=> this._configWatcher = null;//TODO: При вызове SaveAsync(Object), _watcher оказывается уже Disposed. Поэтому приходится его пересоздавать

		private void CreateProxiesForEndpoints()
		{
			ServiceExecutor serviceExecutor = (ServiceExecutor)this.ClientDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
			foreach(ClientEndpointInfo current in this.Endpoints)
				if(current.Valid)
					serviceExecutor.ConstructClientToCache(this.Info.Row.ServiceRow.ServiceType, current.ProxyIdentifier, current.ClientTypeName, current.EndpointConfigurationName);
		}

		private void DeleteProjectFolder()
			=> ExceptionUtility.InvokeFSAction(delegate { Directory.Delete(this.Info.ProjectPath, true); });

		private void DeleteProxiesForEndpoints()
		{
			ServiceExecutor serviceExecutor = (ServiceExecutor)this.ClientDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
			foreach(ClientEndpointInfo current in this.Endpoints)
				if(current.Valid)
					serviceExecutor.DeleteClient(current.ProxyIdentifier);
		}

		private void UpdateAndValidateEndpointsInfo()
		{
			foreach(ClientEndpointInfo current in this.Endpoints)
				if(current.Methods.Count < 1)
				{
					String message = String.Format(CultureInfo.CurrentUICulture, "The contract '{0}' in client configuration does not match the name in service contract.", current.OperationContractTypeName);
					this.Info.Plugin.Trace.TraceEvent(TraceEventType.Error, 5, message);
				} else
				{
					current.ProxyIdentifier = Path.GetFileName(this.Info.ProjectPath) + current.EndpointConfigurationName;
					current.ServiceProject = this;
				}
		}
	}
}