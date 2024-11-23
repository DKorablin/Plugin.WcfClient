using System;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	internal class AddServiceInputs
	{
		/// <summary>Тип выполняемого действия над сервисом</summary>
		public enum ActionType
		{
			/// <summary>Скачать сервис</summary>
			Download,
			/// <summary>Открыть сервис с диска</summary>
			Open,
		}
		public SettingsDataSet.TreeRow[] Endpoints { get; private set; }
		public PluginWindows Plugin { get; private set; }
		public ActionType Action { get; private set; }

		public AddServiceInputs(PluginWindows plugin, ActionType action, params SettingsDataSet.TreeRow[] endpoints)
		{
			this.Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
			this.Action = action;
			this.Endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
		}
	}
}