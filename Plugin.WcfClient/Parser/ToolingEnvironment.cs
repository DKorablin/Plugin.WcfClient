using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	internal static class ToolingEnvironment
	{
		private static readonly String ExpectedDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Constant.Settings.TestAssembliesFolderName);
		private static String _svcConfigEditorPath;
		private static String _svcUtilPath;
		private static String _wsdlPath;
		private static String _discoPath;

		/// <summary>WCF proxy generator</summary>
		private static String SvcTool
			=> ToolingEnvironment._svcUtilPath ?? (ToolingEnvironment._svcUtilPath = ToolingEnvironment.GetMetaDataToolPath(Constant.Programs.SvcBinaryName));

		/// <summary>WS proxy generator</summary>
		private static String WsdlTool
			=> ToolingEnvironment._wsdlPath ?? (ToolingEnvironment._wsdlPath = ToolingEnvironment.GetMetaDataToolPath(Constant.Programs.WsdlBinaryName));

		/// <summary>WS discovery tool</summary>
		internal static String DiscoTool
			=> ToolingEnvironment._discoPath ?? (ToolingEnvironment._discoPath = ToolingEnvironment.GetMetaDataToolPath(Constant.Programs.DiscoBinaryName));

		/// <summary>WCF Configuration Editor tool</summary>
		/// <exception cref="FileNotFoundException">SVC Config editor not found</exception>
		public static String SvcConfigEditorPath
			=> ToolingEnvironment._svcConfigEditorPath ?? (ToolingEnvironment._svcConfigEditorPath = ToolingEnvironment.GetMetaDataToolPath(Constant.Programs.SvcConfigEditorName));

		internal static String SavedDataBase
		{
			get
			{
				if(!Directory.Exists(ToolingEnvironment.ExpectedDirectory))
					Directory.CreateDirectory(ToolingEnvironment.ExpectedDirectory);
				return ToolingEnvironment.ExpectedDirectory;
			}
		}

		public static Process CreateProcess(ServiceType type, String arguments)
		{
			String filePath;
			switch(type)
			{
			case ServiceType.WS:
				filePath = ToolingEnvironment.WsdlTool;
				break;
			case ServiceType.WCF:
				filePath = ToolingEnvironment.SvcTool;
				break;
			default:
				throw new NotImplementedException();
			}

			return ToolingEnvironment.CreateProcess(filePath, arguments);
		}

		public static Process CreateProcess(String filePath, String arguments)
		{
			Process process = new Process
			{
				StartInfo = new ProcessStartInfo(filePath)
				{
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					Arguments = arguments,
				}
			};
			//process.ErrorDataReceived += new DataReceivedEventHandler(ServiceAnalyzer.Svcutil_ErrorDataReceived);
			process.Start();
			//ServiceAnalyzer.SvcUtilError = new StringBuilder();
			//process.BeginErrorReadLine();
			return process;
		}

		private static String GetMetaDataToolPath(String binaryName)
		{
			foreach(String sdkPath in ToolingEnvironment.GetSdkPath())
			{
				if(!Directory.Exists(sdkPath))
					throw new DirectoryNotFoundException(String.Format("Microsoft SDKs directory {0} not found while searching forr metadata tool {1}", sdkPath, binaryName));

				//Файл может лежать как и папке %SdkPath%\bin\, так и в папке %SdkPath%\bin\NETFX 4.5.1 Tools
				String[] binFiles = Directory.GetFiles(sdkPath, binaryName, SearchOption.AllDirectories);
				if(binFiles.Length != 0)
					return binFiles[0];
			}

			String location = Assembly.GetExecutingAssembly().Location;
			String directoryName = Path.GetDirectoryName(location);

			String path2 = Path.Combine(directoryName, binaryName);
			if(File.Exists(path2))
				return path2;

			String path3 = binaryName;
			if(File.Exists(path3))
				return path3;
			throw new FileNotFoundException($"Metadata Tool {binaryName} not found.");
		}

		private static String[] GetSdkPath()
		{//https://blogs.msdn.microsoft.com/windowssdk/2008/05/28/windows-sdk-registry-keys/
			String[] valueNames = new String[] { "CurrentInstallFolder", "InstallationFolder" };

			//x64 Microsoft SDKs path
			List<String> paths = new List<String>(GetSdkPath("SOFTWARE\\Wow6432Node\\Microsoft\\Microsoft SDKs", valueNames));
			if(paths.Count == 0)//x32 Microsoft SDKs path
				paths = new List<String>(GetSdkPath("SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows", valueNames));
			return paths.ToArray();
		}

		private static IEnumerable<String> GetSdkPath(String regPath, params String[] valueNames)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath);
			if(key == null)
				yield break;

			foreach(String valueName in valueNames)
			{
				String result = NativeMethods.GetRegistryValue(regPath, valueName);
				if(result != null)
				{
					yield return result;
					//yield break;
				}
			}

			foreach(String subKey in key.GetSubKeyNames())
				foreach(String subKeyResult in ToolingEnvironment.GetSdkPath(regPath + "\\" + subKey, valueNames))
					yield return subKeyResult;
		}

		private static String GetRegistryValue(String path, String name)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			if(key == null)
				return null;

			String result = (String)key.GetValue(name, null);
			key.Close();

			return result;
			
		}
	}
}