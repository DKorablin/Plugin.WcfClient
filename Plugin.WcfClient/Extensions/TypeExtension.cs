using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Plugin.WcfClient.Extensions
{
	public static class TypeExtension
	{
		public static T[] GetCustomAttributes2<T>(this MemberInfo info, Boolean inherit) where T : Attribute
		{
			List<T> result = new List<T>();
			Type typeOfResult = typeof(T);

			if(info.Module.Assembly.ReflectionOnly)
			{
				foreach(CustomAttributeData attribute in CustomAttributeData.GetCustomAttributes(info))
				{
					if(attribute.Constructor.DeclaringType.FullName == typeOfResult.FullName)
					{
						ConstructorInfo ctor = typeOfResult.GetConstructor(new Type[] { });
						T item = (T)ctor.Invoke(null);
						foreach(var argument in attribute.NamedArguments)
						{
							PropertyInfo property = typeOfResult.GetProperty(argument.MemberInfo.Name);

							if(property != null)
								property.SetValue(item, argument.TypedValue.Value, null);
							else
							{
								FieldInfo field = typeOfResult.GetField(argument.MemberInfo.Name);
								if(field != null)
									field.SetValue(item, argument.TypedValue.Value);
							}
						}
						result.Add(item);
					}
				}
			} else
			{
				Object[] attributes = info.GetCustomAttributes(typeOfResult, inherit);
				foreach(Object item in attributes)
					result.Add((T)item);
			}
			return result.ToArray();
		}

		public static Boolean ContainsCustomAttribute(this MemberInfo info, Type attributeType, Boolean inherit)
		{
			if(info.Module.Assembly.ReflectionOnly)
			{
				foreach(CustomAttributeData attribute in CustomAttributeData.GetCustomAttributes(info))
					if(attribute.Constructor.DeclaringType.FullName == attributeType.FullName)
						return true;
				return false;
			} else
				return info.GetCustomAttributes(attributeType, inherit).Length > 0;
		}

		/// <summary>This type from Basic Class Library</summary>
		/// <param name="type">Type to check</param>
		/// <returns>Type from BCL</returns>
		public static Boolean IsBclType(this Type type)
		{
			switch(type.Assembly.GetName().Name)
			{
			case "mscorlib":
			case "System.Private.CoreLib":
				return true;
			default:
				return false;
			}
		}

		internal static Boolean IsCollectionType(this Type type)
			=> type.ContainsCustomAttribute(typeof(CollectionDataContractAttribute), true);

		internal static Boolean IsDictionaryType(this Type type)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);

		internal static Boolean IsKeyValuePairType(this Type type)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);

		internal static Boolean IsNullableType(this Type type)
			=> type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
	}
}