using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web.Services.Protocols;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Extensions;
using Plugin.WcfClient.Parser.Ws;

namespace Plugin.WcfClient.Parser
{
	public class ServiceExecutor : MarshalByRefObject
	{
		private class ResponseXmlInterceptingBehavior : IEndpointBehavior
		{
			private class ResponseXmlInterceptingInspector : IClientMessageInspector
			{
				private Boolean extractingXML;
				public String InterceptedXml { get; private set; }

				public ResponseXmlInterceptingInspector(Boolean extractingXML)
				{
					this.extractingXML = extractingXML;
				}
				public void AfterReceiveReply(ref Message reply, Object correlationState)
				{
					this.InterceptedXml = reply.ToString();
				}
				public Object BeforeSendRequest(ref Message request, IClientChannel channel)
				{
					if(this.extractingXML)
						throw new ServiceExecutor.StopInvocationException(request.ToString());
					return null;
				}
				public void SetExtractingXML(Boolean value)
				{
					this.extractingXML = value;
				}
			}
			private ServiceExecutor.ResponseXmlInterceptingBehavior.ResponseXmlInterceptingInspector responseXmlInterceptor;
			public String InterceptedXml { get { return this.responseXmlInterceptor.InterceptedXml; } }
			public ResponseXmlInterceptingBehavior(Boolean extractingXML)
				=> this.responseXmlInterceptor = new ServiceExecutor.ResponseXmlInterceptingBehavior.ResponseXmlInterceptingInspector(extractingXML);

			public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
			{ }

			public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
				=> clientRuntime.MessageInspectors.Add(this.responseXmlInterceptor);

			public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
			{ }

			public void SetExtractingXML(Boolean value)
				=> this.responseXmlInterceptor.SetExtractingXML(value);

			public void Validate(ServiceEndpoint endpoint)
			{ }
		}

		internal class StopInvocationException : Exception
		{
			internal StopInvocationException(String message)
				: base(message)
			{ }
		}

		private static IDictionary<String, Object> CachedProxies = new Dictionary<String, Object>();
		private static Object _lockObject = new Object();
		private Boolean _isExtractingXml;
		/// <summary>Используется при вытаскивании только GET запроса от WebService'а</summary>
		private Boolean IsExtractingXml
		{
			get => this._isExtractingXml;
			set => this._isExtractingXml = WsResponseXmlInterceptor.IsExtractingXml = value;
		}

		public ServiceExecutor()
			=> this.IsExtractingXml = false;

		internal static ServiceInvocationOutputs ExecuteInClientDomain(ServiceInvocationInputs inputs)
		{
			ServiceExecutor serviceExecutor = (ServiceExecutor)inputs.Domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
			return serviceExecutor.Execute(inputs);
		}
		internal static String TranslateToXmlInClientDomain(ServiceInvocationInputs inputs)
		{
			AppDomain clientDomain = inputs.Domain;
			ServiceExecutor serviceExecutor = (ServiceExecutor)clientDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
			return serviceExecutor.TranslateToXml(inputs);
		}
		internal static IList<Int32> ValidateDictionary(VariableWrapper variable, AppDomain domain)
		{
			ServiceExecutor serviceExecutor = (ServiceExecutor)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(ServiceExecutor).FullName);
			return serviceExecutor.ValidateDictionary(variable);
		}
		internal void ConstructClientToCache(ServiceType type, String proxyIdentifier, String clientTypeName, String endpointConfigurationName)
		{
			Object value = ServiceExecutor.ConstructClient(clientTypeName, endpointConfigurationName);

			if(ServiceExecutor.CachedProxies.ContainsKey(proxyIdentifier))
				this.DeleteClient(proxyIdentifier);

			switch(type)
			{//Pre intit work
			case ServiceType.WS://TODO: Сюда необходимо пронести ProxyUserName и ProxyPassword из настроек
				value.GetType().InvokeMember("Proxy", BindingFlags.SetProperty, null, value, new Object[] { new System.Net.WebProxy() { UseDefaultCredentials = true, }, });
				break;
			}
			ServiceExecutor.CachedProxies.Add(proxyIdentifier, value);
		}
		internal void DeleteClient(String proxyIdentifier)
		{
			if(ServiceExecutor.CachedProxies.ContainsKey(proxyIdentifier))
			{
				Object client = ServiceExecutor.CachedProxies[proxyIdentifier];
				ICommunicationObject wcfClient = client as ICommunicationObject;
				SoapHttpClientProtocol wsClient = client as SoapHttpClientProtocol;
				if(wcfClient != null)
					ServiceExecutor.CloseClient(wcfClient);
				else if(wsClient != null)
					ServiceExecutor.CloseClient(wsClient);
				else if(client == null)
					throw new ArgumentNullException("client");
				else
					throw new NotImplementedException(client.ToString());

				ServiceExecutor.CachedProxies.Remove(proxyIdentifier);
			}
		}
		private static void CloseClient(ICommunicationObject client)
		{
			try
			{
				client.Close();
			} catch(TimeoutException)
			{
				client.Abort();
			} catch(CommunicationException)
			{
				client.Abort();
			}
		}
		private static void CloseClient(SoapHttpClientProtocol client)
		{
			try
			{
				client.Abort();
			} finally
			{
				client.Dispose();
			}
		}
		private static Object ConstructClient(String clientTypeName, String endpointConfigurationName)
		{
			Object result;
			Type type = ClientSettings.ClientAssembly.GetType(clientTypeName);

			if(String.IsNullOrEmpty(endpointConfigurationName))//HACK: WS
				result = type.GetConstructor(Type.EmptyTypes).Invoke(null);
			else
				result = type.GetConstructor(new Type[] { typeof(String), }).Invoke(new Object[] { endpointConfigurationName });

			return result;
		}
		private static void PopulateInputParameters(String methodName, VariableWrapper[] inputs, Type contractType, out MethodInfo method, out ParameterInfo[] parameters, out Object[] parameterArray)
		{
			method = contractType.GetMethod(methodName);
			parameters = method.GetParameters();
			parameterArray = new Object[parameters.Length];
			IDictionary<String, Object> dictionary = DataContractAnalyzer.BuildParameters(inputs);
			Int32 count = 0;
			foreach(ParameterInfo parameter in parameters)
			{
				if(parameter.IsIn || !parameter.IsOut)
					parameterArray[count] = dictionary[parameter.Name];
				count++;
			}
		}
		private ServiceInvocationOutputs Execute(ServiceInvocationInputs inputValues)
		{
			lock(_lockObject)
			{
				String clientTypeName = inputValues.ClientTypeName;
				String contractTypeName = inputValues.ContractTypeName;
				String endpointConfigurationName = inputValues.EndpointConfigurationName;
				String methodName = inputValues.MethodName;
				VariableWrapper[] inputs = inputValues.GetInputs();
				Type type = ClientSettings.ClientAssembly.GetType(contractTypeName);
				MethodInfo methodInfo;
				ParameterInfo[] prmsType;
				Object[] prmsValues;
				try
				{
					ServiceExecutor.PopulateInputParameters(methodName, inputs, type, out methodInfo, out prmsType, out prmsValues);
				} catch(TargetInvocationException ex)
				{
					return new ServiceInvocationOutputs(ExceptionType.InvalidInput, ex.InnerException.Message, null, null);
				}

				if(inputValues.StartNewClient || !ServiceExecutor.CachedProxies.ContainsKey(inputValues.ProxyIdentifier))
				{
					try
					{
						this.ConstructClientToCache(inputValues.Type, inputValues.ProxyIdentifier, clientTypeName, endpointConfigurationName);
					} catch(TargetInvocationException exc)
					{
						return new ServiceInvocationOutputs(ExceptionType.ProxyConstructFail, exc.InnerException.Message, exc.InnerException.StackTrace, null);
					}
				}

				Object serviceObj = ServiceExecutor.CachedProxies[inputValues.ProxyIdentifier];

				Type baseType = serviceObj.GetType().BaseType;
				ServiceExecutor.ResponseXmlInterceptingBehavior responseWcfXml = null;
				switch(inputValues.Type)
				{//Inject service XML monitor
				case ServiceType.WCF:
					{
						WcfExtender extender = new WcfExtender(serviceObj);
						//PropertyInfo property = baseType.GetProperty("Endpoint");
						ServiceEndpoint serviceEndpoint = extender.Endpoint;//(ServiceEndpoint)property.GetValue(serviceObj, null);
						if(!serviceEndpoint.Behaviors.Contains(typeof(ServiceExecutor.ResponseXmlInterceptingBehavior)))
						{
							responseWcfXml = new ServiceExecutor.ResponseXmlInterceptingBehavior(this.IsExtractingXml);
							serviceEndpoint.Behaviors.Add(responseWcfXml);
						} else
						{
							responseWcfXml = serviceEndpoint.Behaviors.Find<ServiceExecutor.ResponseXmlInterceptingBehavior>();
							responseWcfXml.SetExtractingXML(this.IsExtractingXml);
						}
					}
					break;
				case ServiceType.WS:
					{
						WsExtender extender = new WsExtender(serviceObj);
						extender.InjectExtension();
						if(inputValues.UseAuthentication)//TODO: Если объект уже закеширован, то Credentials выставлять не надо
							extender.Credentials = new NetworkCredential(inputValues.UserName, inputValues.Password);
						//type.InvokeMember("Credentials", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, null, serviceObj, new Object[] { credentials, });

						if(inputValues.UseProxy)
							extender.Proxy = new WebProxy() { Credentials = new NetworkCredential(inputValues.ProxyUserName, inputValues.ProxyPassword), };
						//type.InvokeMember("Proxy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, null, serviceObj, new Object[] { proxy, });
					}
					break;
				}

				Object result = null;
				try
				{
					result = methodInfo.Invoke(serviceObj, prmsValues);
				} catch(TargetInvocationException ex2)
				{
					if(ExceptionUtility.IsFatal(ex2.InnerException))
						throw;

					/*if(inputValues.Type == ServiceAnalyzer.ServiceType.WS)
					{//TODO: После 2х запросов XML GET параметров, WS сервис повисает.
						//((SoapHttpClientProtocol)serviceObj).Abort();
						this.DeleteClient(inputValues.ProxyIdentifier);
					}*/

					return new ServiceInvocationOutputs(ExceptionType.InvokeFail,
						ex2.InnerException.Message,
						ex2.InnerException.StackTrace,
						inputValues.Type == ServiceType.WCF ? responseWcfXml.InterceptedXml : WsResponseXmlInterceptor.XmlRequest);
				}
				IDictionary<String, Object> dictionary = new Dictionary<String, Object>();
				Int32 num = 0;
				ParameterInfo[] array3 = prmsType;
				for(Int32 i = 0; i < array3.Length; i++)
				{
					ParameterInfo parameterInfo = array3[i];
					if(parameterInfo.ParameterType.IsByRef)
					{
						Object obj4 = prmsValues[num];
						if(obj4 == null)
							obj4 = new NullObject();
						dictionary.Add(parameterInfo.Name, obj4);
					}
					num++;
				}
				if(result == null)
					result = new NullObject();

				return new ServiceInvocationOutputs(DataContractAnalyzer.BuildVariables(inputValues.Type, result, dictionary),
					inputValues.Type == ServiceType.WCF ? responseWcfXml.InterceptedXml : WsResponseXmlInterceptor.XmlResponse);
			}
		}
		private String TranslateToXml(ServiceInvocationInputs inputs)
		{
			this.IsExtractingXml = true;
			ServiceInvocationOutputs serviceInvocationOutputs = this.Execute(inputs);
			return serviceInvocationOutputs.ExceptionMessage;
		}
		/*private String TranslateToXml(TestCase testCase, VariableInfo[] inputs)
		{
			this.extractingXML = true;
			ServiceInvocationOutputs serviceInvocationOutputs = this.Execute(new ServiceInvocationInputs(inputs, testCase, false));
			return serviceInvocationOutputs.ExceptionMessage;
		}*/
		private IList<Int32> ValidateDictionary(VariableWrapper variable)
			=> variable.ValidateDictionary();
	}
}