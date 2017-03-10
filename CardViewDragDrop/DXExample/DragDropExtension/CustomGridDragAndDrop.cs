using DevExpress.Mvvm.UI;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace DXSample.DragDropExtension
{
	public class CustomGridDragAndDrop : GridDragDropManager
	{
		public TableView TableView;
		public RowControl SourceRow;

		public CustomGridDragAndDrop()
		{
			Drop += CustomGridDragAndDrop_Drop;
			DragOver += CustomGridDragAndDrop_DragOver;
		}

		void CustomGridDragAndDrop_DragOver(object sender, GridDragOverEventArgs e)
		{
			var result = VisualTreeHelper.HitTest(this.View, Mouse.GetPosition(View));

			if (result != null && result.VisualHit != null)
			{
				var hitRow = LayoutTreeHelper.GetVisualParents(HitElement).Where(row => row is GroupGridRow || row is RowControl).FirstOrDefault() as FrameworkElement;

				if (hitRow != null)
				{
					var rowData = hitRow.DataContext as RowData;
					e.AllowDrop = Equals(rowData.Row.GetType(), e.DraggedRows[0].GetType());
					e.Handled = true;
				}
				else
				{
					e.AllowDrop = false;
					e.Handled = true;
				}
			}
		}

		protected override bool CanStartDrag(MouseButtonEventArgs e)
		{
			SourceRow = LayoutTreeHelper.GetVisualParents(e.OriginalSource as DependencyObject).Where(row => row is RowControl).FirstOrDefault() as RowControl;

			return base.CanStartDrag(e);
		}

		void CustomGridDragAndDrop_Drop(object sender, GridDropEventArgs e)
		{
			if (TableView != null)
			{
				DropRow(e);
				TableView = null;
				e.Handled = true;
			}
		}

		protected override IList CalcDraggingRows(DevExpress.Xpf.Core.IndependentMouseEventArgs e)
		{
			if (SourceRow != null)
			{
				var rowData = SourceRow.DataContext as RowData;

				TableView = rowData.View as TableView;

				return new List<object> { rowData.Row };
			}
			else return null;
		}


		//protected override drag


		public void DropRow(GridDropEventArgs args)
		{

			var currentRow = LayoutTreeHelper.GetVisualParents(HitElement).Where(row => row is GroupGridRow || row is RowControl).FirstOrDefault() as FrameworkElement;

			var rowData = currentRow.DataContext as RowData;

			var tableSource = TableView.Grid.ItemsSource as IList;
			var dataView = rowData.View as TableView;
			var dataSource = dataView.Grid.ItemsSource as IList;
			var rowIndex = dataView.Grid.GetListIndexByRowHandle(args.TargetRowHandle);

			var draggingRow = DraggingRows[0];

			if (dataSource == null || dataSource[0].GetType() != draggingRow.GetType() || dataSource.Count == 0 || draggingRow == rowData.Row)
			{
				return;
			}

			if (Equals(tableSource, dataSource) && tableSource.IndexOf(DraggingRows[0]) <= rowIndex)
			{
				rowIndex--;
			}

			tableSource.Remove(DraggingRows[0]);

			if (!dataView.Grid.IsGrouped)
			{
				if (args.DropTargetType == DropTargetType.InsertRowsBefore)
				{
					dataSource.Insert(rowIndex, draggingRow);
				}
				else if (args.DropTargetType == DropTargetType.InsertRowsAfter)
				{
					dataSource.Insert(Math.Min(rowIndex + 1, dataSource.Count), draggingRow);
				}
				else
				{
					if (Mouse.GetPosition(currentRow).Y >= currentRow.ActualHeight / 2)
						dataSource.Insert(Math.Min(rowIndex + 1, dataSource.Count), draggingRow);
					else dataSource.Insert(rowIndex, draggingRow);
				}
			}
			else if (args.DropTargetType == DropTargetType.InsertRowsIntoGroup || dataView.Grid.IsGrouped)
			{
				var value = dataView.Grid.GetCellValue(rowData.RowHandle.Value, dataView.Grid.SortInfo[0].FieldName);

				TypeDescriptor.GetProperties(draggingRow.GetType())[dataView.Grid.SortInfo[0].FieldName].SetValue(draggingRow, value);

				if (args.DropTargetType == DropTargetType.InsertRowsAfter)
				{
					dataSource.Insert(Math.Min(rowIndex + 1, dataSource.Count), draggingRow);
				}
				else dataSource.Insert(rowIndex, draggingRow);

				dataView.Grid.RefreshData();
			}
		}

		static MethodInfo IsSameGroupInfo = typeof(GridDragDropManager).GetMethod("IsSameGroup", BindingFlags.NonPublic | BindingFlags.Instance);
		protected bool IsSameGroup(DragDropManagerBase sourceManager, GroupInfo[] groupInfos, DependencyObject hitElement)
		{
			return (bool)IsSameGroupInfo.Invoke(this, new object[] { sourceManager, groupInfos, hitElement });
		}

		static MethodInfo GetGroupInfosInfo = typeof(GridDragDropManager).GetMethod("GetGroupInfos", BindingFlags.NonPublic | BindingFlags.Instance);
		protected GroupInfo[] GetGroupInfos(int rowHandle)
		{
			return (GroupInfo[])GetGroupInfosInfo.Invoke(this, new object[] { rowHandle });
		}

		static MethodInfo SetReorderDropInfoInfo = typeof(GridDragDropManager).GetMethod("SetReorderDropInfo", BindingFlags.NonPublic | BindingFlags.Instance);
		protected void SetReorderDropInfo(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement)
		{
			SetReorderDropInfoInfo.Invoke(this, new object[] { sourceManager, insertRowHandle, hitElement });
		}

		static MethodInfo SetMoveToGroupRowDropInfoInfo = typeof(GridDragDropManager).GetMethod("SetMoveToGroupRowDropInfo", BindingFlags.NonPublic | BindingFlags.Instance);

		protected void SetMoveToGroupRowDropInfo(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement)
		{
			SetReorderDropInfoInfo.Invoke(this, new object[] { sourceManager, insertRowHandle, hitElement });
		}

		protected bool IsGrouped(GridControl grid)
		{
			return grid.GroupCount > 0;
		}
		protected bool IsSorted(GridControl grid)
		{
			return grid.SortInfo.Count > 0;
		}
		protected bool IsSortedButNotGrouped(GridControl grid)
		{
			return IsSorted(grid) && !IsGrouped(grid);
		}
		protected bool ShouldReorderGroup(GridControl grid)
		{
			return grid.SortInfo.Count <= grid.GroupCount;
		}

		protected override void PerformDropToViewCore(DragDropManagerBase sourceManager)
		{
			GridViewHitInfoBase hitInfo = GetHitInfo(HitElement);
			if (BanDrop(hitInfo.RowHandle, hitInfo, sourceManager, DropTargetType.None))
			{
				ClearDragInfo(sourceManager);
				return;
			}
			PerformDropToView(sourceManager, hitInfo as TableViewHitInfo, LastPosition, SetReorderDropInfo, (_) => SetMoveToGroupRowDropInfo, SetAddRowsDropInfo);
		}

		protected GridControl GetSourceGrid()
		{
			var parentRow = (FrameworkElement)LayoutHelper.FindParentObject<RowControl>(HitElement) ?? (FrameworkElement)LayoutHelper.FindParentObject<GridRowContent>(HitElement);
			return parentRow != null ? ((RowData)parentRow.DataContext).View.DataControl as GridControl : this.GridControl;
		}

		protected void PerformDropToView(DragDropManagerBase sourceManager, TableViewHitInfo hitInfo, Point pt, MoveRowsDelegate reorderDelegate, Func<bool, MoveRowsDelegate> groupDelegateExtractor, MoveRowsDelegate addRowsDelegate)
		{
			int insertRowHandle = hitInfo.RowHandle;
			var grid = GetSourceGrid();

			if (this.GridControl.IsGroupRowHandle(insertRowHandle))
			{
				groupDelegateExtractor(true)(sourceManager, insertRowHandle, HitElement);
				return;
			}
			if (IsSortedButNotGrouped(grid) || hitInfo.HitTest == TableViewHitTest.DataArea)
			{
				if (sourceManager.DraggingRows.Count > 0 && GetDataAreaElement(HitElement) != null/* && !ReferenceEquals(sourceManager, this)*/)
					addRowsDelegate(sourceManager, insertRowHandle, HitElement);
				else
					ClearDragInfo(sourceManager);

				return;
			}
			if (insertRowHandle == GridControl.InvalidRowHandle || insertRowHandle == GridControl.AutoFilterRowHandle || insertRowHandle == GridControl.NewItemRowHandle)
			{
				ClearDragInfo(sourceManager);
				return;
			}
			if (this.GridControl.GroupCount > 0)
			{
				int groupRowHandle = this.GridControl.GetParentRowHandle(insertRowHandle);
				if (ShouldReorderGroup(grid))
				{
					if (!IsSameGroup(sourceManager, GetGroupInfos(groupRowHandle), HitElement))
						groupDelegateExtractor(false)(sourceManager, groupRowHandle, HitElement);
					reorderDelegate(sourceManager, insertRowHandle, HitElement);
				}
				else
					groupDelegateExtractor(true)(sourceManager, groupRowHandle, HitElement);
			}
			else
			{
				reorderDelegate(sourceManager, insertRowHandle, HitElement);
			}
		}

		public delegate void MoveRowsDelegate(DragDropManagerBase sourceManager, int targetRowHandle, DependencyObject hitElement);
	}
}
