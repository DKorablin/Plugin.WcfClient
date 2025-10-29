using System;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	internal class AddServiceInputs
	{
		/// <summary>Type of action performed on the service</summary>
		public enum ActionType
		{
			/// <summary>Download the service</summary>
			Download,
			/// <summary>Open the service from disk</summary>
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