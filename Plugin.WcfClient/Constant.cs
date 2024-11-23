using System;

namespace Plugin.WcfClient
{
	internal static class Constant
	{
		public static class Settings
		{
			public const String TestAssembliesFolderName = "Test Client Projects";
			/// <summary>Наименование файла, в котором сохраняются все настройки по проекту</summary>
			public const String ServiceFileName = "WcfSettings.xml";
		}

		internal static class Plugin
		{
			/// <summary>Плагин браузера</summary>
			public const String Browser = "7476853a-3a40-4d5f-a5b5-a00f1dc4d24c";
		}

		public static class PluginType
		{
			/// <summary>Документ браузера</summary>
			public const String BrowserDocument = "Plugin.Browser.DocumentBrowser";
		}

		public static class Programs
		{
			public const String SvcConfigEditorName = "SvcConfigEditor.exe";
			public const String SvcBinaryName = "svcutil.exe";
			public const String WsdlBinaryName = "wsdl.exe";
			public const String DiscoBinaryName = "disco.exe";
		}
	}
}