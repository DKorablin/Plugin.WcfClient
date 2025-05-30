﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel.Configuration;
using System.Threading;
using System.Windows.Forms;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Extensions;
using Plugin.WcfClient.Parser.Wcf;
using Plugin.WcfClient.Parser.Ws;

namespace Plugin.WcfClient.Parser
{
	internal abstract class ServiceAnalyzer
	{
		private readonly BackgroundWorker _worker;
		private readonly Single _startProgress;
		private readonly Single _progressRange;

		protected ServiceProjectStartupInfo Info { get; }

		public static ServiceAnalyzer Create(ServiceProjectStartupInfo info, BackgroundWorker worker, Single startProgress, Single progressRange)
		{
			switch(info.Row.ServiceRow.ServiceType)
			{
			case ServiceType.WCF:
				return new ServiceWcfAnalyzer(info, worker, startProgress, progressRange);
			case ServiceType.WS:
				return new ServiceWsAnalyzer(info, worker, startProgress, progressRange);
			default:
				throw new NotImplementedException($"ServiceType {info.Row.ServiceRow.ServiceType} not implemented");
			}
		}

		protected ServiceAnalyzer(ServiceProjectStartupInfo info, BackgroundWorker worker, Single startProgress, Single progressRange)
		{
			this.Info = info ?? throw new ArgumentNullException(nameof(info));

			this._worker = worker;
			this._startProgress = startProgress;
			this._progressRange = progressRange;
		}

		public ServiceProject DownloadService(out String errorMessage)
		{
			Int32 startProgress = this.GetIntValueOfProgress(this._startProgress, this._progressRange, 0.1f);
			Int32 endProgress = this.GetIntValueOfProgress(this._startProgress, this._progressRange, 0.5f);

			String outMessage;
			Boolean result = this.GenerateProxyAndConfig(startProgress, endProgress, out outMessage, out errorMessage);
			if(!String.IsNullOrEmpty(outMessage))//Пишу исходящие тексты от результата выполнения внешнего процесса (иногда там могут быть Warning'и даже при успешном создании сборки)
				this.Info.Plugin.Trace.TraceEvent(TraceEventType.Information, 10, outMessage);

			if(result)
				return this.AnalyzeService(out errorMessage);
			else
			{
				ServiceAnalyzer.DeleteProjectFolder(this.Info.ProjectPath);
				return null;
			}
		}

		public ServiceProject AnalyzeService(out String errorMessage)
		{
			errorMessage = String.Empty;

			Dictionary<ChannelEndpointElement, ClientEndpointInfo> services = new Dictionary<ChannelEndpointElement, ClientEndpointInfo>();

			AppDomain appDomain = this.CompileProxy(services, ref errorMessage);

			return appDomain == null || this.CancelOrReportProgress(appDomain, 0.7f)
				? null
				: new ServiceProject(this.Info, services.Values, appDomain);
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
					ExeConfigFilename = this.Info.ConfigFilePath,
				};

				config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
				result = ServiceModelSectionGroup.GetSectionGroup(config);
				foreach(ChannelEndpointElement endpoint in result.Client.Endpoints)
					services.Add(endpoint, new ClientEndpointInfo(endpoint.Contract));

				if(this.CancelOrReportProgress(null, 0.7f))
					return null;

			} catch(ConfigurationErrorsException ex)
			{
				errorMessage = (errorMessage ?? String.Empty) + ex.Message;
				result = null;
			}
			return result;
		}

		protected abstract AppDomain CompileProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ref String errorMessage);

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
				this.Info.Plugin.Trace.TraceData(TraceEventType.Error, 10, exc);
				this.Info.Plugin.Trace.TraceEvent(TraceEventType.Error, 11,
					"configPath: {1}{0}assemblyPath: {2}{0}Location: {3}{0}Code Base: {4}",
					Environment.NewLine,
					this.Info.ConfigFilePath,
					this.Info.AssemblyFilePath,
					Assembly.GetExecutingAssembly().Location,
					Assembly.GetExecutingAssembly().CodeBase);
				return null;
			}
		}

		public static void CopyConfigFile(String oldPath, String newPath)
			=> ExceptionUtility.InvokeFSAction(delegate { File.Copy(oldPath, newPath, true); });

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
			=> ExceptionUtility.InvokeFSAction(delegate { Directory.Delete(projectDirectory, true); });

		/// <summary>Creates a new AppDomain based on the parent AppDomains Evidence and AppDomainSetup</summary>
		/// <returns>A newly created AppDomain</returns>
		private AppDomain CreateAppDomain(String privateBinPath)
		{
			AppDomainSetup appDomainSetup = new AppDomainSetup()
			{
				ConfigurationFile = this.Info.ConfigFilePath,
				ApplicationBase = privateBinPath,
			};

			AppDomain domain = AppDomain.CreateDomain(this.Info.ConfigFilePath, AppDomain.CurrentDomain.Evidence, appDomainSetup);
			domain.SetClientAssemblyPath(this.Info.AssemblyFilePath);
			return domain;
		}

		protected virtual Boolean GenerateProxyAndConfig(Int32 startProgressPosition, Int32 endProgressPostition, out String successMessage, out String errorMessage)
		{
			if(!Directory.Exists(this.Info.ProjectPath))
				Directory.CreateDirectory(this.Info.ProjectPath);

			Boolean isSuccess;
			using(Process process = this.CreateProcess())
			{
				Int32 milliseconds = 0;
				while(!process.HasExited)
				{
					if(this._worker.CancellationPending)
					{
						successMessage = null;
						errorMessage = null;
						return false;
					}
					if(startProgressPosition < endProgressPostition)
						this._worker.ReportProgress(startProgressPosition++);

					Thread.Sleep(50);
					milliseconds += 50;
					if(milliseconds > 10000)
						break;//Есть процессы, где пока не прочитаешь StandardOutput, процесс не выйдет. Поэтому нужен таймаут, дабы прочитать Output и надеятся на выход процесса
				}
				successMessage = process.StandardOutput.ReadToEnd();
				errorMessage = process.StandardError.ReadToEnd();
				isSuccess = process.HasExited && process.ExitCode == 0 && String.IsNullOrEmpty(errorMessage);
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

			return isSuccess;
		}

		/// <summary>Создание процесса загруски и компиляции клиента сервиса</summary>
		/// <returns>Win32 процесс</returns>
		protected abstract Process CreateProcess();

		/*private static StringBuilder SvcUtilError;
		private static void Svcutil_ErrorDataReceived(Object sender, DataReceivedEventArgs e)
		{
			ServiceAnalyzer.SvcUtilError.AppendLine(e.Data);
		}*/

		private Int32 GetIntValueOfProgress(Single startProgress, Single progressRange, Single percent)
			=> (Int32)(startProgress + progressRange * percent);

		protected Boolean CancelOrReportProgress(AppDomain clientDomain, Single percent)
		{
			if(this._worker == null)
				return false;

			Int32 percentProgress = this.GetIntValueOfProgress(this._startProgress, this._progressRange, percent);
			return this.CancelOrReportProgress(this._worker, clientDomain, percentProgress);
		}

		private Boolean CancelOrReportProgress(BackgroundWorker addServiceWorker, AppDomain clientDomain, Int32 percentProgress)
		{
			if(addServiceWorker.CancellationPending)
			{
				if(clientDomain != null)
					ServiceAnalyzer.UnloadAppDomain(clientDomain);
				ServiceAnalyzer.DeleteProjectFolder(this.Info.ProjectPath);
				return true;
			}
			addServiceWorker.ReportProgress(percentProgress);
			return false;
		}
	}
}