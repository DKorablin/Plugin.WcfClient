using System;
using System.Collections.Generic;
using System.IO;
using Plugin.WcfClient.Extensions;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class ServiceMethodWrapper
	{
		public ClientEndpointInfo Endpoint { get; private set; }

		internal IList<ServiceMemberWrapper> InputParameters { get; private set; }

		internal Boolean IsOneWay { get; private set; }

		public String MethodName { get; private set; }

		internal IList<ServiceMemberWrapper> OtherParameters { get; private set; }

		internal Boolean Valid
			=> this.InputParameters.Find(w => !this.IsServiceMemberValid(w)) == null
				&& this.OtherParameters.Find(w => !this.IsServiceMemberValid(w)) == null;

		internal ServiceMethodWrapper(ClientEndpointInfo endpoint, String methodName, Boolean isOneWay)
		{
			this.InputParameters = new List<ServiceMemberWrapper>();
			this.OtherParameters = new List<ServiceMemberWrapper>();

			this.Endpoint = endpoint;
			this.MethodName = methodName;
			this.IsOneWay = isOneWay;
		}

		public void SaveData(VariableWrapper[] variables)
		{
			if(!Directory.Exists(this.Endpoint.ServiceProject.Info.ParametersPath))
				Directory.CreateDirectory(this.Endpoint.ServiceProject.Info.ParametersPath);

			String parametersPath = Path.Combine(this.Endpoint.ServiceProject.Info.ParametersPath, this.MethodName + ".xml");
			VariableWrapper.SaveData(parametersPath, variables);
		}

		public void LoadData(VariableWrapper[] variables)
		{
			String parametersPath = Path.Combine(this.Endpoint.ServiceProject.Info.ParametersPath, this.MethodName + ".xml");
			if(File.Exists(parametersPath))
				VariableWrapper.LoadData(parametersPath, variables);
		}

		public VariableWrapper[] GetVariables()
		{
			VariableWrapper[] array = new VariableWrapper[this.InputParameters.Count];
			Int32 num = 0;
			foreach(ServiceMemberWrapper current in this.InputParameters)
			{
				array[num] = new VariableWrapper(current);
				array[num].SetServiceMethodInfo(this);
				num++;
			}
			return array;
		}

		private Boolean IsServiceMemberValid(ServiceMemberWrapper member)
			=> member.IsValid;
	}
}