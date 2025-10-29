using System;
using System.Net;
using System.Reflection;
using System.Web.Services.Protocols;
using Plugin.WcfClient.Parser.Ws;

namespace Plugin.WcfClient.Extensions
{
	internal class WsExtender
	{
		private Object _client;
		public Object Service { get; }
		public Object Client
			=> this._client
					?? (this._client = this.Service.GetType().BaseType.InvokeMember("clientType", BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this.Service, null));

		public Array HighPriExtensions
		{
			get => (Array)this.Client.GetType().InvokeMember("HighPriExtensions", BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this.Client, null);
			set => this.Client.GetType().InvokeMember("HighPriExtensions", BindingFlags.SetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this._client, new Object[] { value, });

		}
		public Array HighPriExtensionInitializers
		{
			get => (Array)this.Client.GetType().InvokeMember("HighPriExtensionInitializers", BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this.Client, null);
			private set => this.Client.GetType().InvokeMember("HighPriExtensionInitializers", BindingFlags.SetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this.Client, new Object[] { value, });
		}

		public static Type SoapReflectedExtensionType
			=> typeof(SoapExtension).Assembly.GetType("System.Web.Services.Protocols.SoapReflectedExtension");

		public ICredentials Credentials
		{
			get => (ICredentials)this.Service.GetType().InvokeMember("Credentials", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, this.Service, null);
			set => this.Service.GetType().InvokeMember("Credentials", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, this.Service, new Object[] { value, });
		}

		public IWebProxy Proxy
		{
			get => (IWebProxy)this.Service.GetType().InvokeMember("Proxy", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, this.Service, null);
			set => this.Service.GetType().InvokeMember("Proxy", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, this.Service, new Object[] { value, });
		}

		public WsExtender(Object service)
			=> this.Service = service?? throw new ArgumentNullException(nameof(service));

		public void InjectExtension()
		{
			if(this.HighPriExtensionInitializers.Length > 0)
				return;//The code has already been implemented

			Type extensionType = WsExtender.SoapReflectedExtensionType;

			Object extensionObj = extensionType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
				null,
				new Type[] { typeof(Type), typeof(SoapExtensionAttribute), typeof(Int32), },
				null
				).Invoke(new Object[] { typeof(WsResponseXmlInterceptor), null, 0, });

			Array extensionArr = Array.CreateInstance(extensionType, 1);
			extensionArr.SetValue(extensionObj, 0);

			//Assigning an extension
			this.HighPriExtensions = extensionArr;
			//this._client.GetType().InvokeMember("HighPriExtensions", BindingFlags.SetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this._client, new Object[] { extensionArr, });

			Object extensionInitArr = extensionType.GetMethod("GetInitializers",
				BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static,
				null,
				new Type[] { typeof(Type), typeof(SoapExtension).Assembly.GetType("System.Web.Services.Protocols.SoapReflectedExtension[]"), },
				null).Invoke(null, new Object[] { this.Service.GetType(), extensionArr, });

			this.HighPriExtensionInitializers = (Array)extensionInitArr;
			//this.Client.GetType().InvokeMember("HighPriExtensionInitializers", BindingFlags.SetField | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance, null, this._client, new Object[] { extensionInitArr, });
		}
	}
}