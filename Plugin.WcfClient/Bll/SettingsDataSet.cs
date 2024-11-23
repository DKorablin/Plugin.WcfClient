using System;
using System.Linq;

namespace Plugin.WcfClient.Bll
{
	/// <summary>Attached service type</summary>
	internal enum ServiceType
	{
		/// <summary>Web Service</summary>
		WS,
		/// <summary>Windows Communication Foundation</summary>
		WCF,
	}

	/// <summary>Тип элемента</summary>
	public enum ElementType : Byte
	{
		/// <summary>Элемент дерева</summary>
		Tree = 0,
		/// <summary>WS/WCF Client</summary>
		Client = 1,
	}

	partial class SettingsDataSet
	{
		/// <summary>Ряд описания узла дерева</summary>
		partial class TreeRow
		{
			private ServiceRow _serviceRow;

			/// <summary>Тип элемента в узле дерева</summary>
			public ElementType ElementType
			{
				get => (ElementType)this.ElementTypeId;
				set => this.ElementTypeId = (Byte)value;
			}

			/// <summary>Родительский идентификатор узла в дереве</summary>
			public Int32? ParentTreeIDI
			{
				get => this.IsParentTreeIdNull() ? (Int32?)null : this.ParentTreeId;
				set
				{
					if(value == null)
						this.SetParentTreeIdNull();
					else
						this.ParentTreeId = value.Value;
				}
			}

			/// <summary>Получить информацию о настройках клиента</summary>
			public ServiceRow ServiceRow
			{
				get
				{
					switch(this.ElementType)
					{
						case Bll.ElementType.Client:
							return this._serviceRow
								?? (this._serviceRow = ((SettingsDataSet)this.Table.DataSet).Service.First(p => p.TreeId == this.TreeId));
						case Bll.ElementType.Tree:
							return null;
						default:
							throw new NotImplementedException(String.Format("Type: {0} not implemented", this.ElementType));
					}
				}
			}
		}

		/// <summary>Ряд описания подключения к сервису</summary>
		partial class ServiceRow
		{
			private TreeRow _treeRow;
			/// <summary>Получить информацию о узле дерева, где расположен этот клиент</summary>
			public TreeRow TreeRow
				=> this._treeRow ?? (this._treeRow = ((SettingsDataSet)this.Table.DataSet).Tree.First(p => p.TreeId == this.TreeId));

			/// <summary>Тип сервиса</summary>
			internal ServiceType ServiceType
			{
				get => (ServiceType)this.ServiceTypeId;
				set => this.ServiceTypeId = (Int32)value;
			}

			/// <summary>Переписывать конфиг файл при обновлении сервиса</summary>
			public Boolean RegenerateConfigEnabled
			{
				get => this.IsRegenerateConfigNull() || this.RegenerateConfig;
				set
				{
					if(value)
						this.SetRegenerateConfigNull();
					else
						this.RegenerateConfig = value;
				}
			}

			/// <summary>Использовать прокси</summary>
			public Boolean UseProxy => !String.IsNullOrEmpty(this.ProxyLogin) && !String.IsNullOrEmpty(this.ProxyPassword);

			/// <summary>Использовать авторизацию</summary>
			public Boolean UserAuthentication => !String.IsNullOrEmpty(this.Login) && !String.IsNullOrEmpty(this.Password);
		}
	}
}