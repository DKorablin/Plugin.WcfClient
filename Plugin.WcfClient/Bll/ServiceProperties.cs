using System;
using System.ComponentModel;

namespace Plugin.WcfClient.Bll
{
	internal class ServiceProperties
	{
		private readonly PluginWindows _plugin;
		private readonly SettingsDataSet.TreeRow _row;

		[Category("General")]
		[Description("Тип сервиса WCF/WebService")]
		public ServiceType Type
		{
			get => this._row.ServiceRow.ServiceType;
			set
			{
				if(this._row.ServiceRow.ServiceType != value)
				{
					this._row.ServiceRow.ServiceType = value;
					this.Save();
				}
			}
		}

		[Category("General")]
		[DisplayName("Rewrite Configuration")]
		[Description("Rewrite Web.config file when updating existing service instance")]
		[DefaultValue(true)]
		public Boolean RegenerateConfigEnabled
		{
			get => this._row.ServiceRow.RegenerateConfigEnabled;
			set
			{
				if(this._row.ServiceRow.RegenerateConfigEnabled != value)
				{
					this._row.ServiceRow.RegenerateConfigEnabled = value;
					this.Save();
				}
			}
		}

		[Category("Network")]
		[Description("Путь к сервису на удалённой машине")]
		public String Address
		{
			get => this._row.Name;
			set
			{
				if(this._row.Name != value && !String.IsNullOrEmpty(value))
				{
					this._row.Name = value;
					this.Save();
				}
			}
		}

		[Category("Network")]
		[DisplayName("Login")]
		[Description("Логин для авторизации")]
		public String Login
		{
			get => this._row.ServiceRow.Login;
			set
			{
				if(this._row.ServiceRow.Login != value)
				{
					this._row.ServiceRow.Login = String.IsNullOrEmpty(value) ? null : value;
					this.Save();
				}
			}
		}

		[Category("Network")]
		[DisplayName("Password")]
		[Description("Пароль для авторизации")]
		[PasswordPropertyText(true)]
		public String Password
		{
			get => this._row.ServiceRow.Password;
			set
			{
				if(this._row.ServiceRow.Password != value)
				{
					this._row.ServiceRow.Password = String.IsNullOrEmpty(value) ? null : value;
					this.Save();
				}
			}
		}

		[Category("Proxy")]
		[DisplayName("Login")]
		[Description("Логин для авторизации на прокси сервере")]
		public String ProxyLogin
		{
			get => this._row.ServiceRow.ProxyLogin;
			set
			{
				if(this._row.ServiceRow.ProxyLogin != value)
				{
					this._row.ServiceRow.ProxyLogin = String.IsNullOrEmpty(value) ? null : value;
					this.Save();
				}
			}
		}

		[Category("Proxy")]
		[DisplayName("Password")]
		[Description("Пароль для авторизации на прокси сервере")]
		[PasswordPropertyText(true)]
		public String ProxyPassword
		{
			get => this._row.ServiceRow.ProxyPassword;
			set
			{
				if(this._row.ServiceRow.ProxyPassword != value)
				{
					this._row.ServiceRow.ProxyPassword = String.IsNullOrEmpty(value) ? null : value;
					this.Save();
				}
			}
		}

		public ServiceProperties(PluginWindows plugin, SettingsDataSet.TreeRow row)
		{
			this._plugin = plugin;
			this._row = row;
		}

		private void Save()
		{
			this._plugin.Settings.ServiceSettings.Save();
			this._plugin.OnServicePropertiesChanged(this._row);
		}
	}
}