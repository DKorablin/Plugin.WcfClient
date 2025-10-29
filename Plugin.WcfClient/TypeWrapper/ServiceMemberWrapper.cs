using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plugin.WcfClient.Parser
{
	[Serializable]
	internal class ServiceMemberWrapper : IComparable
	{
		private ServiceTypeWrapper _type;
		private String _variableName;

		public EditorType EditorType => this._type.EditorType;

		public String FriendlyTypeName => this._type.FriendlyName;

		public Boolean IsValid => this._type.IsValid;

		public ICollection<ServiceMemberWrapper> Members => this._type.Members;

		public ICollection<ServiceTypeWrapper> SubTypes => this._type.SubTypes;

		public String TypeName => this._type.TypeName;

		public String VariableName => this._variableName;

		public ServiceMemberWrapper(String variableName, ServiceTypeWrapper type)
		{
			this._variableName = variableName;
			this._type = type;
		}

		public Int32 CompareTo(Object obj)
		{
			ServiceMemberWrapper serviceMemberInfo = (ServiceMemberWrapper)obj;
			return this._variableName.CompareTo(serviceMemberInfo._variableName);
		}

		public String GetDefaultValue()
			=> this._type.GetDefaultValue();

		public Object GetObject(String value, VariableWrapper[] variables)
			=> this._type.GetObject(value, variables);

		public String[] GetSelectionList()
			=> this._type.GetSelectionList();

		public String GetStringRepresentation(Object obj)
			=> this._type.GetStringRepresentation(obj);

		public Boolean HasMembers()
			=> this._type.HasMembers();

		public Boolean IsContainer()
			=> this._type.IsContainer();

		public Boolean IsDictionary()
			=> this._type.IsDictionary();

		public Boolean IsEnum()
			=> this._type.IsEnum();

		public Boolean IsKeyValuePair()
			=> this._type.IsKeyValuePair();

		public Boolean IsStruct()
			=> this._type.IsStruct();

		public String ValidateAndCanonicalize(String value, out String errorMessage)
		{
			String text = this._type.ValidateAndCanonicalize(value, out Int32 _);
			errorMessage = null;

			if(text == null)
				errorMessage = $"'{value}' is not valid value for this type";
			return text;
		}
	}
}