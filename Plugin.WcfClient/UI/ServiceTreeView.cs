using System;
using System.Collections.Generic;
using System.Windows.Forms;
using AlphaOmega.Windows.Forms;
using Plugin.WcfClient.Bll;
using Plugin.WcfClient.Parser;

namespace Plugin.WcfClient.UI
{
	internal class ServiceTreeView : DraggableTreeView<ServiceTreeNode, ServiceTreeNode.TagExtender>
	{
		private SettingsBll Settings { get; set; }

		public new ServiceTreeNode SelectedNode
		{
			get => (ServiceTreeNode)base.SelectedNode;
			set => base.SelectedNode = value;
		}

		/// <summary>Selected project node</summary>
		public ServiceProject SelectedProject
			=> this.SelectedNode?.FindProject();

		protected override Boolean IsFolderNode(ServiceTreeNode treeNode)
		{
			switch(treeNode.NodeType)
			{
			case ServiceTreeNode.TreeImageList.Folder:
				return true;
			case ServiceTreeNode.TreeImageList.Endpoint:
				return false;
			default:
				throw new NotImplementedException($"NodeType '{treeNode.NodeType}' not implemented");
			}
		}

		protected override void OnDragDrop(DragEventArgs args)
		{
			base.OnDragDrop(args);

			if(base.IsDataPresent(args) && base.HasMoved)
			{
				ServiceTreeNode movingNode = base.GetDragDropNode(args);

				SettingsDataSet.TreeRow movingRow = movingNode.Settings;
				SettingsDataSet.TreeRow toRow = movingNode.Parent?.Settings;

				Int32? parentTreeId = toRow == null ? (Int32?)null : toRow.TreeId;
				this.Settings.MoveNode(movingRow.TreeId, parentTreeId);

				Int32 orderId = 0;
				TreeNodeCollection nodes = movingNode.Parent == null
					? base.Nodes
					: movingNode.Parent.Nodes;

				foreach(ServiceTreeNode node in nodes)
					node.Settings.OrderId = orderId++;

				this.Settings.Save();
			}
		}

		protected override void OnBeforeLabelEdit(NodeLabelEditEventArgs e)
		{
			ServiceTreeNode node = (ServiceTreeNode)e.Node;
			switch(node.NodeType)
			{
			case ServiceTreeNode.TreeImageList.Folder:
			case ServiceTreeNode.TreeImageList.Endpoint:
				e.CancelEdit = false;
				break;
			default:
				e.CancelEdit = true;
				break;
			}

			base.OnBeforeLabelEdit(e);
		}

		public ServiceTreeNode FindNode(ServiceTreeNode rootNode, Int32 treeId)
		{
			foreach(ServiceTreeNode node in rootNode == null ? this.Nodes : rootNode.Nodes)
			{
				SettingsDataSet.TreeRow row = node.Settings;
				if(row == null)
					break;//We went deep into service
				else if(row.TreeId == treeId)
					return node;
				else
				{
					ServiceTreeNode result = this.FindNode(node, treeId);
					if(result != null)
						return result;
				}
			}
			return null;
		}

		public void Attach(SettingsBll settings)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));

			List<ServiceTreeNode> nodes = new List<ServiceTreeNode>(this.Fill(settings, null));

			base.BeginUpdate();
			base.Nodes.AddRange(nodes.ToArray());
			base.EndUpdate();
		}

		private IEnumerable<ServiceTreeNode> Fill(SettingsBll settings, Int32? parentTreeId)
		{
			foreach(SettingsDataSet.TreeRow row in settings.GetTreeNodes(parentTreeId))
			{
				ServiceTreeNode node = new ServiceTreeNode(row);
				switch(row.ElementType)
				{
				case ElementType.Tree:
					foreach(ServiceTreeNode childNode in this.Fill(settings, row.TreeId))
						node.Nodes.Add(childNode);
					node.Expand();
					break;
				case ElementType.Client:
					break;
				default:
					throw new NotImplementedException($"Element with type '{row.ElementType}' not implemented");
				}

				yield return node;
			}
		}
	}
}