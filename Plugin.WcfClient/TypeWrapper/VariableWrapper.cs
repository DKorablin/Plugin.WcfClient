using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Plugin.WcfClient.Parser
{
	/// <summary>Variable information for the test</summary>
	[Serializable]
	public class VariableWrapper
	{
		private const String DuplicateKeyMark = "[ # ]";
		private static readonly VariableWrapper[] Empty = new VariableWrapper[0];

		private VariableWrapper[] childVariables;
		private ServiceMemberWrapper currentMember;
		private ServiceMemberWrapper declaredMember;
		[NonSerialized]
		private Boolean _isKey;
		private Boolean _isValid = true;
		private Boolean _canModify = true;
		private String _name;
		[NonSerialized]
		private VariableWrapper parent;
		[NonSerialized]
		private ServiceMethodWrapper _methodInfo;
		private String _value;

		public Boolean IsValid => this._isValid;

		public EditorType EditorType
		{
			get
			{
				if(this.declaredMember.EditorType == EditorType.EditableDropDownBox)
				{
					String[] selectionList = this.GetSelectionList();
					if(selectionList == null || selectionList.Length < 1)
						return EditorType.TextBox;
				}
				return this.declaredMember.EditorType;
			}
		}

		public String FriendlyTypeName => this.declaredMember.FriendlyTypeName;

		public String Name
		{
			get
			{
				if(this._name == null)
					return this.declaredMember.VariableName;
				else if(this.IsValid)
					return this._name;
				else
					return VariableWrapper.DuplicateKeyMark;
			}
		}

		public String TypeName => this.declaredMember.TypeName;

		internal VariableWrapper(ServiceMemberWrapper declaredMember, Object obj)
			: this(declaredMember)
		{
			this._value = declaredMember.GetStringRepresentation(obj);
			this._canModify = false;
		}

		internal VariableWrapper(ServiceMemberWrapper declareMember, Boolean isKey)
			: this(declareMember)
		{
			this._isKey = isKey;
			if(isKey && this._value.Equals(TypeStrategy.NullRepresentation, StringComparison.Ordinal))
			{
				if(declareMember.HasMembers())
					this._value = this.TypeName;
				if(this.TypeName.Equals("System.String", StringComparison.Ordinal))
					this._value = String.Empty;
			}
		}

		internal VariableWrapper(String name, ServiceMemberWrapper declaredMember)
			: this(declaredMember)
		{
			this._name = name;
		}

		internal VariableWrapper(ServiceMemberWrapper declaredMember)
		{
			_ = declaredMember ?? throw new ArgumentNullException(nameof(declaredMember));

			this.currentMember = declaredMember;
			this.declaredMember = declaredMember;
			this._value = this.currentMember.GetDefaultValue();
		}

		public IList<Int32> ValidateDictionary()
		{
			TypeStrategy.CreateAndValidateDictionary(this.TypeName, this.childVariables, out List<Int32> result);
			return result;
		}

		/// <summary>Load service parameter values</summary>
		/// <param name="xmlPath">Path to the XML file</param>
		/// <param name="inputs">Array of parameters to which to load values</param>
		public static void LoadData(String xmlPath, VariableWrapper[] inputs)
		{
			XmlReaderSettings settings = new XmlReaderSettings()
			{
				CloseInput = true,
			};

			using(XmlReader reader = XmlReader.Create(new StreamReader(xmlPath), settings))
			{
				reader.Read();
				reader.ReadStartElement("UserData");
				Boolean flag = false;
				Stack<VariableWrapper[]> stack = new Stack<VariableWrapper[]>();
				VariableWrapper variableInfo = null;
				Int32 num = 1;
				stack.Push(inputs);
				Boolean flag2 = true;
				while(!flag)
				{
					reader.Read();
					if(reader.IsStartElement())
					{
						reader.ReadStartElement("Level");
						Int32 num2 = reader.ReadContentAsInt();
						reader.ReadEndElement();
						reader.ReadStartElement("Name");
						String b = reader.ReadString();
						reader.ReadEndElement();
						reader.ReadStartElement("Value");
						String text = reader.ReadString();
						reader.ReadEndElement();
						reader.ReadEndElement();
						if(num2 > num)
						{
							if(variableInfo != null)
								stack.Push(variableInfo.GetChildVariables());
							else
								flag2 = true;
						} else
						{
							if(num2 < num)
							{
								for(Int32 i = num2; i < num; i++)
									stack.Pop();
							} else
								flag2 = false;
						}
						if(!flag2)
						{
							num = num2;
							VariableWrapper[] array = stack.Peek();
							VariableWrapper variableInfo2 = null;
							foreach(VariableWrapper info in array)
								if(String.Equals(info.Name, b, StringComparison.Ordinal))
								{
									variableInfo2 = info;
									break;
								}
							variableInfo = variableInfo2;
							if(variableInfo != null)
								variableInfo.SetValue(text);
						}
					} else
						flag = true;
				}
				reader.Close();
			}
		}

		/// <summary>Save service parameter values</summary>
		/// <param name="xmlPath">Path to the XML file to write the values ​​to</param>
		/// <param name="inputs">Array of parameters and parameter values</param>
		internal static void SaveData(String xmlPath, VariableWrapper[] inputs)
		{
			using(XmlWriter xmlWriter = XmlWriter.Create(xmlPath))
			{
				Stack<Int32> stack = new Stack<Int32>();
				stack.Push(inputs.Length);
				Array.Reverse(inputs);
				Stack<VariableWrapper> stack2 = new Stack<VariableWrapper>(inputs);
				Array.Reverse(inputs);
				xmlWriter.WriteStartElement("UserData");
				while(stack2.Count > 0)
				{
					VariableWrapper variables = stack2.Pop();
					Int32 num = stack.Peek();
					xmlWriter.WriteStartElement("DataLine");
					xmlWriter.WriteElementString("Level", stack.Count.ToString(CultureInfo.CurrentUICulture));
					xmlWriter.WriteElementString("Name", variables.Name);
					xmlWriter.WriteElementString("Value", variables.GetValue());
					xmlWriter.WriteEndElement();
					stack.Pop();
					stack.Push(num - 1);
					VariableWrapper[] array = variables.GetChildVariables();
					if(array != null && array.Length > 0)
					{
						stack.Push(array.Length);
						Array.Reverse(array);
						foreach(VariableWrapper item in array)
							stack2.Push(item);
						Array.Reverse(array);
					}
					while(stack.Count > 0 && stack.Peek() == 0)
						stack.Pop();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.Close();
			}
		}

		internal VariableWrapper[] GetChildVariables()
		{
			if(TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal))
				return VariableWrapper.Empty;

			if(this._canModify)
			{
				if(this.declaredMember.HasMembers() && (this.childVariables == null || this._value != this.currentMember.TypeName))
				{
					this.currentMember = this.declaredMember;
					String variableName = this.declaredMember.VariableName;
					foreach(ServiceTypeWrapper current in this.declaredMember.SubTypes)
					{
						if(current.TypeName.Equals(this._value))
						{
							this.currentMember = new ServiceMemberWrapper(variableName, current);
							break;
						}
					}

					this.childVariables = new VariableWrapper[this.currentMember.Members.Count];
					Int32 num = 0;
					foreach(ServiceMemberWrapper current2 in this.currentMember.Members)
					{
						if(this.currentMember.IsKeyValuePair() && String.Equals(current2.VariableName, "Key", StringComparison.Ordinal))
							this.childVariables[num] = new VariableWrapper(current2, true);
						else
							this.childVariables[num] = new VariableWrapper(current2);

						this.childVariables[num].SetServiceMethodInfo(this._methodInfo);
						if(this.parent != null)
							this.childVariables[num].SetParent(this);
						num++;
					}
				}
				if(this.declaredMember.IsContainer())
				{
					Int32 arrayLength = VariableWrapper.GetArrayLength(this._value);
					VariableWrapper[] array = this.childVariables;
					this.childVariables = new VariableWrapper[arrayLength];
					ServiceMemberWrapper member = null;
					foreach(ServiceMemberWrapper item in this.declaredMember.Members)
					{
						member = item;
						break;
					}

					for(Int32 i = 0; i < arrayLength; i++)
					{
						if(array != null && i < array.Length)
							this.childVariables[i] = array[i];
						else
						{
							this.childVariables[i] = new VariableWrapper("[" + i + "]", member);
							this.childVariables[i].SetServiceMethodInfo(this._methodInfo);
							if(this.declaredMember.IsDictionary() || this.parent != null)
							{
								this.childVariables[i].SetParent(this);
								if(this.declaredMember.IsDictionary())
									this.childVariables[i].GetChildVariables();
							}
						}
					}
				}
			}
			return this.childVariables;
		}

		internal Object GetObject()
		{
			this.childVariables = this.IsExpandable()
				? this.GetChildVariables()
				: null;

			return this.currentMember.GetObject(this._value, this.childVariables);
		}

		internal String[] GetSelectionList()
		{
			String[] selectionList = this.declaredMember.GetSelectionList();
			if(this._isKey && selectionList != null)
			{
				Int32 num = Array.FindIndex<String>(selectionList, new Predicate<String>(VariableWrapper.IsNullRepresentation));
				if(num >= 0)
				{
					String[] array = new String[selectionList.Length - 1];
					Int32 num2 = 0;
					for(Int32 i = 0; i < array.Length; i++)
					{
						if(num2 == num)
							num2++;
						array[i] = selectionList[num2];
						num2++;
					}
					return array;
				}
			}
			return selectionList;
		}

		internal String GetValue()
		{
			if(String.Equals(this._value, this.TypeName, StringComparison.Ordinal)
				&& this.currentMember.HasMembers())
				return this.FriendlyTypeName;
			return this._value;
		}

		internal Boolean IsExpandable()
		{
			if(this.childVariables != null && this.childVariables.Length > 0)
				return true;
			if(this.EditorType == EditorType.DropDownBox)
				return !this.declaredMember.TypeName.Equals("System.Boolean")
					&& !this.declaredMember.IsEnum()
					&& !TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal);

			return this.declaredMember.IsContainer()
				&& !this._value.Equals(TypeStrategy.NullRepresentation, StringComparison.Ordinal)
				&& VariableWrapper.GetArrayLength(this._value) > 0;
		}

		internal void SetChildVariables(VariableWrapper[] value)
			=> this.childVariables = value;

		internal void SetServiceMethodInfo(ServiceMethodWrapper serviceMethodInfo)
			=> this._methodInfo = serviceMethodInfo;

		internal ValidationResult SetValue(String userValue)
		{
			String text = this._value;
			if((this._value = this.declaredMember.ValidateAndCanonicalize(userValue, out String errorMessage)) == null
				|| (this._isKey && TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal)))
			{
				this._value = text;
				return new ValidationResult(false, false, errorMessage);
			}
			if(this.declaredMember.IsDictionary())
			{
				this.GetChildVariables();
				this.Validate();
			}
			Boolean refreshRequired = false;
			if(this.parent != null)
			{
				VariableWrapper variableInfo = this;
				while(variableInfo != null && !variableInfo._isKey)
					variableInfo = variableInfo.parent;

				if(variableInfo != null)
				{
					refreshRequired = true;
					variableInfo = variableInfo.parent.parent;
					variableInfo.Validate();
				}
			}
			if(this.EditorType == EditorType.EditableDropDownBox && this.declaredMember.IsContainer())
			{
				if(TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal))
				{
					if(this.declaredMember.Members.Count > 0)
						return new ValidationResult(true, true);
				} else
				{
					if(TypeStrategy.NullRepresentation.Equals(text, StringComparison.Ordinal))
						return new ValidationResult(true, true);

					Int32 arrayLength = VariableWrapper.GetArrayLength(text);
					Int32 arrayLength2 = VariableWrapper.GetArrayLength(this._value);
					return new ValidationResult(true, arrayLength != arrayLength2);
				}
			}
			if(this.EditorType == EditorType.DropDownBox)
			{
				if(!TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal))
					return new ValidationResult(true, true);
				if(TypeStrategy.NullRepresentation.Equals(this._value, StringComparison.Ordinal) && this.currentMember.Members.Count > 0)
					return new ValidationResult(true, true);
			}
			return new ValidationResult(true, refreshRequired);
		}

		private static Int32 GetArrayLength(String canonicalizedValue)
			=> Int32.Parse(canonicalizedValue.Substring(TypeStrategy.LengthRepresentation.Length), CultureInfo.CurrentUICulture);

		internal static Boolean IsNullRepresentation(String str)
			=> TypeStrategy.NullRepresentation.Equals(str, StringComparison.Ordinal);

		private void SetParent(VariableWrapper parent)
			=> this.parent = parent;

		private void Validate()
		{
			if(this.childVariables != null)
			{
				foreach(VariableWrapper info in this.childVariables)
					info._isValid = true;

				IList<Int32> list = ServiceExecutor.ValidateDictionary(this, this._methodInfo.Endpoint.ServiceProject.ClientDomain);
				foreach(Int32 current in list)
					this.childVariables[current]._isValid = false;
			}
		}
	}
}