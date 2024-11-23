using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.VirtualTreeGrid;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Parser;
using Plugin.WcfClient.Properties;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.WcfClient
{
	public partial class DocumentSvcTestMethod : UserControl, IPluginSettings<DocumentSvcTestMethodSettings>
	{
		private DocumentSvcTestMethodSettings _settings;
		private readonly Stopwatch _swElapsed = new Stopwatch();

		internal PluginWindows Plugin => (PluginWindows)this.Window.Plugin;

		private IWindow Window => (IWindow)base.Parent;

		Object IPluginSettings.Settings => this.Settings;

		public DocumentSvcTestMethodSettings Settings
			=> this._settings ?? (this._settings = new DocumentSvcTestMethodSettings());

		public DocumentSvcTestMethod()
		{
			this.InitializeComponent();
			VirtualTreeColumnHeader[] headers = new VirtualTreeColumnHeader[]
			{
				new VirtualTreeColumnHeader(Resources.colMethodName),
				new VirtualTreeColumnHeader(Resources.colMethodValue),
				new VirtualTreeColumnHeader(Resources.colMethodType)
			};
			gridInput.SetColumnHeaders(headers, true);
			gridInput.KeyDown+=new KeyEventHandler(this.gridInput_KeyDown);
			VirtualTreeColumnHeader[] headers2 = new VirtualTreeColumnHeader[]
			{
				new VirtualTreeColumnHeader(Resources.colMethodName),
				new VirtualTreeColumnHeader(Resources.colMethodValue),
				new VirtualTreeColumnHeader(Resources.colMethodType)
			};
			gridOutput.SetColumnHeaders(headers2, true);
			gridOutput.KeyDown += new KeyEventHandler(this.gridOutput_KeyDown);
		}

		protected override void OnCreateControl()
		{
			this.Window.SetDockAreas(DockAreas.Document | DockAreas.Float);
			this.Window.SetTabPicture(Resources.iconWs_operation);
			base.OnCreateControl();

			this.SetProject();
		}

		private ServiceProject GetProject()
		{
			if(this.Settings.TreeId == null)
				return null;

			ServiceProject project = this.Plugin.GetOpenedProject(this.Settings.TreeId.Value);
			if(project == null)
			{
				SettingsDataSet.TreeRow row = this.Plugin.Settings.ServiceSettings.GetTreeNode(this.Settings.TreeId.Value);
				if(row != null)
				{
					AddServiceInputs inputs = new AddServiceInputs(this.Plugin, AddServiceInputs.ActionType.Open, row);
					bwLoadService.RunWorkerAsync(inputs);
					this.ToggleForm(true);
				}
				return null;//Проект самостоятельно загрузит SetProject (Не сработает, если его вызвал InvokeTest)
			} else
				return project;
		}

		private ServiceMethodWrapper GetMethod()
		{
			ServiceProject project = this.GetProject();
			if(project == null)
				return null;

			ClientEndpointInfo endpoint = project.Endpoints.First(p => p.EndpointName == this.Settings.EndpointName);
			return endpoint.Methods.First(p => p.MethodName == this.Settings.MethodName);
		}

		private void SetProject()
		{
			ServiceMethodWrapper method = this.GetMethod();
			if(method != null)
				this.SetProject(method);
		}

		private void SetProject(ServiceProject project)
		{
			ClientEndpointInfo endpoint = project.Endpoints.First(p => p.EndpointName == this.Settings.EndpointName);
			ServiceMethodWrapper method = endpoint.Methods.First(p => p.MethodName == this.Settings.MethodName);

			this.SetProject(method);
		}

		private void SetProject(ServiceMethodWrapper method)
		{
			this.Settings.TreeId = method.Endpoint.ServiceProject.Info.Row.TreeId;
			this.Settings.EndpointName = method.Endpoint.EndpointName;
			this.Settings.MethodName = method.MethodName;

			this.Window.Caption = method == null
				? Resources.Empty
				: String.Join(" - ", new String[] { method.Endpoint.ServiceProject.Info.Row.Name, method.MethodName, });

			this.ResetInputTree();
			if(method.Endpoint.ServiceProject.Info.Row.ServiceRow.ServiceType == ServiceType.WS && !this.Plugin.Settings.ShowWsPayload)
			{//TODO: После 2х запросов XML GET параметров, WS сервис повисает. (См. ServiceExecutor Ln.264)
				tabMain.TabPages.Remove(tabXml);
				tabXml.Dispose();
				tabXml = null;
			}
		}

		private VariableWrapper[] GetVariables()
			=> ((ParameterTreeAdapter)((ITree)gridInput.MultiColumnTree).Root).GetVariables();

		private void InvokeTestCase(ServiceMethodWrapper method, VariableWrapper[] inputs, Boolean newClient)
		{
			//this.GlobalStatusLabel.Text = StringResources.StatusInvokingService;
			if(!bwInvokeService.IsBusy)
			{
				if(this.Plugin.Settings.SaveInputValues)
					method.SaveData(inputs);

				this.ToggleForm(true);
				this.Settings.Variables = inputs;

				bwInvokeService.RunWorkerAsync(new ServiceInvocationInputs(method, inputs, newClient));
			}
		}

		private void ToggleForm(Boolean isWorking)
		{
			if(isWorking)
			{
				error.SetError(lblRequest, null);
				_swElapsed.Reset();
				_swElapsed.Start();
			} else
			{
				_swElapsed.Stop();
				txtElapsed.Text = _swElapsed.Elapsed.ToString();
			}

			tsbnInvoke.Enabled = !isWorking;
			base.Cursor = isWorking ? Cursors.WaitCursor : Cursors.Default;
		}

		internal void ResetInputTree()
		{
			ServiceMethodWrapper method = this.GetMethod();
			if(method == null)
				return;

			VariableWrapper[] variables;

			if(this.Settings.Variables == null)
			{
				variables = method.GetVariables();
				if(this.Plugin.Settings.SaveInputValues)
					method.LoadData(variables);
			} else
			{//TODO: Тут может быть ошибка, если массив переменных изменился в сервисе, если в будущем внедрить обновление параметров
				variables = this.Settings.Variables;
			}

			foreach(VariableWrapper info in variables)
			{
				String[] selectionList = info.GetSelectionList();
				if(selectionList != null && selectionList.Length == 2 && selectionList[0] == TypeStrategy.NullRepresentation)
					info.SetValue(selectionList[1]);
			}

			this.PopulateTree(variables, gridInput, false);
			for(Int32 loop = 0; loop < variables.Length; loop++)
			{
				Int32 row = variables.Length - loop - 1;
				if(gridInput.Tree.IsExpandable(row, 0))
					gridInput.Tree.ToggleExpansion(row, 0);
			}
		}

		private void PopulateTree(VariableWrapper[] variables, VirtualTreeControl parameterTreeView, Boolean readOnly)
		{
			parameterTreeView.MultiColumnTree = new MultiColumnTree(3);
			ITree tree = (ITree)parameterTreeView.MultiColumnTree;
			tree.Root = new ParameterTreeAdapter(parameterTreeView, variables, readOnly, null);
			((ParameterTreeAdapter)tree.Root).OnValueUpdated += new EventHandler<EventArgs>(this.DocumentSvcTestMethod_OnValueUpdated);
		}

		private void PopulateOutputTree(VariableWrapper[] variables, String responseXml)
		{
			if(responseXml != null)
				txtXmlOutput.Text = responseXml;

			if(variables != null)
			{
				this.PopulateTree(variables, gridOutput, true);

				for(Int32 i = 0; i < variables.Length; i++)
				{
					Int32 row = variables.Length - i - 1;
					if(gridOutput.Tree.IsExpandable(row, 0))
					{
						//FIX: Код разворачивания дерева может уйти в рекурсию, если объекты ссылаются на родителя. Example: Node->Node->ParentNode
						gridOutput.Tree.ToggleExpansion(row, 0);
						//gridOutput.ExpandRecurse(row, 0);
					}
				}
			}
		}

		private void DocumentSvcTestMethod_OnValueUpdated(Object sender, EventArgs e)
		{
			this.gridOutput.MultiColumnTree = null;
			txtXmlOutput.Text = String.Empty;
		}

		private void gridInput_KeyDown(Object sender, KeyEventArgs e)
		{
			switch(e.KeyData)
			{
			case Keys.C | Keys.Control:
				ColumnItemEnumerator columnItemEnumerator = gridInput.CreateSelectedItemEnumerator();
				Int32 row = columnItemEnumerator.RowInTree + 1;
				Int32 columnInTree = columnItemEnumerator.ColumnInTree;
				VirtualTreeItemInfo itemInfo = gridInput.Tree.GetItemInfo(row, columnInTree, false);
				String text = itemInfo.Branch.GetText(itemInfo.Row, itemInfo.Column);
				if(!String.IsNullOrEmpty(text))
					Clipboard.SetText(text);
				break;
			case Keys.L | Keys.Control:
				ServiceMethodWrapper methodL = this.GetMethod();
				if(methodL != null)
				{
					VariableWrapper[] items = this.GetVariables();
					methodL.LoadData(items);
					gridInput.Refresh();
				}
				break;
			case Keys.S | Keys.Control:
				ServiceMethodWrapper methodS = this.GetMethod();
				methodS?.SaveData(this.GetVariables());
				break;
			}
		}

		private void gridOutput_KeyDown(Object sender, KeyEventArgs e)
		{
			switch(e.KeyData)
			{
			case Keys.C | Keys.Control:
				ColumnItemEnumerator columnItemEnumerator = gridOutput.CreateSelectedItemEnumerator();
				Int32 row = columnItemEnumerator.RowInTree + 1;
				Int32 columnInTree = columnItemEnumerator.ColumnInTree;
				VirtualTreeItemInfo itemInfo = gridOutput.Tree.GetItemInfo(row, columnInTree, false);
				String text = itemInfo.Branch.GetText(itemInfo.Row, itemInfo.Column);
				if(!String.IsNullOrEmpty(text))
					Clipboard.SetText(text);
				break;
			}
		}

		private void bwLoadService_DoWork(Object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker)sender;
			AddServiceOutputs output;
			try
			{
				AddServiceExecutor executor = new AddServiceExecutor((AddServiceInputs)e.Argument, worker);
				output = executor.Execute();
			} catch(Exception exc)
			{
				this.Plugin.Trace.TraceData(TraceEventType.Error, 10, exc);
				output = new AddServiceOutputs();
				output.AddError(exc.Message);
			}

			if(worker.CancellationPending)
				output.Cancelled = true;
			else
				worker.ReportProgress(100);

			e.Result = output;
		}

		private void bwLoadService_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
		{
			this.ToggleForm(false);

			AddServiceOutputs output = (AddServiceOutputs)e.Result;

			IEnumerable<String> errors = output.Errors;
			if(errors != null)
			{
				String message = String.Concat(errors.ToArray());
				error.SetError(lblRequest, message);
				this.Plugin.Trace.TraceEvent(TraceEventType.Warning, 1, message);
			}

			this.Plugin.LoadService(output);

			ServiceProject project = this.Plugin.GetOpenedProject(this.Settings.TreeId.Value);
			this.SetProject(project);
		}

		private void bwInvokeService_DoWork(Object sender, DoWorkEventArgs e)
		{
			ServiceInvocationInputs inputs = (ServiceInvocationInputs)e.Argument;
			ServiceProject project = this.Plugin.GetOpenedProject(this.Settings.TreeId.Value);
			project.IsWorking = true;
			e.Result = ServiceExecutor.ExecuteInClientDomain(inputs);
		}

		private void bwInvokeService_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
		{
			this.ToggleForm(false);

			if(e.Error != null)
			{
				error.SetError(lblRequest, e.Error.Message);
				this.Plugin.Trace.TraceData(TraceEventType.Error, 10, e.Error);
			} else
			{
				ServiceInvocationOutputs outputs = (ServiceInvocationOutputs)e.Result;
				VariableWrapper[] serviceInvocationResult = outputs.GetServiceInvocationResult();
				if(serviceInvocationResult != null)
				{
					this.PopulateOutputTree(serviceInvocationResult, outputs.ResponseXml);
					//this.GlobalStatusLabel.Text = StringResources.StatusInvokingServiceCompleted;
				} else
				{
					this.PopulateOutputTree(null, outputs.ResponseXml);
					if(!String.IsNullOrEmpty(outputs.ExceptionMessage))
					{
						error.SetError(lblRequest, outputs.ExceptionMessage);
						this.Plugin.Trace.TraceEvent(TraceEventType.Warning, 1, outputs.ExceptionMessage);
					}
					/*if(serviceInvocationOutputs.ExceptionType == ExceptionType.InvalidInput)
						RtlAwareMessageBox.Show(serviceInvocationOutputs.ExceptionMessage, StringResources.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);
					else
					{
						serviceInvocationOutputs.ServicePage.PopulateOutput(serviceInvocationResult, serviceInvocationOutputs.ResponseXml);
						this.GlobalStatusLabel.Text = StringResources.StatusInvokingServiceFailed;
						serviceInvocationOutputs.ServicePage.TestCase.SetError(new ErrorItem(StringResources.StatusInvokingServiceFailed, serviceInvocationOutputs.ExceptionMessage + Environment.NewLine + serviceInvocationOutputs.ExceptionStack, serviceInvocationOutputs.ServicePage.TestCase));
					}*/
				}
				ServiceProject project = this.GetProject();
				project.IsWorking = false;
				if(project.IsConfigChanged)
					this.Plugin.RefreshWcfConfig(project);
			}
		}

		private void txtXml_KeyDown(Object sender, KeyEventArgs e)
		{
			switch(e.KeyData)
			{
			case Keys.A | Keys.Control:
				e.Handled = true;
				((TextBox)sender).SelectAll();
				break;
			}
		}

		private void tsbnInvoke_Click(Object sender, EventArgs e)
		{
			ServiceMethodWrapper method = this.GetMethod();
			if(method != null)
				this.InvokeTestCase(method, this.GetVariables(), tsmiProxy.Checked);
		}

		private void tsbnValues_DropDownItemClicked(Object sender, ToolStripItemClickedEventArgs e)
		{
			if(e.ClickedItem == tsmiValuesLoad)
			{
				using(OpenFileDialog dlg = new OpenFileDialog() { CheckFileExists = true, DefaultExt = "xml", Filter = "XML file (*.xml)|*.xml|Aff files (*.*)|*.*", Title = "Load varaibles", })
					if(dlg.ShowDialog() == DialogResult.OK)
					{
						VariableWrapper[] items = this.GetVariables();
						VariableWrapper.LoadData(dlg.FileName, items);
						gridInput.Refresh();
					}
			} else if(e.ClickedItem == tsmiValuesSave)
			{
				using(SaveFileDialog dlg = new SaveFileDialog() { OverwritePrompt = true, AddExtension = true, DefaultExt = "xml", Filter = "XML file (*.xml)|*.xml|All files (*.*)|*.*", })
					if(dlg.ShowDialog() == DialogResult.OK)
						VariableWrapper.SaveData(dlg.FileName, this.GetVariables());
			} else
				throw new NotImplementedException(String.Format("Item {0} not implemented", e.ClickedItem));
		}

		private void tabMain_SelectedIndexChanged(Object sender, EventArgs e)
		{
			if(tabMain.SelectedTab == tabXml)
				if(bwInvokeService.IsBusy)
					tabMain.SelectedTab = tabFormatted;
				else
				{
					ServiceMethodWrapper method = this.GetMethod();
					if(method != null)
						txtXmlInput.Text = ServiceExecutor.TranslateToXmlInClientDomain(new ServiceInvocationInputs(method, this.GetVariables(), false));
				}
		}
	}
}