using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using System.Xml;
using Microsoft.CSharp;
using Plugin.WsTestClient.Bll;
using Plugin.WsTestClient.Reflection;

namespace Plugin.WsTestClient.WCF
{
	internal class ServiceAnalyzer
	{
		private readonly ServiceProjectStartupInfo _info;
		private ServiceProjectStartupInfo Info { get { return this._info; } }

		public ServiceAnalyzer(ServiceProjectStartupInfo info)
		{
			this._info = info;
		}

		public ServiceProject DownloadService(BackgroundWorker addServiceWorker, Single startProgress, Single progressRange, out String errorMessage)
		{
			if(!this.GenerateProxyAndConfig(this.GetIntValueOfProgress(startProgress, progressRange, 0.1f), this.GetIntValueOfProgress(startProgress, progressRange, 0.5f), addServiceWorker, out errorMessage))
			{
				ServiceAnalyzer.DeleteProjectFolder(this.Info.ProjectPath);
				return null;
			}

			return this.AnalyzeService(addServiceWorker, startProgress, progressRange, out errorMessage);
		}

		public ServiceProject AnalyzeService(BackgroundWorker addServiceWorker, Single startProgress, Single progressRange, out String errorMessage)
		{
			errorMessage = String.Empty;

			Dictionary<ChannelEndpointElement, ClientEndpointInfo> services = new Dictionary<ChannelEndpointElement, ClientEndpointInfo>();
			AppDomain appDomain;

			switch(this.Info.Row.ServiceRow.ServiceType)
			{
			case ServiceType.WCF:
				ServiceModelSectionGroup configObject = this.AnalyzeConfig(services, ref errorMessage);
				if(configObject == null || this.CancelOrReportProgress(addServiceWorker, null, this.GetIntValueOfProgress(startProgress, progressRange, 0.7f), this.Info.ProjectPath))
					return null;

				appDomain = this.AnalyzeWcfProxy(services, configObject, ref errorMessage);
				break;
			case ServiceType.WS:
				appDomain = this.AnalyzeWsProxy(services, ref errorMessage);
				break;
			default:
				throw new NotImplementedException();
			}

			if(appDomain == null || this.CancelOrReportProgress(addServiceWorker, appDomain, this.GetIntValueOfProgress(startProgress, progressRange, 0.9f), this.Info.ProjectPath))
				return null;

			return new ServiceProject(this.Info, services.Values, appDomain);
		}

		public ServiceModelSectionGroup AnalyzeConfig(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ref String errorMessage)
		{
			ServiceModelSectionGroup result;
			try
			{
				Configuration config = ConfigurationManager.OpenMachineConfiguration();
				ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap()
				{
					MachineConfigFilename = config.FilePath,
					ExeConfigFilename = this.Info.ConfigPath,
				};

				config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
				result = ServiceModelSectionGroup.GetSectionGroup(config);
				foreach(ChannelEndpointElement endpoint in result.Client.Endpoints)
					services.Add(endpoint, new ClientEndpointInfo(endpoint.Contract));

			} catch(ConfigurationErrorsException ex)
			{
				errorMessage = (errorMessage ?? String.Empty) + ex.Message;
				result = null;
			}
			return result;
		}

		public AppDomain AnalyzeProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ServiceModelSectionGroup configObject)
		{
			try
			{
				String privateBinPath = Assembly.GetExecutingAssembly().Location;
				if(String.IsNullOrEmpty(privateBinPath))//Т.к. модуль может лежать в другом месте
					if(Assembly.GetEntryAssembly() == null)
						privateBinPath = Process.GetCurrentProcess().MainModule.FileName;
					else//Если плагин к EnvDTE
						privateBinPath = Assembly.GetEntryAssembly().Location;

				AppDomain result = this.CreateAppDomain(Path.GetDirectoryName(privateBinPath));
				//TODO: Если плагин загружен через память, то Location будет пустой.
				//TODO: Как один и вариантов, сохранить текущую сборку на диске и прописать путь к ней.
				//TODO: Или взять путь к сборке, который передаёт провайдер плагинов

				try
				{
					using(DataContractAnalyzer dataContractAnalyzer = (DataContractAnalyzer)result.CreateInstanceAndUnwrap(
						Assembly.GetExecutingAssembly().FullName,
						typeof(DataContractAnalyzer).FullName))
					{
						if(configObject == null && services.Count == 0)//WebService
							services.Add(new ChannelEndpointElement(), dataContractAnalyzer.AnalyzeWebService());
						else
							foreach(ChannelEndpointElement key in configObject.Client.Endpoints)
							{
								ClientEndpointInfo clientEndpointInfo = services[key];
								clientEndpointInfo = dataContractAnalyzer.AnalyzeDataContract(clientEndpointInfo);
								if(clientEndpointInfo == null)
									services.Remove(key);
								else
									services[key] = clientEndpointInfo;
							}
					}

					foreach(KeyValuePair<ChannelEndpointElement, ClientEndpointInfo> current in services)
						current.Value.EndpointConfigurationName = current.Key.Name;
				} catch
				{//TODO: Не проверено на Fatal Exception'ы
					ServiceAnalyzer.UnloadAppDomain(result);
					throw;
				}
				return result;
			} catch(ArgumentException exc)
			{
				this.Info.Plugin.Trace.TraceEvent(TraceEventType.Error, 1,
					"Message: {1}{0}configPath: {2}{0}assemblyPath: {3}{0}Location: {4}{0}Code Base: {5}",
					Environment.NewLine,
					exc.StackTrace,
					this.Info.ConfigPath,
					this.Info.AssemblyPath,
					Assembly.GetExecutingAssembly().Location,
					Assembly.GetExecutingAssembly().CodeBase);
				return null;
			}
		}

		public static void CopyConfigFile(String oldPath, String newPath)
		{
			ExceptionUtility.InvokeFSAction(delegate { File.Copy(oldPath, newPath, true); });
		}

		public static void UnloadAppDomain(AppDomain domain)
		{
			try
			{
				AppDomain.Unload(domain);
			} catch(CannotUnloadAppDomainException exc)
			{
				MessageBox.Show(exc.Message, "CannotUnloadAppDomainException", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			domain = null;
		}

		private static void DeleteProjectFolder(String projectDirectory)
		{
			ExceptionUtility.InvokeFSAction(delegate { Directory.Delete(projectDirectory, true); });
		}

		/// <summary>Creates a new AppDomain based on the parent AppDomains Evidence and AppDomainSetup</summary>
		/// <returns>A newly created AppDomain</returns>
		private AppDomain CreateAppDomain(String privateBinPath)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup()
			{
				ConfigurationFile = this.Info.ConfigPath,
				ApplicationBase = privateBinPath,
			};

			AppDomain domain = AppDomain.CreateDomain(this.Info.ConfigPath, AppDomain.CurrentDomain.Evidence, appDomainSetup);
			domain.SetClientAssemblyPath(this.Info.AssemblyPath);
			return domain;
		}

		private Boolean GenerateProxyAndConfig(Int32 startProgressPosition, Int32 endProgressPostition, BackgroundWorker addServiceWorker, out String errorMessage)
		{
			if(!Directory.Exists(this.Info.ProjectPath))
				Directory.CreateDirectory(this.Info.ProjectPath);

			Boolean isSuccess;
			using(Process process = this.CreateProcess())
			{
				while(!process.HasExited)
				{
					if(addServiceWorker.CancellationPending)
					{
						errorMessage = String.Empty;
						return false;
					}
					if(startProgressPosition < endProgressPostition)
						addServiceWorker.ReportProgress(startProgressPosition++);
					Thread.Sleep(50);
				}
				errorMessage = process.StandardError.ReadToEnd();
				isSuccess = process.ExitCode == 0 && String.IsNullOrEmpty(errorMessage);
				//При ошибки генерации WSDL из WCF сервиса, ExitCode==0.
				//TODO: Но, есть ошибки, при которых, файлы создаются и сервис работоспособен.

				/*switch(info.Row.ServiceType)
				{
				case ServiceType.WCF:
					isSuccess = File.Exists(info.ProxyPath) && File.Exists(info.DefaultConfigPath);
					break;
				case ServiceType.WS:
					isSuccess = File.Exists(info.ProxyPath);
					break;
				default:
					throw new NotImplementedException();
				}*/
			}

			if(isSuccess && this.Info.Row.ServiceRow.ServiceType == ServiceType.WCF)
			{
				if(this.Info.Plugin.Settings.RegenerateConfigEnabled || !ConfigFileMappingManager.Instance.DoesConfigMappingExist(this.Info.Row.Name))
					ServiceAnalyzer.CopyConfigFile(this.Info.DefaultConfigPath, this.Info.ConfigPath);
				else
					ServiceAnalyzer.CopyConfigFile(ConfigFileMappingManager.Instance.GetSavedConfigPath(this.Info.Row.Name), this.Info.ConfigPath);
			}
			return isSuccess;
		}

		/// <summary>Создание процесса загруски и компиляции клиента сервиса</summary>
		/// <returns>Win32 процесс</returns>
		private Process CreateProcess()
		{
			var serviceRow=this.Info.Row.ServiceRow;
			String arguments;
			switch(serviceRow.ServiceType)
			{
			case ServiceType.WCF:
				arguments = String.Concat(new String[]
				{
					"/targetClientVersion:Version35 \"",
					this.Info.Row.Name,
					"\" \"/out:",
					this.Info.ProxyPath,
					"\" \"/config:",
					this.Info.DefaultConfigPath,
					"\""
				});
				break;
			case ServiceType.WS:
				StringBuilder args = new StringBuilder();
				args.Append(String.Concat(
					"/language:CS \"",
					this.Info.Row.Name,
					"\" /out:\"",
					this.Info.ProxyPath,
					"\""));
				if(serviceRow.UseProxy)
					args.Append(String.Concat(
						" /proxyusername:\"",
						serviceRow.ProxyLogin,
						"\" /proxypassword:\"",
						serviceRow.ProxyPassword,
						"\""));
				if(serviceRow.UserAuthentication)
					args.Append(String.Concat(
						" /username:\"",
						serviceRow.Login,
						"\" /password:\"",
						serviceRow.Password,
						"\""));
				arguments = args.ToString();
				break;
			default:
				throw new NotImplementedException();
			}

			return ToolingEnvironment.CreateProcess(serviceRow.ServiceType, arguments);
		}

		/*private static StringBuilder SvcUtilError;
		private static void Svcutil_ErrorDataReceived(Object sender, DataReceivedEventArgs e)
		{
			ServiceAnalyzer.SvcUtilError.AppendLine(e.Data);
		}*/
		private AppDomain AnalyzeWcfProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ServiceModelSectionGroup configObject, ref String errorMessage)
		{
			CompilerResults compilerResults;
			using(CSharpCodeProvider provider = new CSharpCodeProvider())
				compilerResults = provider.CompileAssemblyFromFile(new CompilerParameters
				{
					OutputAssembly = this.Info.AssemblyPath,
					ReferencedAssemblies = 
				{
					"System.dll",
					typeof(DataSet).Assembly.Location,
					typeof(TypedTableBaseExtensions).Assembly.Location,
					typeof(XmlReader).Assembly.Location,
					typeof(OperationDescription).Assembly.Location,
					typeof(DataContractAttribute).Assembly.Location
				},
					GenerateExecutable = false,
					//CompilerOptions = "/platform:x86"//TODO: По умолчанию генерится 32 битная сборка???
				}, new String[] { this.Info.ProxyPath });

			if(compilerResults.Errors.Count == 0)
				return this.AnalyzeProxy(services, configObject);

			if(errorMessage == null)
				errorMessage = String.Empty;
			foreach(CompilerError compilerError in compilerResults.Errors)
				errorMessage = errorMessage + compilerError.ToString() + Environment.NewLine;
			return null;
		}

		private AppDomain AnalyzeWsProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ref String errorMessage)
		{
			CompilerResults compilerResults;
			using(CSharpCodeProvider provider = new CSharpCodeProvider())
				compilerResults = provider.CompileAssemblyFromFile(new CompilerParameters
				{
					OutputAssembly = this.Info.AssemblyPath,
					ReferencedAssemblies =
					{
						"System.dll",
						typeof(DataSet).Assembly.Location,
						typeof(SoapHttpClientProtocol).Assembly.Location,
						typeof(XmlReader).Assembly.Location,
					},
					GenerateExecutable = false,
				}, new String[] { this.Info.ProxyPath });

			if(compilerResults.Errors.Count == 0)
				return this.AnalyzeProxy(services, null);

			if(errorMessage == null)
				errorMessage = String.Empty;
			foreach(CompilerError compilerError in compilerResults.Errors)
				errorMessage = errorMessage + compilerError.ToString() + Environment.NewLine;
			return null;
		}

		private Boolean CancelOrReportProgress(BackgroundWorker addServiceWorker, AppDomain clientDomain, Int32 percentProgress, String projectPath)
		{
			if(addServiceWorker.CancellationPending)
			{
				if(clientDomain != null)
					ServiceAnalyzer.UnloadAppDomain(clientDomain);
				ServiceAnalyzer.DeleteProjectFolder(projectPath);
				return true;
			}
			addServiceWorker.ReportProgress(percentProgress);
			return false;
		}

		private Int32 GetIntValueOfProgress(Single startProgress, Single progressRange, Single percent)
		{
			return (Int32)(startProgress + progressRange * percent);
		}
	}
}
