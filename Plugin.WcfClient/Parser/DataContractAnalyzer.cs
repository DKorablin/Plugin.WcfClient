using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Extensions;

namespace Plugin.WcfClient.Parser
{
	public class DataContractAnalyzer : MarshalByRefObject,IDisposable
	{
		private static readonly Type[] memberAttributes = new Type[]
		{
			typeof(DataMemberAttribute),
			typeof(MessageBodyMemberAttribute),
			typeof(MessageHeaderAttribute),
			typeof(MessageHeaderArrayAttribute),
			typeof(XmlAttributeAttribute),
			typeof(XmlElementAttribute),
			typeof(XmlArrayAttribute),
			typeof(XmlTextAttribute)
		};
		private static readonly IDictionary<StringPair, ServiceMemberWrapper> sharedMembers = new Dictionary<StringPair, ServiceMemberWrapper>();
		private static readonly IDictionary<String, ServiceTypeWrapper> sharedTypes = new Dictionary<String, ServiceTypeWrapper>();
		private static readonly Type[] typeAttributes = new Type[]
		{
			typeof(DataContractAttribute),
			typeof(XmlTypeAttribute),
			typeof(MessageContractAttribute)
		};
		public DataContractAnalyzer()
		{
			//HACK: ReflectionOlnyLoad
			//AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
		}
		public void Dispose()
		{
			//HACK: ReflectionOlnyLoad
			//AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
		}

		Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(Object sender, ResolveEventArgs args)
		{
			Assembly loadedAssembly = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().FirstOrDefault(asm => String.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

			if(loadedAssembly != null)
				return loadedAssembly;

			AssemblyName assemblyName = new AssemblyName(args.Name);
			//String dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

			//if(File.Exists(dependentAssemblyFilename))
				//return Assembly.ReflectionOnlyLoadFrom(dependentAssemblyFilename);
			//else
				return Assembly.ReflectionOnlyLoad(args.Name);
		}

		internal static IDictionary<String, Object> BuildParameters(VariableWrapper[] inputs)
		{
			IDictionary<String, Object> result = new Dictionary<String, Object>();

			foreach(VariableWrapper info in inputs)
			{
				Object obj = info.GetObject();
				result.Add(info.Name, obj);
			}
			return result;
		}
		internal static VariableWrapper[] BuildVariables(ServiceType svcType, Object result, IDictionary<String, Object> outValues)
		{
			Queue<Object> queue = new Queue<Object>();
			queue.Enqueue(result);
			foreach(Object current in outValues.Values)
				if(!queue.Contains(current))
					queue.Enqueue(current);

			IDictionary<Object, IList<StringObjectPair>> objGraph = DataContractAnalyzer.BuildObjectGraph(svcType, queue);
			IDictionary<StringObjectPair, VariableWrapper> dictionary2 = new Dictionary<StringObjectPair, VariableWrapper>();
			foreach(IList<StringObjectPair> current2 in objGraph.Values)
				foreach(StringObjectPair current3 in current2)
					DataContractAnalyzer.BuildVariable(dictionary2, current3);

			IList<VariableWrapper> list = new List<VariableWrapper>
			{
				DataContractAnalyzer.BuildVariable(dictionary2, new StringObjectPair("(return)", result))
			};

			foreach(KeyValuePair<String, Object> current4 in outValues)
				list.Add(DataContractAnalyzer.BuildVariable(dictionary2, new StringObjectPair(current4.Key, current4.Value)));

			foreach(KeyValuePair<StringObjectPair, VariableWrapper> current5 in dictionary2)
			{
				IList<StringObjectPair> list2 = objGraph[current5.Key.ObjectValue];
				VariableWrapper[] variables = new VariableWrapper[list2.Count];
				for(Int32 loop = 0;loop < variables.Length;loop++)
					variables[loop] = dictionary2[list2[loop]];
				current5.Value.SetChildVariables(variables);
			}
			VariableWrapper[] array2 = new VariableWrapper[list.Count];
			list.CopyTo(array2, 0);
			return array2;
		}

		internal ClientEndpointInfo AnalyzeWebService()
		{
			ClientEndpointInfo result = null;
			IDictionary<StringPair, ServiceMemberWrapper> dictionary = new Dictionary<StringPair, ServiceMemberWrapper>();
			IDictionary<String, ServiceTypeWrapper> dictionary2 = new Dictionary<String, ServiceTypeWrapper>();

			Assembly assembly = ClientSettings.ClientAssembly;
			Type[] types = assembly.GetTypes().Where(p=>p.ContainsCustomAttribute(typeof(WebServiceBindingAttribute),false)).ToArray();
			if(types.Length == 0)
				throw new InvalidOperationException(String.Format("WebService client assembly '{0}' does not contain types with attribute '{1}'", assembly.FullName, typeof(WebServiceBindingAttribute).Name));

			foreach(Type type in types)
			{
				result = new ClientEndpointInfo(type.Name);

				Queue<Type> queue = DataContractAnalyzer.ExtractContractTypes(type, typeof(SoapDocumentMethodAttribute), out String clientTypeName);
				result.ClientTypeName = clientTypeName;
				queue.Enqueue(typeof(NullObject));

				IDictionary<String, String[]> dictionary3 = new Dictionary<String, String[]>();
				IDictionary<String, TypeProperty> dictionary4 = new Dictionary<String, TypeProperty>();
				TypeGraph typeGraph = DataContractAnalyzer.BuildTypeGraph(ServiceType.WS, queue, dictionary3, dictionary4);
				Queue<String> queue2 = new Queue<String>();
				foreach(String current in typeGraph.Types)
				{
					ServiceTypeWrapper serviceTypeInfo = new ServiceTypeWrapper(current, new List<ServiceMemberWrapper>(), dictionary4[current], dictionary3[current]);
					if(!serviceTypeInfo.IsValid)
						queue2.Enqueue(current);
					if(!dictionary2.ContainsKey(current))
						dictionary2.Add(current, serviceTypeInfo);
				}

				IDictionary<String, IList<String>> dictionary5 = typeGraph.Reverse();
				IDictionary<String, Object> dictionary6 = new Dictionary<String, Object>();
				while(queue2.Count > 0)
				{
					String key = queue2.Dequeue();
					if(!dictionary6.ContainsKey(key))
					{
						dictionary6.Add(key, null);
						dictionary2[key].MarkAsInvalid();
						foreach(String current2 in dictionary5[key])
							queue2.Enqueue(current2);
					}
				}

				typeGraph.PopulateKnownTypeLinks(dictionary2);
				foreach(List<StringPair> list in typeGraph.Links)
					foreach(StringPair item in list)
						if(!dictionary.ContainsKey(item))
							dictionary.Add(item, new ServiceMemberWrapper(item.String1, dictionary2[item.String2]));

				foreach(String graphType in typeGraph.Types)
					if(!graphType.Equals(typeof(NullObject).FullName))
					{
						ServiceTypeWrapper serviceTypeInfo2 = dictionary2[graphType];
						foreach(StringPair linkByType in typeGraph.GetLinksByType(graphType))
							serviceTypeInfo2.AddToMemebers(dictionary[linkByType]);
					}

				MethodInfo[] methods = type.GetMethods();
				for(Int32 i = 0; i < methods.Length; i++)
				{
					MethodInfo methodInfo = methods[i];
					if(!methodInfo.ContainsCustomAttribute(typeof(SoapDocumentMethodAttribute), false))
						continue;

					Boolean isOneWay = false;
					/*OperationContractAttribute[] operationAttribute = methodInfo.GetCustomAttributes<OperationContractAttribute>(false);
					if(operationAttribute.Length == 1 && operationAttribute[0].IsOneWay)
						isOneWay = true;*/

					ServiceMethodWrapper serviceMethodInfo = new ServiceMethodWrapper(result, methodInfo.Name, isOneWay);
					result.Methods.Add(serviceMethodInfo);
					ParameterInfo[] parameters = methodInfo.GetParameters();
					for(Int32 j = 0; j < parameters.Length; j++)
					{
						ParameterInfo parameterInfo = parameters[j];
						String name = parameterInfo.Name;
						String fullName = parameterInfo.ParameterType.FullName;
						if(parameterInfo.ParameterType.IsByRef)
							fullName = parameterInfo.ParameterType.GetElementType().FullName;

						ServiceMemberWrapper serviceMemberInfo = DataContractAnalyzer.GetServiceMemberInfo(dictionary, dictionary2, name, fullName);
						if(parameterInfo.IsIn || !parameterInfo.IsOut)
							serviceMethodInfo.InputParameters.Add(serviceMemberInfo);
						else
							serviceMethodInfo.OtherParameters.Add(serviceMemberInfo);
					}
					if(methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
					{
						String parameterName = "(return)";
						String fullName2 = methodInfo.ReturnParameter.ParameterType.FullName;
						serviceMethodInfo.OtherParameters.Add(DataContractAnalyzer.GetServiceMemberInfo(dictionary, dictionary2, parameterName, fullName2));
					}
				}
				foreach(KeyValuePair<String, ServiceTypeWrapper> current6 in dictionary2)
					if(!DataContractAnalyzer.sharedTypes.ContainsKey(current6.Key))
						DataContractAnalyzer.sharedTypes.Add(current6.Key, current6.Value);

				foreach(KeyValuePair<StringPair, ServiceMemberWrapper> current7 in dictionary)
					if(!DataContractAnalyzer.sharedMembers.ContainsKey(current7.Key))
						DataContractAnalyzer.sharedMembers.Add(current7.Key, current7.Value);

				return result;
			}

			throw new ApplicationException();
		}

		internal ClientEndpointInfo AnalyzeDataContract(ClientEndpointInfo endpoint)
		{
			IDictionary<StringPair, ServiceMemberWrapper> dictionary = new Dictionary<StringPair, ServiceMemberWrapper>();
			IDictionary<String, ServiceTypeWrapper> dictionary2 = new Dictionary<String, ServiceTypeWrapper>();

			Assembly clientAssembly = ClientSettings.ClientAssembly;
			Type type = clientAssembly.GetType(endpoint.OperationContractTypeName);
			if(type == null)
			{
				endpoint.Valid = false;
				return endpoint;
			}
			ServiceContractAttribute[] contractAttribute = type.GetCustomAttributes2<ServiceContractAttribute>(true);
			endpoint.Valid = contractAttribute == null || contractAttribute.Length == 0 || contractAttribute[0].CallbackContract == null;

			Queue<Type> queue = DataContractAnalyzer.ExtractContractTypes(type, null, out String clientTypeName);
			endpoint.ClientTypeName = clientTypeName;
			queue.Enqueue(typeof(NullObject));

			IDictionary<String, String[]> dictionary3 = new Dictionary<String, String[]>();
			IDictionary<String, TypeProperty> dictionary4 = new Dictionary<String, TypeProperty>();
			TypeGraph typeGraph = DataContractAnalyzer.BuildTypeGraph(ServiceType.WCF, queue, dictionary3, dictionary4);
			Queue<String> queue2 = new Queue<String>();
			foreach(String current in typeGraph.Types)
			{
				ServiceTypeWrapper serviceTypeInfo = new ServiceTypeWrapper(current, new List<ServiceMemberWrapper>(), dictionary4[current], dictionary3[current]);
				if(!serviceTypeInfo.IsValid)
					queue2.Enqueue(current);
				if(!dictionary2.ContainsKey(current))
					dictionary2.Add(current, serviceTypeInfo);
			}

			IDictionary<String, IList<String>> dictionary5 = typeGraph.Reverse();
			IDictionary<String, Object> dictionary6 = new Dictionary<String, Object>();
			while(queue2.Count > 0)
			{
				String key = queue2.Dequeue();
				if(!dictionary6.ContainsKey(key))
				{
					dictionary6.Add(key, null);
					dictionary2[key].MarkAsInvalid();
					foreach(String current2 in dictionary5[key])
						queue2.Enqueue(current2);
				}
			}

			typeGraph.PopulateKnownTypeLinks(dictionary2);
			foreach(List<StringPair> list in typeGraph.Links)
				foreach(StringPair current3 in list)
					if(!dictionary.ContainsKey(current3))
						dictionary.Add(current3, new ServiceMemberWrapper(current3.String1, dictionary2[current3.String2]));

			foreach(String graphType in typeGraph.Types)
				if(!graphType.Equals(typeof(NullObject).FullName))
				{
					ServiceTypeWrapper serviceTypeInfo2 = dictionary2[graphType];
					foreach(StringPair current5 in typeGraph.GetLinksByType(graphType))
						serviceTypeInfo2.AddToMemebers(dictionary[current5]);
				}

			MethodInfo[] methods = type.GetMethods();
			for(Int32 i = 0; i < methods.Length; i++)
			{
				MethodInfo methodInfo = methods[i];
				Boolean isOneWay = false;
				OperationContractAttribute[] operationAttribute = methodInfo.GetCustomAttributes2<OperationContractAttribute>(false);
				if(operationAttribute.Length == 1 && operationAttribute[0].IsOneWay)
					isOneWay = true;

				ServiceMethodWrapper serviceMethodInfo = new ServiceMethodWrapper(endpoint, methodInfo.Name, isOneWay);
				endpoint.Methods.Add(serviceMethodInfo);
				ParameterInfo[] parameters = methodInfo.GetParameters();
				for(Int32 j = 0; j < parameters.Length; j++)
				{
					ParameterInfo parameterInfo = parameters[j];
					String name = parameterInfo.Name;
					String fullName = parameterInfo.ParameterType.FullName;
					if(parameterInfo.ParameterType.IsByRef)
						fullName = parameterInfo.ParameterType.GetElementType().FullName;

					ServiceMemberWrapper serviceMemberInfo = DataContractAnalyzer.GetServiceMemberInfo(dictionary, dictionary2, name, fullName);
					if(parameterInfo.IsIn || !parameterInfo.IsOut)
						serviceMethodInfo.InputParameters.Add(serviceMemberInfo);
					else
						serviceMethodInfo.OtherParameters.Add(serviceMemberInfo);
				}
				if(methodInfo.ReturnType != null && !methodInfo.ReturnType.Equals(typeof(void)))
				{
					String parameterName = "(return)";
					String fullName2 = methodInfo.ReturnParameter.ParameterType.FullName;
					serviceMethodInfo.OtherParameters.Add(DataContractAnalyzer.GetServiceMemberInfo(dictionary, dictionary2, parameterName, fullName2));
				}
			}
			foreach(KeyValuePair<String, ServiceTypeWrapper> current6 in dictionary2)
				if(!DataContractAnalyzer.sharedTypes.ContainsKey(current6.Key))
					DataContractAnalyzer.sharedTypes.Add(current6.Key, current6.Value);

			foreach(KeyValuePair<StringPair, ServiceMemberWrapper> current7 in dictionary)
				if(!DataContractAnalyzer.sharedMembers.ContainsKey(current7.Key))
					DataContractAnalyzer.sharedMembers.Add(current7.Key, current7.Value);
			return endpoint;
		}

		private static void AddMemberValueToList(Queue<Object> objects, IDictionary<Object, IList<StringObjectPair>> objectGraph, IList<StringObjectPair> subObjects, Object currentMemberValue, String name)
		{
			if(currentMemberValue == null)
				currentMemberValue = new NullObject();

			subObjects.Add(new StringObjectPair(name, currentMemberValue));
			if(!objectGraph.ContainsKey(currentMemberValue) && !objects.Contains(currentMemberValue))
				objects.Enqueue(currentMemberValue);
		}

		private static IDictionary<Object, IList<StringObjectPair>> BuildObjectGraph(ServiceType svcType, Queue<Object> objects)
		{
			IDictionary<Object, IList<StringObjectPair>> result = new Dictionary<Object, IList<StringObjectPair>>();
			while(objects.Count > 0)
			{
				Object obj = objects.Dequeue();
				IList<StringObjectPair> list = new List<StringObjectPair>();
				result.Add(obj, list);
				Type type = obj.GetType();
				if(type.IsBclType())//HACK: Исправление для параметров WebService'ов
					continue;//Если дошли до базового типа, то продолжать разбирать бесполезно
				if(type.IsArray)
				{
					Array array = (Array)obj;
					Int32 index = 0;
					foreach(Object value in array)
						DataContractAnalyzer.AddMemberValueToList(objects, result, list, value, "[" + index++ + "]");
				} else if(type.IsCollectionType())
				{
					ICollection collection = (ICollection)obj;
					Int32 index = 0;
					foreach(Object value in collection)
						DataContractAnalyzer.AddMemberValueToList(objects, result, list, value, "[" + index++ + "]");
				} else if(type.IsDictionaryType())
				{//Разбор словаря
					IDictionary dictionary2 = (IDictionary)obj;
					Int32 index = 0;
					foreach(Object value in dictionary2)
						DataContractAnalyzer.AddMemberValueToList(objects, result, list, value, "[" + index++ + "]");
				} else
				{//Разбор свойств и пропертей объекта
					foreach(PropertyInfo property in type.GetProperties())
						if(property.CanRead && property.GetIndexParameters().Length == 0)
						{
							if(svcType == ServiceType.WS
								|| DataContractAnalyzer.IsSupportedMember(property)
								|| obj is DictionaryEntry
								|| type.IsKeyValuePairType())
							{
								Object value = property.GetValue(obj, null);
								DataContractAnalyzer.AddMemberValueToList(objects, result, list, value, property.Name);
							}
						}
					foreach(FieldInfo field in type.GetFields())
						if(svcType == ServiceType.WS || DataContractAnalyzer.IsSupportedMember(field))
						{
							Object value = field.GetValue(obj);
							DataContractAnalyzer.AddMemberValueToList(objects, result, list, value, field.Name);
						}
				}
			}
			return result;
		}

		private static TypeGraph BuildTypeGraph(ServiceType serviceType, Queue<Type> rootTypes, IDictionary<String, String[]> enumChoices, IDictionary<String, TypeProperty> typeProperties)
		{//serviceType необходим для WebService, где дочерние элементы входящих объектов не имеют атрибутов: Method(Element { Property get { Value { get {0} } })
			IDictionary<String, IList<StringPair>> dictionary = new Dictionary<String, IList<StringPair>>();
			IDictionary<String, IList<String>> dictionary2 = new Dictionary<String, IList<String>>();
			while(rootTypes.Count > 0)
			{
				Type type = rootTypes.Dequeue();
				String fullName = type.FullName;
				if(!dictionary.ContainsKey(fullName))
				{
					TypeProperty typeProperty = new TypeProperty();
					typeProperties.Add(fullName, typeProperty);
					enumChoices.Add(fullName, null);
					IList<StringPair> list = new List<StringPair>();
					dictionary.Add(fullName, list);
					IList<String> list2 = new List<String>();
					dictionary2.Add(fullName, list2);
					if(type.IsArray)
					{
						String @string = fullName.Substring(0, fullName.Length - 2);
						list.Add(new StringPair("[0]", @string));
						typeProperty.IsArray = true;
						rootTypes.Enqueue(type.GetElementType());
					} else if(type.IsEnum)
					{
						enumChoices[fullName] = Enum.GetNames(type);
					} else if(type.IsNullableType())
					{
						Type[] genericArguments = type.GetGenericArguments();
						list.Add(new StringPair("Value", genericArguments[0].FullName));
						typeProperty.IsNullable = true;
						rootTypes.Enqueue(genericArguments[0]);
					} else if(type.IsCollectionType())
					{
						Type baseType = type.BaseType;
						if(baseType.IsGenericType)
						{
							Type[] genericArguments2 = baseType.GetGenericArguments();
							list.Add(new StringPair("[0]", genericArguments2[0].FullName));
							typeProperty.IsCollection = true;
							rootTypes.Enqueue(genericArguments2[0]);
						}
					} else if(type.IsDictionaryType())
					{
						Type[] genericArguments3 = type.GetGenericArguments();
						Type typeFromHandle = typeof(KeyValuePair<,>);
						Type type2 = typeFromHandle.MakeGenericType(new Type[]
						{
							genericArguments3[0],
							genericArguments3[1]
						});

						list.Add(new StringPair("[0]", type2.FullName));
						typeProperty.IsDictionary = true;
						if(!TypeStrategy.typesCache.ContainsKey(fullName))
							TypeStrategy.typesCache.Add(fullName, type);
						rootTypes.Enqueue(type2);
					} else if(type.IsKeyValuePairType())
					{
						Type[] genericArguments4 = type.GetGenericArguments();
						list.Add(new StringPair("Key", genericArguments4[0].FullName));
						list.Add(new StringPair("Value", genericArguments4[1].FullName));
						typeProperty.IsKeyValuePair = true;
						if(!TypeStrategy.typesCache.ContainsKey(fullName))
							TypeStrategy.typesCache.Add(fullName, type);

						rootTypes.Enqueue(genericArguments4[0]);
						rootTypes.Enqueue(genericArguments4[1]);
					} else if(type.IsBclType())
					{//TODO: Хак для WS сервисов. Чтобы не разбирать базовые типы
						continue;
					} else if(serviceType == ServiceType.WS || DataContractAnalyzer.IsSupportedType(type))
					{
						typeProperty.IsComposite = true;
						if(type.IsValueType)
							typeProperty.IsStruct = true;
						PropertyInfo[] properties = type.GetProperties();
						for(Int32 i = 0; i < properties.Length; i++)
						{
							PropertyInfo propertyInfo = properties[i];
							if(serviceType == ServiceType.WS || DataContractAnalyzer.IsSupportedMember(propertyInfo))
							{
								list.Add(new StringPair(propertyInfo.Name, propertyInfo.PropertyType.FullName));
								rootTypes.Enqueue(propertyInfo.PropertyType);
							}
						}
						FieldInfo[] fields = type.GetFields();
						for(Int32 j = 0; j < fields.Length; j++)
						{
							FieldInfo fieldInfo = fields[j];
							if(serviceType == ServiceType.WS || DataContractAnalyzer.IsSupportedMember(fieldInfo))
							{
								list.Add(new StringPair(fieldInfo.Name, fieldInfo.FieldType.FullName));
								rootTypes.Enqueue(fieldInfo.FieldType);
							}
						}
					}

					KnownTypeAttribute[] knownAttributes = type.GetCustomAttributes2<KnownTypeAttribute>(false);
					for(Int32 loop = 0; loop < knownAttributes.Length; loop++)
					{
						rootTypes.Enqueue(knownAttributes[loop].Type);
						list2.Add(knownAttributes[loop].Type.FullName);
					}

					XmlIncludeAttribute[] xmlIncludeAttributes = type.GetCustomAttributes2<XmlIncludeAttribute>(false);
					for(Int32 loop = 0; loop < xmlIncludeAttributes.Length; loop++)
					{
						rootTypes.Enqueue(xmlIncludeAttributes[loop].Type);
						list2.Add(xmlIncludeAttributes[loop].Type.FullName);
					}
				}
			}
			return new TypeGraph(dictionary, dictionary2);
		}
		private static VariableWrapper BuildVariable(IDictionary<StringObjectPair, VariableWrapper> variables, StringObjectPair link)
		{
			String stringValue = link.StringValue;
			String fullName = link.ObjectValue.GetType().FullName;
			if(!DataContractAnalyzer.sharedMembers.TryGetValue(new StringPair(stringValue, fullName), out ServiceMemberWrapper serviceMemberInfo))
			{
				if(!DataContractAnalyzer.sharedTypes.ContainsKey(fullName))
					DataContractAnalyzer.sharedTypes.Add(fullName, new ServiceTypeWrapper(fullName, new List<ServiceMemberWrapper>(), new TypeProperty(), null));
				serviceMemberInfo = new ServiceMemberWrapper(stringValue, DataContractAnalyzer.sharedTypes[fullName]);
				DataContractAnalyzer.sharedMembers.Add(new StringPair(stringValue, fullName), serviceMemberInfo);
			}
			Object obj = link.ObjectValue;
			if(obj.GetType().Equals(typeof(NullObject)))
				obj = null;

			if(!variables.TryGetValue(link, out VariableWrapper result))
				variables.Add(link, result = new VariableWrapper(serviceMemberInfo, obj));
			return result;
		}

		private static Queue<Type> ExtractContractTypes(Type contractType, Type attributeType, out String clientTypeName)
		{
			Queue<Type> queue = new Queue<Type>();
			MethodInfo[] methods = contractType.GetMethods();
			for(Int32 i = 0;i < methods.Length;i++)
			{
				MethodInfo methodInfo = methods[i];
				if(attributeType != null && !methodInfo.ContainsCustomAttribute(attributeType, false))
					continue;

				ParameterInfo[] parameters = methodInfo.GetParameters();
				for(Int32 j = 0;j < parameters.Length;j++)
				{
					ParameterInfo parameterInfo = parameters[j];
					if(parameterInfo.ParameterType.IsByRef)
						queue.Enqueue(parameterInfo.ParameterType.GetElementType());
					else
						queue.Enqueue(parameterInfo.ParameterType);
				}
				if(methodInfo.ReturnType != null && !methodInfo.ReturnType.Equals(typeof(void)))
					queue.Enqueue(methodInfo.ReturnType);
			}

			clientTypeName = null;
			foreach(Type type in ClientSettings.ClientAssembly.GetTypes())
				if(contractType.IsAssignableFrom(type) && !type.IsInterface)
					clientTypeName = type.FullName;//TODO: Возможно, тут не хватает break;
			return queue;
		}

		private static Boolean HasAttribute(MemberInfo member, Type type)
			=> member.ContainsCustomAttribute(type, true);

		/// <summary>Проверка на подходящий тип данных для WCF сервиса</summary>
		/// <remarks>Проверка необходима только для WCF сервисов. К примеру для ExtensionDataObject</remarks>
		/// <param name="member">Тип для проверки на поддерживаемость</param>
		/// <returns>Тип поддерживается WCF сервисом</returns>
		private static Boolean IsSupportedMember(MemberInfo member)
		{
			foreach(Type attribute in DataContractAnalyzer.memberAttributes)
				if(DataContractAnalyzer.HasAttribute(member, attribute))
					return true;
			return false;
		}

		private static Boolean IsSupportedType(Type currentType)
		{
			foreach(Type type in DataContractAnalyzer.typeAttributes)
				if(DataContractAnalyzer.HasAttribute(currentType, type))
					return true;
			return false;
		}

		private static ServiceMemberWrapper GetServiceMemberInfo(IDictionary<StringPair, ServiceMemberWrapper> members, IDictionary<String, ServiceTypeWrapper> types, String parameterName, String parameterTypeName)
		{
			if(!members.TryGetValue(new StringPair(parameterName, parameterTypeName), out ServiceMemberWrapper result))
			{
				result = new ServiceMemberWrapper(parameterName, types[parameterTypeName]);
				members.Add(new StringPair(parameterName, parameterTypeName), result);
			}
			return result;
		}
	}
}