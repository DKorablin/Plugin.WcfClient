using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using Microsoft.VisualStudio.VirtualTreeGrid;
using Plugin.WcfClient.UI.WCF;

namespace Plugin.WcfClient.Parser
{
	internal class ParameterTreeAdapter : IMultiColumnBranch, IBranch
	{
		private enum ColumnIndex
		{
			VariableName = 0,
			VariableValue = 1,
			VariableType = 2,
		}

		private sealed class ChoiceContainer
		{
			private readonly ParameterTreeAdapter _branch;
			private readonly Int32 _column;
			private readonly Int32 _row;

			[TypeConverter(typeof(ChoiceConverter))]
			public String Choice
			{
				get => this._branch.GetText(this._row, this._column);
				set
				{
					MessageBox.Show("Whoops... (Do not open ComboBox or VirtualTreeGrid will crash)", "Crash");
					if(Array.FindIndex(ChoiceConverter.StaticChoices, (item) => { return item == value; }) > -1)
						this._branch.CommitLabelEdit(this._row, this._column, value);
					else
						this._branch.CommitLabelEdit(this._row, this._column, null);
				}
			}

			internal ChoiceContainer(ParameterTreeAdapter branch, Int32 row, Int32 column)
			{
				this._branch = branch;
				this._row = row;
				this._column = column;
			}
		}

		private sealed class ChoiceConverter : StringConverter
		{
			internal static String[] StaticChoices;

			internal static Boolean StaticExclusive;

			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
				=> new StandardValuesCollection(ChoiceConverter.StaticChoices);

			public override Boolean GetStandardValuesExclusive(ITypeDescriptorContext context)
				=> ChoiceConverter.StaticExclusive;

			public override Boolean GetStandardValuesSupported(ITypeDescriptorContext context)
				=> true;
		}

		private sealed class DummyContainer
		{
			[Editor(typeof(UITypeEditor), typeof(UITypeEditor))]
			public String DummyProperty { get { return null; } }
		}

		internal const Int32 NumColumns = 3;
		private ParameterTreeAdapter[] _children;
		private ParameterTreeAdapter _parent;
		private Boolean _readOnly;
		private Int32 _relativeRow;
		private VariableWrapper[] _variables;
		private ITree _virtualTree;
		private VirtualTreeControl _virtualTreeControl;

		public event BranchModificationEventHandler OnBranchModification;

		internal event EventHandler<EventArgs> OnValueUpdated;

		public Int32 ColumnCount => 3;

		public BranchFeatures Features
		{
			get
			{
				BranchFeatures result = BranchFeatures.Expansions | BranchFeatures.BranchRelocation | BranchFeatures.Realigns | BranchFeatures.PositionTracking;
				if(!this._readOnly)
					result |= BranchFeatures.ImmediateSelectionLabelEdits;
				return result;
			}
		}

		public Int32 UpdateCounter => 0;

		public Int32 VisibleItemCount => this._variables.Length;

		internal ParameterTreeAdapter(VirtualTreeControl treeCtrl, VariableWrapper[] variables, Boolean readOnly, ParameterTreeAdapter parent)
		{
			this._virtualTree = (ITree)treeCtrl.MultiColumnTree;
			this._virtualTreeControl = treeCtrl;
			this._variables = variables;
			this._readOnly = readOnly;
			this._parent = parent;
			this._children = new ParameterTreeAdapter[variables.Length];
			this.OnBranchModification = (BranchModificationEventHandler)Delegate.Combine(this.OnBranchModification, new BranchModificationEventHandler(this.ParameterTreeAdapter_OnBranchModification));
		}

		public VirtualTreeLabelEditData BeginLabelEdit(Int32 row, Int32 column, VirtualTreeLabelEditActivationStyles activationStyle)
		{
			if(column != 1)
				return VirtualTreeLabelEditData.Invalid;

			VirtualTreeLabelEditData result = VirtualTreeLabelEditData.Default;
			if(this._variables[row].EditorType == EditorType.TextBox)
			{
				ParameterTreeAdapter.DummyContainer dummyContainer = new ParameterTreeAdapter.DummyContainer();
				PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(dummyContainer)["DummyProperty"];
				result.CustomInPlaceEdit = TypeEditorHost.Create(propertyDescriptor, dummyContainer);
			} else
			{
				ParameterTreeAdapter.ChoiceContainer container = new ParameterTreeAdapter.ChoiceContainer(this, row, column);

				switch(this._variables[row].EditorType)
				{
				case EditorType.EditableDropDownBox:
					ParameterTreeAdapter.ChoiceConverter.StaticExclusive = false;
					break;
				case EditorType.DropDownBox:
					ParameterTreeAdapter.ChoiceConverter.StaticExclusive = true;
					break;
				}
				ParameterTreeAdapter.ChoiceConverter.StaticChoices = this._variables[row].GetSelectionList();
				PropertyDescriptor descriptor = TypeDescriptor.GetProperties(container)["Choice"];
				result.CustomInPlaceEdit = TypeEditorHost.Create(descriptor, container);
			}
			return result;
		}

		public SubItemCellStyles ColumnStyles(Int32 column)
			=> column == 0
				? SubItemCellStyles.Expandable
				: SubItemCellStyles.Simple;

		public LabelEditResult CommitLabelEdit(Int32 row, Int32 column, String newText)
		{
			if(newText == null)
			{
				this._virtualTreeControl.EndLabelEdit(true);
				return LabelEditResult.CancelEdit;
			}

			ValidationResult validation = this._variables[row].SetValue(newText);
			if(!validation.IsValid)
			{
				if(validation.ErrorMessage != null)
					RtlAwareMessageBox.Show(validation.ErrorMessage, "StringResources.ProductName", MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0);

				this._virtualTreeControl.EndLabelEdit(true);
				return LabelEditResult.CancelEdit;
			}

			if(validation.RefreshRequired)
			{
				this._virtualTreeControl.BeginUpdate();
				this._relativeRow = row;
				this._virtualTree.ListShuffle = true;
				this._virtualTree.Realign(this);
				this._virtualTree.ListShuffle = false;
				this._virtualTreeControl.EndUpdate();
			}

			ParameterTreeAdapter adapter = this;
			while(adapter._parent != null)
				adapter = adapter._parent;

			adapter.OnValueUpdated?.Invoke(this, EventArgs.Empty);

			return LabelEditResult.AcceptEdit;
		}

		public VirtualTreeAccessibilityData GetAccessibilityData(Int32 row, Int32 column)
			=> default;

		public VirtualTreeDisplayData GetDisplayData(Int32 row, Int32 column, VirtualTreeDisplayDataMasks requiredData)
			=> VirtualTreeDisplayData.Empty;

		public Int32 GetJaggedColumnCount(Int32 row)
			=> 0;

		public Object GetObject(Int32 row, Int32 column, ObjectStyle style, ref Int32 options)
		{
			if(style == ObjectStyle.TrackingObject)
				return new RowCol(row, column);

			return this.IsExpandable(row, column)
				? (this._children[row] = new ParameterTreeAdapter(this._virtualTreeControl, this._variables[row].GetChildVariables(), this._readOnly, this))
				: null;
		}

		public String GetText(Int32 row, Int32 column)
		{
			switch((ColumnIndex)column)
			{
			case ColumnIndex.VariableName:
				return this._variables[row].Name;
			case ColumnIndex.VariableValue:
				return this._variables[row].GetValue();
			case ColumnIndex.VariableType:
				if(this._variables[row].TypeName.Equals(typeof(NullObject).FullName))
					return typeof(NullObject).Name;
				return this._variables[row].FriendlyTypeName;
			default:
				throw new NotImplementedException();
			}
		}

		public String GetTipText(Int32 row, Int32 column, ToolTipType tipType)
			=> this.GetText(row, column);

		public Boolean IsExpandable(Int32 row, Int32 column)
			=> column == 0 && this._variables[row].IsExpandable();

		public LocateObjectData LocateObject(Object obj, ObjectStyle style, Int32 locateOptions)
		{
			LocateObjectData result = default;
			switch(style)
			{
				case ObjectStyle.TrackingObject:
					RowCol rowCol = (RowCol)obj;
					result.Row = rowCol.Row;
					result.Column = rowCol.Col;
					result.Options = 1;
					break;
				case ObjectStyle.ExpandedBranch:
					ParameterTreeAdapter adapter = (ParameterTreeAdapter)obj;
					ParameterTreeAdapter parentAdapter = adapter._parent;
					result.Row = -1;
					for(Int32 i = 0; i < parentAdapter._children.Length; i++)
					{
						if(parentAdapter._children[i] == adapter)
							result.Row = i;
					}
					result.Column = 0;
					result.Options = result.Row == this._relativeRow
						? 0
						: 1;
					break;

			}
			return result;
		}

		public void OnDragEvent(Object sender, Int32 row, Int32 column, DragEventType eventType, DragEventArgs args)
		{ }

		public void OnGiveFeedback(GiveFeedbackEventArgs args, Int32 row, Int32 column)
		{ }

		public void OnQueryContinueDrag(QueryContinueDragEventArgs args, Int32 row, Int32 column)
		{ }

		public VirtualTreeStartDragData OnStartDrag(Object sender, Int32 row, Int32 column, DragReason reason)
			=> VirtualTreeStartDragData.Empty;

		public void ParameterTreeAdapter_OnBranchModification(Object sender, BranchModificationEventArgs e)
		{ }

		public StateRefreshChanges SynchronizeState(Int32 row, Int32 column, IBranch matchBranch, Int32 matchRow, Int32 matchColumn)
			=> StateRefreshChanges.None;

		public StateRefreshChanges ToggleState(Int32 row, Int32 column)
			=> StateRefreshChanges.None;

		internal VariableWrapper[] GetVariables()
			=> this._variables;
	}
}