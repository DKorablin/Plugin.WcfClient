using System;
using System.IO;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	/// <summary>Information for initializing and running a WS\WCF project</summary>
	[Serializable]
	internal class ServiceProjectStartupInfo
	{
		private const String DefaultConfigFileName = "default.config";
		private const String ProjectConfigFileName = "Client.dll.config";
		private const String DefaultProxyFileName = "Client.cs";
		private const String ProjectAssemblyFileName="Client.dll";
		private const String ParametersFolderName = "Parameters\\";

		private String _projectPath;

		/// <summary>Path to the project folder</summary>
		public String ProjectPath
		{
			get
			{
				if(this._projectPath == null)
				{
					if(this.Row.ServiceRow.Path == null || !Directory.Exists(this.Row.ServiceRow.Path))
					{
						String projectBase = ApplicationSettings.Instance.ProjectBase;
						this._projectPath = Path.Combine(projectBase, Guid.NewGuid().ToString());

						this.Plugin.Settings.ServiceSettings.ModifyClientPath(this.Row, this._projectPath);
					} else
						this._projectPath = this.Row.ServiceRow.Path;
				}
				return this._projectPath;
			}
		}

		/// <summary>Project already loaded</summary>
		public Boolean IsProjectExists
		{
			get
			{
				if(Directory.Exists(this.ProjectPath))
				{
					switch(this.Row.ServiceRow.ServiceType)
					{
					case ServiceType.WS:
						return File.Exists(this.AssemblyFilePath) && File.Exists(this.ProxyFilePath);
					case ServiceType.WCF:
						return File.Exists(this.AssemblyFilePath) && File.Exists(this.ConfigFilePath);
					default:
						throw new NotImplementedException(this.Row.ServiceRow.ServiceType.ToString());
					}
				} else
					return false;
			}
		}

		/// <summary>Path to the default configuration file</summary>
		public String DefaultConfigFilePath => Path.Combine(this.ProjectPath, ServiceProjectStartupInfo.DefaultConfigFileName);

		/// <summary>Path to the service configuration file</summary>
		public String ConfigFilePath => Path.Combine(this.ProjectPath, ServiceProjectStartupInfo.ProjectConfigFileName);

		/// <summary>Path to the .cs code that serves as a proxy for communicating with the service</summary>
		public String ProxyFilePath => Path.Combine(this.ProjectPath, ServiceProjectStartupInfo.DefaultProxyFileName);

		/// <summary>Assembly path</summary>
		public String AssemblyFilePath => Path.Combine(this.ProjectPath, ServiceProjectStartupInfo.ProjectAssemblyFileName);

		/// <summary>Path to the folder with service method call parameters</summary>
		public String ParametersPath => Path.Combine(this.ProjectPath, ServiceProjectStartupInfo.ParametersFolderName);

		/// <summary>The plugin instance</summary>
		public PluginWindows Plugin { get; private set; }

		/// <summary>Row in configuration file</summary>
		public SettingsDataSet.TreeRow Row { get; private set; }

		public ServiceProjectStartupInfo(PluginWindows plugin, SettingsDataSet.TreeRow row)
		{
			this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
			this.Row = row ?? throw new ArgumentNullException(nameof(row));
		}
	}
}