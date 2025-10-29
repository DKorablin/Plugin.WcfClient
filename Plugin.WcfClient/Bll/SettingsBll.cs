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

		/// <summary>Create a settings instance and load settings from the plugin provider</summary>
		/// <param name="plugin">The current plugin instance</param>
		public SettingsBll(PluginWindows plugin)
			: base(0)
		{
			this._plugin = plugin;

			using(Stream stream = this._plugin.HostWindows.Plugins.Settings(this._plugin).LoadAssemblyBlob(Constant.Settings.ServiceFileName))
				if(stream != null)
					base.DataSet.ReadXml(stream);
		}

		/// <summary>Save in plugin settings</summary>
		public override void Save()
		{
			using(MemoryStream stream = new MemoryStream())
			{
				base.DataSet.WriteXml(stream);
				this._plugin.HostWindows.Plugins.Settings(this._plugin).SaveAssemblyBlob(Constant.Settings.ServiceFileName, stream);
			}
			//base.Save();
		}

		/// <summary>Get service configuration by ID</summary>
		/// <param name="serviceId">Service configuration ID</param>
		/// <returns>Service configuration</returns>
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

			if(row.ParentTreeIDI != parentTreeId)//He's already in this node.
			{
				row.BeginEdit();
				row.ParentTreeIDI = parentTreeId;
				row.AcceptChanges();
			}
		}

		/// <summary>Change project path</summary>
		/// <param name="treeRow">Row of service settings to change</param>
		/// <param name="path">Path to the service project folder</param>
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

		/// <summary>Change a set of service settings</summary>
		/// <param name="serviceId">The service settings ID, or null if needed</param>
		/// <param name="type">Service type</param>
		/// <param name="address">Service address</param>
		/// <param name="path">Path to the service project on the file system</param>
		/// <returns>The changed set of service settings</returns>
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

		/// <summary>Change service path</summary>
		/// <param name="row">Service row</param>
		/// <param name="address">New name</param>
		/// <returns>Change result</returns>
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

		/// <summary>Delete a tree node and all its child nodes</summary>
		/// <param name="row">The tree row to delete</param>
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
				throw new NotImplementedException($"Element with type {row.ElementType} not implemented");
			}
		}

		/// <summary>Delete client node and client settings</summary>
		/// <param name="treeRow">Tree row to delete</param>
		private void RemoveClient(SettingsDataSet.TreeRow treeRow)
		{
			_ = treeRow ?? throw new ArgumentNullException(nameof(treeRow));

			if(treeRow.ElementType == ElementType.Client)
			{
				SettingsDataSet.ServiceRow clientRow = this.GetClientRow(treeRow.TreeId)
					?? throw new ApplicationException($"Row with client ID {treeRow.TreeId} not found");

				base.DataSet.Service.RemoveServiceRow(clientRow);

				base.DataSet.Tree.RemoveTreeRow(treeRow);
			}
		}
	}
}