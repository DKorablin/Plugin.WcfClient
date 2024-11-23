using System;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class ServiceInvocationInputs
	{
		[NonSerialized]
		private readonly AppDomain _domain;
		private VariableWrapper[] _inputs;

		public String ClientTypeName { get; private set; }

		public String ContractTypeName { get; private set; }

		public AppDomain Domain => this._domain;

		public String EndpointConfigurationName { get; private set; }

		public String MethodName { get; private set; }

		public String ProxyIdentifier { get; private set; }

		public Boolean StartNewClient { get; private set; }

		public ServiceType Type { get; private set; }

		public String UserName { get; private set; }
		public String Password { get; private set; }
		public String ProxyUserName { get; private set; }
		public String ProxyPassword { get; private set; }

		public Boolean UseProxy => !String.IsNullOrEmpty(this.ProxyUserName) && !String.IsNullOrEmpty(this.ProxyPassword);

		public Boolean UseAuthentication => !String.IsNullOrEmpty(this.UserName) && !String.IsNullOrEmpty(this.Password);

		internal ServiceInvocationInputs(ServiceMethodWrapper method, VariableWrapper[] inputs, Boolean startNewClient)
		{
			ClientEndpointInfo endpoint = method.Endpoint;
			if(endpoint.ServiceProject == null)
				throw new InvalidOperationException("ServiceProject can't be null");

			this.ClientTypeName = endpoint.ClientTypeName;
			this.ContractTypeName = endpoint.OperationContractTypeName;
			this.EndpointConfigurationName = endpoint.EndpointConfigurationName;
			this.ProxyIdentifier = endpoint.ProxyIdentifier;
			this.MethodName = method.MethodName;
			this._inputs = inputs;
			this.StartNewClient = startNewClient;

			//if(endpoint.ServiceProject==null)
			//this.Type = ServiceType.WCF;
			//else
			this._domain = endpoint.ServiceProject.ClientDomain;

			var serviceRow = endpoint.ServiceProject.Info.Row.ServiceRow;
			this.Type = serviceRow.ServiceType;
			this.UserName = serviceRow.Login;
			this.Password = serviceRow.Password;
			this.ProxyUserName = serviceRow.ProxyLogin;
			this.ProxyPassword = serviceRow.ProxyPassword;
		}

		internal VariableWrapper[] GetInputs()
			=> this._inputs;
	}
}