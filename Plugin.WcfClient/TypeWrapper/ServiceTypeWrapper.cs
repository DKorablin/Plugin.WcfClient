using System;
using System.Collections.Generic;
using System.Text;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class ServiceTypeWrapper
	{
		private String _friendlyName;
		private TypeStrategy typeStrategy;

		public EditorType EditorType => this.typeStrategy.EditorType;

		public String FriendlyName
			=> this._friendlyName == null
				? this._friendlyName = this.ComposeFriendlyName()
				: this._friendlyName;

		public Boolean IsValid => this.typeStrategy.IsValid;

		public ServiceMemberWrapper[] Members { get; private set; }

		public ServiceTypeWrapper[] SubTypes { get; private set; }

		public String TypeName => this.typeStrategy.TypeName;

		public ServiceTypeWrapper(String typeName, List<ServiceMemberWrapper> members, TypeProperty typeProperty, String[] enumChoices)
		{
			this.typeStrategy = new TypeStrategy(typeName, typeProperty, enumChoices);
			members.Sort();//TODO: Сортировка тут лишняя
			this.Members = members.ToArray();
			this.SubTypes = new ServiceTypeWrapper[] { };
		}

		public void AddToMemebers(ServiceMemberWrapper member)
			=> this.Members = AddItemToArray(this.Members, member);

		public void AddToSubTypes(ServiceTypeWrapper subType)
			=> this.SubTypes = AddItemToArray(this.SubTypes, subType);

		private static T[] AddItemToArray<T>(T[] arr, T item)
		{
			Array.Resize(ref arr, arr.Length + 1);
			arr[arr.Length - 1] = item;
			return arr;
		}

		public String GetDefaultValue()
			=> this.typeStrategy.GetDefaultValue();

		public Int32 GetEnumMemberCount()
			=> this.typeStrategy.GetEnumMemberCount();

		public Object GetObject(String value, VariableWrapper[] variables)
			=> this.typeStrategy.GetObject(value, variables);

		public String[] GetSelectionList()
		{
			String[] array = this.typeStrategy.GetSelectionList();
			if(array != null && array.Length == 0)
			{
				List<String> list = new List<String>();
				list.Add(TypeStrategy.NullRepresentation);
				list.Add(this.typeStrategy.TypeName);
				foreach(ServiceTypeWrapper current in this.SubTypes)
				{
					if(current.IsValid)
					{
						list.Add(current.TypeName);
					}
				}
				array = new String[list.Count];
				list.CopyTo(array);
			}
			return array;
		}

		public String GetStringRepresentation(Object obj)
			=> this.typeStrategy.GetStringRepresentation(obj);

		public Boolean HasMembers()
			=> this.typeStrategy.HasMembers();

		public Boolean IsContainer()
			=> this.typeStrategy.IsContainer();

		public Boolean IsDictionary()
			=> this.typeStrategy.IsDictionary();

		public Boolean IsEnum()
			=> this.typeStrategy.IsEnum();

		public Boolean IsKeyValuePair()
			=> this.typeStrategy.IsKeyValuePair();

		public Boolean IsStruct()
			=> this.typeStrategy.IsStruct();

		public void MarkAsInvalid()
			=> this.typeStrategy.MarkAsInvalid();

		public String ValidateAndCanonicalize(String input, out Int32 length)
			=> this.typeStrategy.ValidateAndCanonicalize(input, out length);

		private String ComposeFriendlyName()
		{
			Int32 index = this.TypeName.IndexOf('`');
			if(index > -1)
			{
				StringBuilder stringBuilder = new StringBuilder(this.TypeName.Substring(0, index));
				stringBuilder.Append("<");

				ICollection<ServiceMemberWrapper> collection = this.Members;
				if(this.IsDictionary())
					collection = this.Members[0].Members;

				Int32 num2 = 0;
				foreach(ServiceMemberWrapper current in collection)
				{
					if(num2++ > 0)
						stringBuilder.Append(",");
					stringBuilder.Append(current.FriendlyTypeName);
				}
				stringBuilder.Append(">");
				return stringBuilder.ToString();
			}
			return this.TypeName;
		}
	}
}