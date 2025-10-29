using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Xml;
using Microsoft.CSharp;

namespace Plugin.WcfClient.Parser.Wcf
{
	internal class ServiceWcfAnalyzer : ServiceAnalyzer
	{
		public ServiceWcfAnalyzer(ServiceProjectStartupInfo info, BackgroundWorker worker, Single startProgress, Single progressRange)
			: base(info, worker, startProgress, progressRange)
		{ }

		protected override AppDomain CompileProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ref String errorMessage)
		{
			ServiceModelSectionGroup configObject = base.AnalyzeConfig(services, ref errorMessage);
			if(configObject == null)
				return null;

			CompilerResults compilerResults;
			using(CSharpCodeProvider provider = new CSharpCodeProvider())
				compilerResults = provider.CompileAssemblyFromFile(new CompilerParameters
				{
					OutputAssembly = this.Info.AssemblyFilePath,
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
					//CompilerOptions = "/platform:x86"//TODO: By default, a 32-bit assembly is generated???
				}, new String[] { this.Info.ProxyFilePath });

			if(compilerResults.Errors.Count == 0)
				return base.AnalyzeProxy(services, configObject);

			if(errorMessage == null)
				errorMessage = String.Empty;
			foreach(CompilerError compilerError in compilerResults.Errors)
				errorMessage = errorMessage + compilerError.ToString() + Environment.NewLine;
			return null;
		}

		protected override Boolean GenerateProxyAndConfig(Int32 startProgressPosition, Int32 endProgressPostition, out String successMessage, out String errorMessage)
		{
			Boolean result = base.GenerateProxyAndConfig(startProgressPosition, endProgressPostition, out successMessage, out errorMessage);

			if(result)
			{
				String oldPath = base.Info.Plugin.Settings.RegenerateConfigEnabled || !ConfigFileMappingManager.Instance.DoesConfigMappingExist(base.Info.Row.Name)
					? this.Info.DefaultConfigFilePath
					: ConfigFileMappingManager.Instance.GetSavedConfigPath(this.Info.Row.Name);

				ServiceAnalyzer.CopyConfigFile(oldPath, base.Info.ConfigFilePath);
			}

			return result;
		}

		/// <summary>Creating a process to download and compile the service client</summary>
		/// <returns>Win32 process</returns>
		protected override Process CreateProcess()
		{
			var serviceRow = base.Info.Row.ServiceRow;
			String arguments = String.Concat(new String[]
				{
					"/targetClientVersion:Version35 \"",
					this.Info.Row.Name,
					"\" \"/out:",
					this.Info.ProxyFilePath,
					"\" \"/config:",
					this.Info.DefaultConfigFilePath,
					"\""
				});

			return ToolingEnvironment.CreateProcess(serviceRow.ServiceType, arguments);
		}
	}
}