using System;
using System.Collections.Generic;
using System.ComponentModel;
using Plugin.WcfClient.Bll;

namespace Plugin.WcfClient.Parser
{
	internal class AddServiceExecutor
	{
		internal AddServiceInputs Inputs { get; }
		internal BackgroundWorker Worker { get; }

		public AddServiceExecutor(AddServiceInputs inputs, BackgroundWorker worker)
		{
			this.Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
			this.Worker = worker ?? throw new ArgumentNullException(nameof(worker));
		}

		private IEnumerable<ServiceProjectStartupInfo> GetEndpointsStartupInfo()
		{
			foreach(SettingsDataSet.TreeRow row in this.Inputs.Endpoints)
				yield return new ServiceProjectStartupInfo(this.Inputs.Plugin, row);
		}

		public AddServiceOutputs Execute()
		{
			AddServiceOutputs result = new AddServiceOutputs();
			if(this.Inputs.Endpoints.Length > 0)
			{
				Single startProgress = 0f;
				Single progressRange = 100f / (Single)this.Inputs.Endpoints.Length;
				foreach(ServiceProjectStartupInfo info in this.GetEndpointsStartupInfo())
				{
					ServiceProject project = this.AddServiceProject(info, startProgress, progressRange, out String error);

					if(project == null && !String.IsNullOrEmpty(error))
						result.AddError(error);
					else if(project != null)
					{
						result.AddServiceProject(project);
						result.IncrementSucceedCount();
					}
					startProgress += progressRange;
				}
			}
			return result;
		}

		private ServiceProject AddServiceProject(ServiceProjectStartupInfo info, Single startProgress, Single progressRange, out String error)
		{
			ServiceAnalyzer analyzer = ServiceAnalyzer.Create(info, this.Worker, startProgress, progressRange);

			ServiceProject project;
			switch(this.Inputs.Action)
			{
			case AddServiceInputs.ActionType.Download:
				project = analyzer.DownloadService(out error);
				break;
			case AddServiceInputs.ActionType.Open:
				project = info.IsProjectExists
					? analyzer.AnalyzeService(out error)
					: analyzer.DownloadService(out error);
				break;
			default:
				throw new NotImplementedException(this.Inputs.Action.ToString());
			}

			if(project != null && this.Worker.CancellationPending)
			{
				project.Remove();
				project = null;
			}

			return project;
		}
	}
}