using System;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient.Bll
{
	/// <summary>Аргументы изменения состояния сервиса</summary>
	internal class ServiceListChangedEventArgs : EventArgs
	{
		/// <summary>Статус изменения статуса</summary>
		public enum ChangeStatus
		{
			/// <summary>Добавлен</summary>
			Added,
			/// <summary>Удалён</summary>
			Removed,
			/// <summary>Загружен</summary>
			Loaded,
			/// <summary>Выгружен</summary>
			Unloaded,
			/// <summary>Перезагружается</summary>
			Reloading,
			/// <summary>Изменён</summary>
			Changed,
		}

		/// <summary>Ряд таблицы из настроек</summary>
		public SettingsDataSet.TreeRow TreeRow { get; }

		/// <summary>Проект. Используется при загрузке</summary>
		public ServiceProject Project { get; }

		/// <summary>Статус изменения в настройках</summary>
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