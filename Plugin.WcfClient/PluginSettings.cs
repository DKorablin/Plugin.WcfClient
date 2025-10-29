using System;
using System.ComponentModel;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient
{
	public class PluginSettings
	{
		private readonly PluginWindows _plugin;
		private SettingsBll _serviceSettings;

		[Category("Proxy")]
		[Description("Login for proxy access (if any)")]
		[DisplayName("User Name")]
		public String ProxyUserName { get; set; }

		[Category("Proxy")]
		[Description("Password for proxy access (if any)")]
		[DisplayName("Password")]
		[PasswordPropertyText(true)]
		public String ProxyPassword { get; set; }

		[Category("Data")]
		[DisplayName("Rewrite Configuration")]
		[Description("Rewrite Web.config file when updating existing service instance")]
		[DefaultValue(true)]
		public Boolean RegenerateConfigEnabled { get; set; } = true;

		[Category("UI")]
		[DisplayName("Show SOAP WS Payload")]
		[Description("By default payload is disabled for WebServices. Because WS Client can hang")]
		[DefaultValue(false)]
		public Boolean ShowWsPayload { get; set; }

		[Category("Data")]
		[DisplayName("Auto save input values")]
		[Description("Automatic save input web service parameters and values to files on invoke")]
		[DefaultValue(false)]
		public Boolean SaveInputValues { get; set; }

		/// <summary>Use a proxy to access the internet</summary>
		internal Boolean UseProxy
		=> !String.IsNullOrEmpty(this.ProxyUserName)
		&& !String.IsNullOrEmpty(this.ProxyPassword);

		/// <summary>WCF/WS service settings</summary>
		internal SettingsBll ServiceSettings
			=> this._serviceSettings ?? (this._serviceSettings = new SettingsBll(this._plugin));

		public PluginSettings(PluginWindows plugin)
			=> this._plugin = plugin;
	}
}