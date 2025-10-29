using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class ClientEndpointInfo
	{
		[NonSerialized]
		private String _invalidReason;

		private IList<ServiceMethodWrapper> _methods = new List<ServiceMethodWrapper>();

		[NonSerialized]
		private ServiceProject _project;

		internal String InvalidReason
		{
			get => this._invalidReason;
			set => this._invalidReason = value;
		}

		public Boolean Valid { get; internal set; }

		public String ClientTypeName { get; internal set; }

		public String EndpointConfigurationName { get; internal set; }

		public String OperationContractTypeName { get; private set; }

		public String EndpointName
			=> this.EndpointConfigurationName == null
				? this.OperationContractTypeName
				: $"{this.OperationContractTypeName} ({this.EndpointConfigurationName})";

		public String ProxyIdentifier { get; internal set; }

		public IList<ServiceMethodWrapper> Methods => this._methods;

		internal ServiceProject ServiceProject
		{
			get => this._project;
			set => this._project = value;
		}

		public ClientEndpointInfo(String operationContractTypeName)
			=> this.OperationContractTypeName = operationContractTypeName;
	}
}