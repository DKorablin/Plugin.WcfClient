using System;
using System.Data;
using System.IO;
using System.Linq;
using AlphaOmega.Bll;

namespace Plugin.WcfClient.Bll
{
	internal class SettingsBll : BllBase<SettingsDataSet,SettingsDataSet.ServiceRow>
	{
		private readonly PluginWindows _plugin;

		/// <summary>Создать экземпляр настроек и загрузить настройки из провайдера плагинов</summary>
		/// <param name="plugin">Плагин</param>
		public SettingsBll(PluginWindows plugin)
			: base(0)
		{
			this._plugin = plugin;

			using(Stream stream = this._plugin.HostWindows.Plugins.Settings(this._plugin).LoadAssemblyBlob(Constant.Settings.ServiceFileName))
				if(stream != null)
					base.DataSet.ReadXml(stream);
		}

		/// <summary>Сохранить в настройках плагина</summary>
		public override void Save()
		{
			using(MemoryStream stream = new MemoryStream())
			{
				base.DataSet.WriteXml(stream);
				this._plugin.HostWindows.Plugins.Settings(this._plugin).SaveAssemblyBlob(Constant.Settings.ServiceFileName, stream);
			}
			//base.Save();
		}

		/// <summary>Получить конфиг сервиса по идентификатору</summary>
		/// <param name="serviceId">Идентификатор конфига сервиса</param>
		/// <returns>Конфиг сервиса</returns>
		public SettingsDataSet.ServiceRow GetClientRow(Int32 treeId)
			=> base.DataSet.Service.FirstOrDefault(p => p.TreeId == treeId);

		public SettingsDataSet.TreeRow GetTreeNode(Int32 treeId)
			=> base.DataSet.Tree.FirstOrDefault(p => p.TreeId == treeId);

		public SettingsDataSet.TreeRow[] GetTreeNodes(Int32? parentTreeId)
			=> base.DataSet.Tree.Where(p => p.ParentTreeIDI == parentTreeId).OrderBy(p=>p.OrderId).ToArray();

		public void MoveNode(Int32 treeId, Int32? parentTreeId)
		{
			SettingsDataSet.TreeRow row = this.GetTreeNode(treeId);
			_ = row ?? throw new ArgumentNullException(nameof(treeId));

			SettingsDataSet.TreeRow parentRow;
			if(parentTreeId.HasValue)
			{
				parentRow = this.GetTreeNode(parentTreeId.Value)
					?? throw new ArgumentNullException(nameof(parentTreeId));
				if(parentRow.ElementType == ElementType.Client)
					throw new ArgumentException("Can't move to a client node");
			}

			if(row.ParentTreeIDI != parentTreeId)//Он уже в этом узле
			{
				row.BeginEdit();
				row.ParentTreeIDI = parentTreeId;
				row.AcceptChanges();
			}
		}

		/// <summary>Изменить путь к проекту</summary>
		/// <param name="treeRow">Ряд настроек сервиса, который необходимо изменить</param>
		/// <param name="path">Путь к папке с проектом сервиса</param>
		public void ModifyClient(SettingsDataSet.TreeRow treeRow, ServiceType serviceType, String proxyLogin, String proxyPassword)
		{
			if(treeRow.ElementType != ElementType.Client)
				throw new InvalidOperationException();

			SettingsDataSet.ServiceRow clientRow = this.GetClientRow(treeRow.TreeId);
			if(clientRow == null)
				clientRow = base.DataSet.Service.NewServiceRow();

			clientRow.BeginEdit();
			clientRow.TreeId = treeRow.TreeId;
			clientRow.ServiceType = serviceType;
			clientRow.ProxyLogin = proxyLogin;
			clientRow.ProxyPassword = proxyPassword;

			if(clientRow.RowState == DataRowState.Detached)
				base.DataSet.Service.AddServiceRow(clientRow);
			else
				clientRow.AcceptChanges();

			this.Save();
		}

		public void ModifyClientPath(SettingsDataSet.TreeRow treeRow, String path)
		{
			SettingsDataSet.ServiceRow clientRow = treeRow.ServiceRow;
			if(clientRow.Path == path)
				return;

			clientRow.BeginEdit();
			clientRow.Path = path;
			clientRow.AcceptChanges();
			this.Save();
		}

		/// <summary>Изменить ряд настроек сервиса</summary>
		/// <param name="serviceId">Идентификатор настроек сервиса или null, если его необходимо добавить</param>
		/// <param name="type">Тип сервиса</param>
		/// <param name="address">Адрес сервиса</param>
		/// <param name="path">Путь к проекту сервиса на файловой системе</param>
		/// <returns>Изменённый ряд настроек сервиса</returns>
		public SettingsDataSet.TreeRow ModifyTreeNode(Int32? treeId, Int32? parentTreeId, ElementType elementType, String name)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			SettingsDataSet.TreeRow row = treeId == null
				? base.DataSet.Tree.NewTreeRow()
				: this.GetTreeNode(treeId.Value);

			_ = row ?? throw new ArgumentNullException(nameof(treeId));

			if(elementType == ElementType.Client)
				while(name.EndsWith("/", StringComparison.Ordinal))
					name = name.Substring(0, name.Length - 1);

			row.BeginEdit();
			row.ParentTreeIDI = parentTreeId;
			row.ElementType = elementType;
			row.Name = name;

			if(row.RowState == System.Data.DataRowState.Detached)
				base.DataSet.Tree.AddTreeRow(row);
			else
				row.AcceptChanges();
			return row;
		}

		/// <summary>Изменить путь до сервиса</summary>
		/// <param name="row">Ряд сервиса</param>
		/// <param name="address">Новое наименование</param>
		/// <returns>Результат смены</returns>
		public Boolean ModifyTreeNodeAddress(SettingsDataSet.TreeRow row, String address)
		{
			_ = row ?? throw new ArgumentNullException(nameof(row));
			if(String.IsNullOrEmpty(address))
				throw new ArgumentNullException(nameof(address));

			Boolean result = address != row.Name;
			if(result)
			{
				row.BeginEdit();
				row.Name = address;
				row.AcceptChanges();
				this.Save();
			}
			return result;
		}

		/// <summary>Удалить узел дерева со всеми дочерними узлами</summary>
		/// <param name="row">Ряд дерева для удаления</param>
		public void RemoveNode(SettingsDataSet.TreeRow row)
		{
			_ = row ?? throw new ArgumentNullException(nameof(row));

			switch(row.ElementType)
			{
			case ElementType.Tree:
				foreach(SettingsDataSet.TreeRow treeRow in this.GetTreeNodes(row.TreeId))
					this.RemoveNode(treeRow);
				base.DataSet.Tree.RemoveTreeRow(row);
				break;
			case ElementType.Client:
				this.RemoveClient(row);
				break;
			default:
				throw new NotImplementedException(String.Format("Element with type {0} not implemented", row.ElementType));
			}
		}

		/// <summary>Удалить узел клиента и настройки клиента</summary>
		/// <param name="treeRow">Ряд дерева для удаления</param>
		private void RemoveClient(SettingsDataSet.TreeRow treeRow)
		{
			_ = treeRow ?? throw new ArgumentNullException(nameof(treeRow));

			if(treeRow.ElementType == ElementType.Client)
			{
				SettingsDataSet.ServiceRow clientRow = this.GetClientRow(treeRow.TreeId)
					?? throw new ApplicationException(String.Format("Row with client ID {0} not found", treeRow.TreeId));

				base.DataSet.Service.RemoveServiceRow(clientRow);

				base.DataSet.Tree.RemoveTreeRow(treeRow);
			}
		}
	}
}