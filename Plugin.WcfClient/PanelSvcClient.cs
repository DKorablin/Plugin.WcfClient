using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Parser;
using Plugin.WcfClient.Properties;
using Plugin.WcfClient.UI;
using SAL.Windows;

namespace Plugin.WcfClient
{
	public partial class PanelSvcTestClient : UserControl
	{
		private PluginWindows Plugin => (PluginWindows)this.Window.Plugin;
		private IWindow Window => (IWindow)base.Parent;

		public PanelSvcTestClient()
		{
			InitializeComponent();
			gridSearch.TreeView = tvService;
		}

		protected override void OnCreateControl()
		{
			this.Window.Caption = "Service Test Client";
			this.Window.SetTabPicture(Resources.iconWs);
			this.Window.SetDockAreas(DockAreas.DockBottom | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop | DockAreas.Float);
			this.Window.Closing += Window_Closing;
			this.Plugin.ServiceListChanged += Plugin_ServiceListChanged;
			base.OnCreateControl();

			tvService.Attach(this.Plugin.Settings.ServiceSettings);
		}

		private void Window_Closing(Object sender, CancelEventArgs e)
			=> this.Plugin.ServiceListChanged -= Plugin_ServiceListChanged;

		private void AddService(SettingsDataSet.TreeRow row)
		{
			ServiceTreeNode node = new ServiceTreeNode(row);
			
			tvService.BeginUpdate();
			ServiceTreeNode selectedNode = tvService.SelectedNode;
			while(selectedNode != null)
			{
				if(selectedNode.NodeType == ServiceTreeNode.TreeImageList.Folder)
				{
					selectedNode.Nodes.Add(node);
					break;
				}
				selectedNode = selectedNode.Parent;
			}
			if(selectedNode == null)
				tvService.Nodes.Add(node);

			tvService.SelectedNode = node;
			tvService.EndUpdate();
			//this.StartAddServiceWorker(new AddServiceInputs(address), "Fetching...", "Adding service...");
		}

		private void StartAddServiceWorker(SettingsDataSet.TreeRow row, AddServiceInputs.ActionType action, String status)
		{
			//this.UpdateButtonStatus(true, false);
			//this.GlobalStatusLabel.Text = status;
			ServiceProject project = this.Plugin.GetOpenedProject(row);
			if(project == null)
			{
				AddServiceInputs inputs = new AddServiceInputs(this.Plugin, action, row);
				bgAddService.RunWorkerAsync(inputs);
				if(!ProgressDlg.Prompt(action, status, new TimeSpan(0,0,20), bgAddService))
					bgAddService.CancelAsync();
			} else
				this.Plugin_ServiceListChanged(null, new ServiceListChangedEventArgs(project));
		}

		/// <summary>Выполнить базовое действие по умолчанию на выбранном узле</summary>
		/// <param name="node">Узел дерева</param>
		private void InvokeNodeDefaultAction(ServiceTreeNode node)
		{
			_ = node ?? throw new ArgumentNullException(nameof(node), "Не указан узел дерева для действия по умолчанию.");

			switch(node.NodeType)
			{
			case ServiceTreeNode.TreeImageList.File:
				ServiceProject project1 = node.FindProject();
				project1.StartSvcConfigEditor();
				break;
			case ServiceTreeNode.TreeImageList.Endpoint:
				node.Expand();
				break;
			case ServiceTreeNode.TreeImageList.Operation:
				ServiceMethodWrapper method = node.Method;
				ClientEndpointInfo endpoint = method.Endpoint;
				ServiceProject project2 = node.FindProject();

				/*if(node.Parent.Parent != null)//WCF
					project = (ServiceProject)node.Parent.Parent.Tag;
				else//WS
					project = (ServiceProject)node.Parent.Tag;*/

				IWindow window = this.Plugin.CreateWindow(
					typeof(DocumentSvcTestMethod).ToString(),
					true,
					new DocumentSvcTestMethodSettings()
					{
						TreeId = project2.Info.Row.TreeId,
						EndpointName = endpoint.EndpointName,
						MethodName = method.MethodName
					});
				break;
			default:
				this.Plugin.Trace.TraceData(TraceEventType.Error, 2, new NotImplementedException($"Action for node {node.NodeType} not implemented"));
				break;
			}
		}

		private void ShowProperties(ServiceTreeNode node)
		{
			if(node.IsRealEndpoint())
			{
				SettingsDataSet.TreeRow row = node.Settings;
				ServiceProperties prop = new ServiceProperties(this.Plugin, row);
				pgSettings.SelectedObject = prop;
				splitMain.Panel2Collapsed = false;
			} else
				pgSettings.SelectedObject = null;
		}

		private void Plugin_ServiceListChanged(Object sender, ServiceListChangedEventArgs e)
		{
			switch(e.Status)
			{
			case ServiceListChangedEventArgs.ChangeStatus.Added:
				this.AddService(e.TreeRow);
				break;
			case ServiceListChangedEventArgs.ChangeStatus.Loaded:
				{
					ServiceTreeNode node = tvService.FindNode(null, e.Project.Info.Row.TreeId);
					tvService.BeginUpdate();
					try
					{
						node.LoadProject(e.Project);
					} finally
					{
						tvService.EndUpdate();
					}
				}
				break;
			case ServiceListChangedEventArgs.ChangeStatus.Removed:
				{
					ServiceTreeNode node = tvService.FindNode(null, e.TreeRow.TreeId);
					node.Remove();
				}
				break;
			case ServiceListChangedEventArgs.ChangeStatus.Unloaded:
				{
					ServiceTreeNode node = tvService.FindNode(null, e.TreeRow.TreeId);
					tvService.BeginUpdate();
					try
					{
						node.UnloadProject();
					} finally
					{
						tvService.EndUpdate();
					}
				}
				break;
			case ServiceListChangedEventArgs.ChangeStatus.Changed:
				{
					ServiceTreeNode node = tvService.FindNode(null, e.TreeRow.TreeId);
					node.Text = e.TreeRow.Name;
					if(node.Project != null && e.TreeRow.ElementType == ElementType.Client)
					{
						if(!splitMain.Panel2Collapsed)
							splitMain.Panel2Collapsed = true;

						this.Plugin.UnloadService(e.TreeRow);
						this.StartAddServiceWorker(e.TreeRow, AddServiceInputs.ActionType.Download, "Downloading service...");
					}
				}
				break;
			}
		}

		private void splitMain_MouseDoubleClick(Object sender, MouseEventArgs e)
		{
			if(splitMain.SplitterRectangle.Contains(e.Location))
				splitMain.Panel2Collapsed = true;
		}

		private void tsbnAdd_Click(Object sender, EventArgs e)
			=> this.tsbnAdd_DropDownItemClicked(sender, new ToolStripItemClickedEventArgs(tsmiAddClient));

		private void tsbnAdd_DropDownItemClicked(Object sender, ToolStripItemClickedEventArgs e)
		{
			if(e.ClickedItem == tsmiAddClient)
			{
				using(OpenUrlDlg dlg = new OpenUrlDlg())
					if(dlg.ShowDialog() == DialogResult.OK)
						this.Plugin.AddService(dlg.Type, dlg.Address);
			} else if(e.ClickedItem == tsmiAddNode)
			{
				ServiceTreeNode node = new ServiceTreeNode() { NodeType = ServiceTreeNode.TreeImageList.Folder, };
				if(tvService.SelectedNode != null && tvService.SelectedNode.NodeType == ServiceTreeNode.TreeImageList.Folder)
					tvService.SelectedNode.Nodes.Add(node);
				else
					tvService.Nodes.Add(node);
				node.BeginEdit();
			} else
				throw new NotImplementedException($"Element {e.ClickedItem} not implemented");
		}

		private void tsbnDelete_Click(Object sender, EventArgs e)
		{
			ServiceTreeNode node = tvService.SelectedNode;
			SettingsDataSet.TreeRow row = node == null ? null : node.Settings;
			if(row == null)
				throw new ArgumentException("Cant remove node. Text: " + node.Text);

			String message;
			switch(row.ElementType)
			{
			case ElementType.Client:
				message = "Are you shure you want to remove selected project?";
				break;
			case ElementType.Tree:
				message = String.Format("Are you sure you want to remove node {0} and all children{1}?", row.Name, node.Nodes.Count == 0 ? String.Empty : "s");
				break;
			default:
				throw new NotImplementedException();
			}
			if(MessageBox.Show(message, this.Window.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				this.Plugin.RemoveNode(row);
		}

		private void bwAddService_DoWork(Object sender, DoWorkEventArgs e)
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

		private void bwAddService_RunWorkerCompleted(Object sender, RunWorkerCompletedEventArgs e)
		{
			AddServiceOutputs output = (AddServiceOutputs)e.Result;

			IEnumerable<String> errors = output.Errors;
			if(errors != null)
			{
				String message = String.Concat(errors.ToArray());

				tvService.BeginUpdate();
				tvService.SelectedNode.Nodes.Clear();
				ServiceTreeNode nodeError = new ServiceTreeNode
				{
					NodeType = ServiceTreeNode.TreeImageList.Error
				};
				nodeError.SetError(message);
				tvService.SelectedNode.Nodes.Add(nodeError);
				nodeError.Expand();
				tvService.EndUpdate();
				//ErrorItem[] array = new ErrorItem[addServiceOutputs.Errors.Count];
				//addServiceOutputs.Errors.CopyTo(array);
				//this.OnErrorReported(array);
			}
			//this.ConstructRecentServiceMenuItems();
			this.Plugin.LoadService(output);
			//this.UpdateButtonStatus();
			//MainForm.codeMarkers.CodeMarker(CodeMarkerEvent.perfWcfTools_TestClientReady);
		}

		private void tvService_MouseClick(Object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				TreeViewHitTestInfo info = tvService.HitTest(e.Location);
				if(info.Node != null)
				{
					tvService.SelectedNode = (ServiceTreeNode)info.Node;
					cmsService.Show(tvService, e.Location);
				}
			}
		}
		private void tvService_MouseDoubleClick(Object sender, MouseEventArgs e)
		{
			TreeViewHitTestInfo info = tvService.HitTest(e.Location);
			ServiceTreeNode node = (ServiceTreeNode)info.Node;
			if(node != null && e.Button == MouseButtons.Left)
				this.InvokeNodeDefaultAction(node);
		}

		private void tvService_BeforeExpand(Object sender, TreeViewCancelEventArgs e)
		{
			ServiceTreeNode node = (ServiceTreeNode)e.Node;
			if(e.Action == TreeViewAction.Expand && node.IsProjectUnloaded())
			{
				tvService.SelectedNode = node;

				SettingsDataSet.TreeRow row = node.Settings;

				this.StartAddServiceWorker(row, AddServiceInputs.ActionType.Open, "Opening service...");
			}
		}

		private void cmsService_Opening(Object sender, CancelEventArgs e)
		{
			ServiceTreeNode node = tvService.SelectedNode;
			if(node == null)
				e.Cancel = true;
			else
			{
				tsmiServiceBrowse.Visible =
					tsmiServiceCopy.Visible =
					tsmiServiceRemove.Visible =
					tsmiServiceUpdate.Visible =
					tsmiServiceProperties.Visible = node.IsRealEndpoint();
				tsmiServiceOpen.Visible = node.IsRealEndpoint() || node.NodeType == ServiceTreeNode.TreeImageList.Operation;

				if(!node.IsRealEndpoint() && node.NodeType != ServiceTreeNode.TreeImageList.Operation)
					e.Cancel = true;
			}
		}

		private void cmsService_ItemClicked(Object sender, ToolStripItemClickedEventArgs e)
		{
			if(e.ClickedItem == tsmiServiceCopy)
				return;//Игнор

			ServiceTreeNode node = tvService.SelectedNode;
			if(node != null)//Для шорткатов
			{
				cmsService.Close();
				if(e.ClickedItem == tsmiServiceOpen)
					this.InvokeNodeDefaultAction(node);
				else if(e.ClickedItem == tsmiServiceUpdate)
				{
					SettingsDataSet.TreeRow row = node.Settings;
					this.Plugin.UnloadService(row);
					this.StartAddServiceWorker(row, AddServiceInputs.ActionType.Download, "Downloading service...");
				} else if(e.ClickedItem == tsmiServiceBrowse)
				{
					SettingsDataSet.TreeRow row = node.Settings;
					if(this.Plugin.HostWindows.Windows.CreateWindow(Constant.Plugin.Browser,
						Constant.PluginType.BrowserDocument,
						true,
						new { NavigateUrl = row.Name }) == null)
						Process.Start(row.Name);
				} else if(e.ClickedItem == tsmiServiceRemove)
					this.tsbnDelete_Click(sender, e);
				else if(e.ClickedItem == tsmiServiceProperties)
					this.ShowProperties(node);
				else
					this.Plugin.Trace.TraceData(TraceEventType.Error, 2, new NotImplementedException(e.ClickedItem.ToString()));
			}
		}
		private void tsmiServiceCopy_DropDownItemClicked(Object sender, ToolStripItemClickedEventArgs e)
		{
			SettingsDataSet.TreeRow row = tvService.SelectedNode.Settings;
			if(e.ClickedItem == tsmiServiceCopyUrl)
				Clipboard.SetText(row.Name);
			else if(e.ClickedItem == tsmiServiceCopyPath)
				Clipboard.SetText(row.ServiceRow.Path);
			else
				this.Plugin.Trace.TraceData(TraceEventType.Error, 2, new NotImplementedException(e.ClickedItem.ToString()));
		}

		private void tvService_KeyDown(Object sender, KeyEventArgs e)
		{
			switch(e.KeyData)
			{
			case Keys.C | Keys.Control:
				if(tvService.SelectedNode != null)
				{
					e.Handled = true;
					Clipboard.SetText(tvService.SelectedNode.Text);
				}
				break;
			case Keys.O | Keys.Control:
				e.Handled = true;
				this.tsbnAdd_Click(null, e);
				break;
			case Keys.Delete:
				e.Handled = true;
				this.cmsService_ItemClicked(null, new ToolStripItemClickedEventArgs(tsmiServiceRemove));
				break;
			case Keys.F2:
				e.Handled = true;
				tvService.SelectedNode.BeginEdit();
				break;
			case Keys.F5:
				e.Handled = true;
				this.cmsService_ItemClicked(null, new ToolStripItemClickedEventArgs(tsmiServiceUpdate));
				break;
			case Keys.Enter:
				e.Handled = true;
				this.cmsService_ItemClicked(null, new ToolStripItemClickedEventArgs(tsmiServiceOpen));
				break;
			}
		}

		private void tvService_AfterLabelEdit(Object sender, NodeLabelEditEventArgs e)
		{
			ServiceTreeNode node = (ServiceTreeNode)e.Node;
			if(e.Label == null)//Отмена редактирования
			{
				if(node.Settings==null)//Удаление узла, если пользователь отменил создание
					node.Remove();
				return;
			} else if(e.Label.Trim().Length == 0)
				e.CancelEdit = true;
			else if(node.Text.Equals(e.Label.Trim()))
				e.CancelEdit = true;
			/*else if(!Uri.IsWellFormedUriString(e.Label, UriKind.RelativeOrAbsolute))
				e.CancelEdit = true;*///Сервис может быть и на TCP
			else if(node.Settings == null)//Добавление новой папки в дерево
			{
				SettingsDataSet.TreeRow parentRow = node.Parent == null ? null : node.Parent.Settings;
				switch(node.NodeType)
				{
				case ServiceTreeNode.TreeImageList.Endpoint://Добавление нового клиента
					throw new NotImplementedException("Endpont cant be added through add folder dlg");
				case ServiceTreeNode.TreeImageList.Folder://Добавление нового узла в дереве
					{
						SettingsDataSet.TreeRow newRow = this.Plugin.Settings.ServiceSettings.ModifyTreeNode(null, parentRow == null ? (Int32?)null : parentRow.TreeId, ElementType.Tree, e.Label);
						node.Settings = newRow;
						node.Text = e.Label;
						this.Plugin.Settings.ServiceSettings.Save();
					}
					break;
				default:
					throw new NotImplementedException(String.Format("Element with type {0} not implemented", node.NodeType));
				}
			} else
			{
				SettingsDataSet.TreeRow row = node.Settings;
				this.Plugin.Settings.ServiceSettings.ModifyTreeNodeAddress(row, e.Label.Trim());
				if(node.Project != null && row.ElementType == ElementType.Client)
				{
					//this.SelectedProject.RefreshConfig();//Это работать будет, но узлы не обновятся.
					this.Plugin.UnloadService(row);//Выгружаю старый проект, если он был уже загружен, чтобы сервис смог обновить
					this.StartAddServiceWorker(row, AddServiceInputs.ActionType.Download, "Downloading service...");
				}
			}
		}

		private void tvService_AfterSelect(Object sender, TreeViewEventArgs e)
		{
			tsbnDelete.Enabled = e.Node != null;
			if(!splitMain.Panel2Collapsed)
			{
				ServiceTreeNode node = (ServiceTreeNode)e.Node;
				this.ShowProperties(node);
			}
		}
	}
}