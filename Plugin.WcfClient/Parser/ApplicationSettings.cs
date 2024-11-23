using System;
using System.IO;

namespace Plugin.WcfClient.Parser
{
	internal class ApplicationSettings
	{//TODO: Этот класс не нужен, он всегда статичен
		private const String DisableRefreshPrompt = "RefreshPrompt : Disabled";
		private const String DisableRegenerateConfig = "RegenerateConfig : Disabled";
		private const String DisableSecurityPrompt = "SecurityPrompt : Disabled";
		private const String EnableRefreshPrompt = "RefreshPrompt : Enabled";
		private const String EnableRegenerateConfig = "RegenerateConfig : Enabled";
		private const String EnableSecurityPrompt = "SecurityPrompt : Enabled";

		private const String projectBaseTitle = "ProjectBase:";
		private static ApplicationSettings instance;
		private String _projectBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp\\Plugin.WcfClient Test Projects");
		
		/// <summary>Базовая директория для сохранения WS\WCF проектов</summary>
		public String ProjectBase
		{
			get
			{
				if(!Directory.Exists(this._projectBase))
					ExceptionUtility.InvokeFSAction(delegate { Directory.CreateDirectory(this._projectBase); });
				return this._projectBase;
			}
		}

		public static ApplicationSettings Instance
			=> ApplicationSettings.instance == null ? ApplicationSettings.instance = new ApplicationSettings() : ApplicationSettings.instance;
	}
}