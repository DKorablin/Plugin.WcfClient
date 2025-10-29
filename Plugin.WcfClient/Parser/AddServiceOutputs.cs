using System;
using System.Collections.Generic;

namespace Plugin.WcfClient.Parser
{
	internal class AddServiceOutputs
	{
		private readonly List<String> _errors = new List<String>();
		private readonly List<ServiceProject> _projects = new List<ServiceProject>();
		private Int32 succeedCount;

		public Boolean Cancelled { get; set; }

		public IEnumerable<String> Errors
			=> this._errors.Count == 0 ? null : this._errors.AsReadOnly();

		public IEnumerable<ServiceProject> ServiceProjects => this._projects.AsReadOnly();

		public void AddError(String errorMessage)
			=> this._errors.Add(errorMessage);

		internal void AddServiceProject(ServiceProject project)
			=> this._projects.Add(project);

		internal void IncrementSucceedCount()
			=> this.succeedCount++;
	}
}