using System;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient.Bll
{
	/// <summary>Service state change arguments</summary>
	internal class ServiceListChangedEventArgs : EventArgs
	{
		/// <summary>Status change status</summary>
		public enum ChangeStatus
		{
			/// <summary>Added</summary>
			Added,
			/// <summary>Removed</summary>
			Removed,
			/// <summary>Loaded</summary>
			Loaded,
			/// <summary>Unloaded</summary>
			Unloaded,
			/// <summary>Reloading</summary>
			Reloading,
			/// <summary>Changed</summary>
			Changed,
		}

		/// <summary>Table row from settings</summary>
		public SettingsDataSet.TreeRow TreeRow { get; }

		/// <summary>Project. Used during loading</summary>
		public ServiceProject Project { get; }

		/// <summary>Settings change status</summary>
		public ChangeStatus Status { get; }

		public ServiceListChangedEventArgs(ServiceProject project)
		{
			this.Project = project;
			this.Status = ChangeStatus.Loaded;
		}
		public ServiceListChangedEventArgs(SettingsDataSet.TreeRow row, ChangeStatus status)
		{
			this.TreeRow = row;
			this.Status = status;
		}
	}
}