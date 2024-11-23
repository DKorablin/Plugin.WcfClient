using System;
using System.Reflection;
using System.ServiceModel.Description;

namespace Plugin.WcfClient.Extensions
{
	internal class WcfExtender
	{
		private readonly Object _service;

		public ServiceEndpoint Endpoint
		{
			get => (ServiceEndpoint)this._service.GetType().InvokeMember("Endpoint", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public, null, this._service, null);
			set => this._service.GetType().InvokeMember("Endpoint", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public, null, this._service, new Object[] { value, });
		}

		public WcfExtender(Object service)
			=> this._service = service?? throw new ArgumentNullException(nameof(service));
	}
}