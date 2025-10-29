using System;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	public class ServiceInvocationOutputs
	{
		private VariableWrapper[] _serviceInvocationResult;

		internal String ExceptionMessage { get; private set; }

		internal String ExceptionStack { get; private set; }

		internal ExceptionType ExceptionType { get; private set; }

		internal String ResponseXml { get; private set; }

		internal ServiceInvocationOutputs(VariableWrapper[] serviceInvocationResult, String responseXml)
		{
			this._serviceInvocationResult = serviceInvocationResult;
			this.ResponseXml = responseXml;
		}

		internal ServiceInvocationOutputs(ExceptionType exceptionType, String exceptionMessage, String exceptionStack, String responseXml)
		{
			this.ExceptionType = exceptionType;
			this.ExceptionMessage = exceptionMessage;
			this.ExceptionStack = exceptionStack;
			this.ResponseXml = responseXml;
		}

		internal VariableWrapper[] GetServiceInvocationResult()
			=> this._serviceInvocationResult;
	}
}