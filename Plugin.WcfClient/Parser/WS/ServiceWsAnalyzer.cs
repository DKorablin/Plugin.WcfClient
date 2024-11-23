using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceModel.Configuration;
using System.Text;
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.CSharp;

namespace Plugin.WcfClient.Parser.Ws
{
	internal class ServiceWsAnalyzer : ServiceAnalyzer
	{
		public ServiceWsAnalyzer(ServiceProjectStartupInfo info, BackgroundWorker worker, Single startProgress, Single progressRange)
			: base(info, worker, startProgress, progressRange)
		{ }

		protected override AppDomain CompileProxy(IDictionary<ChannelEndpointElement, ClientEndpointInfo> services, ref String errorMessage)
		{
			CompilerResults compilerResults;
			using(CSharpCodeProvider provider = new CSharpCodeProvider())
				compilerResults = provider.CompileAssemblyFromFile(new CompilerParameters
				{
					OutputAssembly = this.Info.AssemblyFilePath,
					ReferencedAssemblies =
					{
						"System.dll",
						typeof(DataSet).Assembly.Location,
						typeof(SoapHttpClientProtocol).Assembly.Location,
						typeof(XmlReader).Assembly.Location,
					},
					GenerateExecutable = false,
				}, new String[] { this.Info.ProxyFilePath });

			if(compilerResults.Errors.Count == 0)
				return this.AnalyzeProxy(services, null);

			if(errorMessage == null)
				errorMessage = String.Empty;
			foreach(CompilerError compilerError in compilerResults.Errors)
				errorMessage = errorMessage + compilerError.ToString() + Environment.NewLine;
			return null;
		}

		/// <summary>Создание процесса загрузки и компиляции клиента сервиса</summary>
		/// <returns>Win32 процесс</returns>
		protected override Process CreateProcess()
		{
			var serviceRow = this.Info.Row.ServiceRow;
			StringBuilder arguments = new StringBuilder();

			arguments.Append(String.Concat(
				"/language:CS \"",
				this.Info.Row.Name,
				"\" /out:\"",
				this.Info.ProxyFilePath,
				"\""));
			if(serviceRow.UseProxy)
				arguments.Append(String.Concat(
					" /proxyusername:\"",
					serviceRow.ProxyLogin,
					"\" /proxypassword:\"",
					serviceRow.ProxyPassword,
					"\""));
			if(serviceRow.UserAuthentication)
				arguments.Append(String.Concat(
					" /username:\"",
					serviceRow.Login,
					"\" /password:\"",
					serviceRow.Password,
					"\""));

			return ToolingEnvironment.CreateProcess(serviceRow.ServiceType, arguments.ToString());
		}
	}
}