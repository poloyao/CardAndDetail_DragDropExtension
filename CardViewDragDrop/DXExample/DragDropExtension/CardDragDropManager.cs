// Developer Express Code Central Example:
// How to implement the Drag&Drop functionality for the CardView
// 
// We have created an example demonstrating how to implement the Drag&Drop
// functionality for the CardView.
// This functionality is encapsulated in the
// CardDragDropManager class. So, all you need to do is to attach this behavior to
// the GridControl.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E4616

// Developer Express Code Central Example:
// How to implement the Drag&Drop functionality for the CardView
// 
// We have created an example demonstrating how to implement the Drag&Drop
// functionality for the CardView.
// This functionality is encapsulated in the
// CardDragDropManager class. So, all you need to do is to attach this behavior to
// the GridControl.
// 
// You can find sample updates and versions for different programming languages here:
// http://www.devexpress.com/example=E4616

#region Copyright (c) 2000-2013 Developer Express Inc.
/*
{*******************************************************************}
{                                                                   }
{       Developer Express .NET Component Library                    }
{                                                                   }
{                                                                   }
{       Copyright (c) 2000-2013 Developer Express Inc.              }
{       ALL RIGHTS RESERVED                                         }
{                                                                   }
{   The entire contents of this file is protected by U.S. and       }
{   International Copyright Laws. Unauthorized reproduction,        }
{   reverse-engineering, and distribution of all or any portion of  }
{   the code contained in this file is strictly prohibited and may  }
{   result in severe civil and criminal penalties and will be       }
{   prosecuted to the maximum extent possible under the law.        }
{                                                                   }
{   RESTRICTIONS                                                    }
{                                                                   }
{   THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES           }
{   ARE CONFIDENTIAL AND PROPRIETARY TRADE                          }
{   SECRETS OF DEVELOPER EXPRESS INC. THE REGISTERED DEVELOPER IS   }
{   LICENSED TO DISTRIBUTE THE PRODUCT AND ALL ACCOMPANYING .NET    }
{   CONTROLS AS PART OF AN EXECUTABLE PROGRAM ONLY.                 }
{                                                                   }
{   THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED      }
{   FILES OR ANY PORTION OF ITS CONTENTS SHALL AT NO TIME BE        }
{   COPIED, TRANSFERRED, SOLD, DISTRIBUTED, OR OTHERWISE MADE       }
{   AVAILABLE TO OTHER INDIVIDUALS WITHOUT EXPRESS WRITTEN CONSENT  }
{   AND PERMISSION FROM DEVELOPER EXPRESS INC.                      }
{                                                                   }
{   CONSULT THE END USER LICENSE AGREEMENT FOR INFORMATION ON       }
{   ADDITIONAL RESTRICTIONS.                                        }
{                                                                   }
{*******************************************************************}
*/
#endregion Copyright (c) 2000-2013 Developer Express Inc.

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Collections.Generic;

using DevExpress.Data.Access;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.DragDrop;
using DevExpress.Xpf.Utils;
using DevExpress.Mvvm.UI;

namespace DXSample.DragDropExtension {
    public class CardDragDropManager : DragDropManagerBase {
        #region Properties
        public DataViewBase View { get { return (GridControl != null) ? GridControl.View : null; } }
        public double ScrollSpacing { get; set; }
        public double ScrollSpeed { get; set; }
        public GridViewHitInfoBase MouseDownHitInfo { get; protected internal set; }

        public bool RestoreSelection {
            get {
                return DataControl.ItemsSource is System.Collections.Specialized.INotifyCollectionChanged || DataControl.ItemsSource is IBindingList;
            }
        }

        CardView CardView { get { return View as CardView; } }
        GridControl GridControl { get { return DataControl as GridControl; } }
        DataControlBase DataControl { get { return AssociatedObject as DataControlBase; } }
        Point LastPosition { get; set; }
        DependencyObject HitElement { get { return this.hitElement; } }

        protected override IList ItemsSource {
            get {
                IListSource source = DataControl.ItemsSource as IListSource;
                if (source != null)
                    return source.GetList();
                return DataControl.ItemsSource as IList;
            }
        }
        #endregion

        DependencyObject hitElement;
        int HoverRowHandle;

        #region Inner classes
        public class CardDragSource : SupportDragDropBase {
            CardDragDropManager CardDragDropManager { get { return (CardDragDropManager)dragDropManager; } }
            protected override FrameworkElement Owner { get { return CardDragDropManager.DataControl; } }
            public CardDragSource(DragDropManagerBase dragDropManager)
                : base(dragDropManager) {
            }
            protected override FrameworkElement SourceElementCore {
                get { return CardDragDropManager.DataControl; }
            }
        }
        delegate void MoveRowsDelegate(DragDropManagerBase sourceManager, int targetRowHandle, DependencyObject hitElement);
        protected class DragDropHitTestResult : DragDropObjectBase {
            public UIElement Element { get; private set; }
            public DragDropHitTestResult(DragDropManagerBase manager)
                : base(manager) {
            }
            public HitTestResultBehavior CallBack(HitTestResult result) {
                Element = result.VisualHit as UIElement;
                if (Element == null || !UIElementHelper.IsVisibleInTree(Element as FrameworkElement) || !Element.IsHitTestVisible) {
                    return HitTestResultBehavior.Continue;
                }
                return HitTestResultBehavior.Stop;
            }
        }
        #endregion

        #region Behavior
        protected override void OnAttached() {
            base.OnAttached();
            DataControlBase dataCtrl = DataControl;
            if (dataCtrl != null) {
                DataViewDragDropManager.SetDragDropManager(dataCtrl, this);
                DragDropHelper = new RowDragDropElementHelper(CreateDragSource(this));
                AutoExpandTimer.Tick += new EventHandler(AutoExpandTimer_Tick);
                DragManager.SetDropTargetFactory(dataCtrl, new DragDropManagerDropTargetFactory());
            }
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            if (DataControl != null) {
                AutoExpandTimer.Tick -= new EventHandler(AutoExpandTimer_Tick);
            }
        }
        #endregion

        #region AutoExpander
        public static readonly DependencyProperty AllowAutoExpandProperty = DependencyPropertyManager.Register("AllowAutoExpandGroups",
                                                                                                               typeof(bool),
                                                                                                               typeof(CardDragDropManager),
                                                                                                               new PropertyMetadata(true));
        public static readonly DependencyProperty AutoExpandDelayProperty = DependencyPropertyManager.Register("AutoExpandGroupsDelay",
                                                                                                               typeof(int), typeof(CardDragDropManager),
                                                                                                               new PropertyMetadata(1000, (s, e) => {
                                                                                                                   ((CardDragDropManager)s).AutoExpandTimer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
                                                                                                               }));

        public bool AllowAutoExpandGroups {
            get { return (bool)GetValue(AllowAutoExpandProperty); }
            set { SetValue(AllowAutoExpandProperty, value); }
        }
        public int AutoExpandGroupsDelay {
            get { return (int)GetValue(AutoExpandDelayProperty); }
            set { SetValue(AutoExpandDelayProperty, value); }
        }

        DispatcherTimer AutoExpandTimer;
        bool IsExpandable {
            get {
                if (HoverRowHandle != GridControl.InvalidRowHandle &&
                    HoverRowHandle <= 0 &&
                    HoverRowHandle != GridControl.NewItemRowHandle &&
                    HoverRowHandle != GridControl.AutoFilterRowHandle)
                    return !GridControl.IsGroupRowExpanded(HoverRowHandle);
                else
                    return false;
            }
        }

        void PerformAutoExpand() {
            GridControl.ExpandGroupRow(HoverRowHandle);
        }
        void AutoExpandTimer_Tick(object sender, EventArgs e) {
            if (AllowAutoExpandGroups)
                DataControl.Dispatcher.BeginInvoke(new Action(() => PerformAutoExpand()));
        }
        void StopAutoExpandTimer() {
            if (AutoExpandTimer.IsEnabled)
                AutoExpandTimer.Stop();
        }
        #endregion

        public CardDragDropManager() {
            AutoExpandTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(AutoExpandGroupsDelay) };
        }

        #region DragDropHandlers
        public override void OnDragOver(DragDropManagerBase sourceManager, UIElement source, Point pt) {
            LastPosition = pt;
            UpdateHoverRowHandle(source, pt);
            if (IsExpandable) {
                AutoExpandTimer.Stop();
                AutoExpandTimer.Start();
            }
            else
                AutoExpandTimer.Stop();
            this.hitElement = GetVisibleHitTestElement(pt);
            PerformDropToViewCore(sourceManager);
            base.OnDragOver(sourceManager, source, pt);
        }
        protected override void OnDragLeave() {
            StopAutoExpandTimer();
            HoverRowHandle = GridControl.InvalidRowHandle;
            MouseDownHitInfo = null;
            base.OnDragLeave();
        }
        protected override void OnDrop(DragDropManagerBase sourceManager, UIElement source, Point pt) {
            if (DropEventIsLocked) return;
            //GridDropEventArgs e = RaiseDropEvent(sourceManager, pt);
            //if (!e.Handled)
                PerformDropToView(sourceManager, GetHitInfo(HitElement) as CardViewHitInfo, pt, MoveSelectedRows, x => new MoveRowsDelegate((s, g, h) => MoveSelectedRowsToGroup(s, g, h, x)), AddRows);
            //RaiseDroppedEvent(sourceManager, e);
            base.OnDrop(sourceManager, source, pt);


            //if (DropEventIsLocked) return;
            //PerformDropToView(sourceManager, GetHitInfo(HitElement) as CardViewHitInfo, pt, MoveSelectedRows, MoveSelectedRowsToGroup, AddRows);
            //base.OnDrop(sourceManager, source, pt);
        }
        #endregion

        #region Overrides
        protected override bool CanShowDropMarker() {
            return !View.RenderSize.IsZero();
        }
        protected override bool CustomAllowDrag(IndependentMouseEventArgs e) {
            DraggingRows = CalcDraggingRows(e);
            StartDragEventArgs startDragArgs = RaiseStartDragEvent(e);
            return startDragArgs.CanDrag;
        }
        protected override IList CalcDraggingRows(IndependentMouseEventArgs e) {
            CardViewHitInfo hitInfo = MouseDownHitInfo as CardViewHitInfo;
            if (IsInDataRow(hitInfo)) {
                return GetSelectedRowsCopy();
            }
            if (IsInGroupRow(hitInfo)) {
                return GetChildRows(hitInfo.RowHandle);
            }
            return null;
        }
        protected override FrameworkElement CreateLogicalOwner() {
            return View;
        }
        protected override bool CanStartDrag(MouseButtonEventArgs e) {
			//var asdasd = LayoutTreeHelper.GetVisualParents(e.OriginalSource as DependencyObject).Where(row => row is RowControl).FirstOrDefault() as RowControl;
			//var asjjjjj = LayoutHelper.GetParent(e.Source as DependencyObject);
			if (View.IsEditing)
                return false;
            MouseDownHitInfo = GetHitInfo(e.OriginalSource as DependencyObject);
            CardViewHitInfo hitInfo = MouseDownHitInfo as CardViewHitInfo;
            return hitInfo.InRow;
        }
        protected override StartDragEventArgs RaiseStartDragEvent(IndependentMouseEventArgs e) {
            StartDragEventArgs result = new StartDragEventArgs() { CanDrag = true };
            return result;
        }
        #endregion

        GridViewHitInfoBase GetHitInfo(DependencyObject element) {
            CardViewHitInfo result = CardView.CalcHitInfo(element);
            return result;
        }

        TableDragIndicatorPosition GetDragIndicatorPositionForRowElement(FrameworkElement rowElement) {
            if (rowElement == null)
                return TableDragIndicatorPosition.None;
            double point = LastPosition.Y - LayoutHelper.GetRelativeElementRect(rowElement, DataControl).Top;
            return point > rowElement.ActualHeight / 2 ?
                            TableDragIndicatorPosition.Bottom :
                            TableDragIndicatorPosition.Top;
        }

        bool BanDrop(int insertRowHandle, GridViewHitInfoBase hitInfo, DragDropManagerBase sourceManager) {
            SetDropMarkerVisibility(true);
            return DropEventIsLocked = false;
        }

        GridViewHitInfoBase GetHitInfo(IndependentMouseEventArgs e) {
            return GetHitInfo(e.OriginalSource as DependencyObject);
        }

        ISupportDragDrop CreateDragSource(CardDragDropManager dataViewDragDropManager) {
            return new CardDragSource(dataViewDragDropManager);
        }

        List<object> GetSelectedRowsCopy() {
            return new List<object>(GridControl.SelectedItems.Cast<object>());
        }
        bool IsInDataRow(CardViewHitInfo info) {
            return IsInRowCore(info) && !GridControl.IsGroupRowHandle(info.RowHandle);
        }
        bool IsInGroupRow(CardViewHitInfo info) {
            return IsInRowCore(info) && GridControl.IsGroupRowHandle(info.RowHandle);
        }
        bool IsInRowCore(CardViewHitInfo info) {
            return info.InRow;
        }

        DropTargetType GetDropTargetTypeByHitElement(DependencyObject hitElement) {
            FrameworkElement rowElement;
            TableDragIndicatorPosition dragIndicatorPosition;
            return GetDropTargetTypeByHitElement(hitElement, out rowElement, out dragIndicatorPosition);
        }
        DropTargetType GetDropTargetTypeByHitElement(DependencyObject hitElement, out FrameworkElement rowElement, out TableDragIndicatorPosition dragIndicatorPosition) {
            rowElement = GetRowElement(hitElement);
            dragIndicatorPosition = GetDragIndicatorPositionForRowElement(rowElement);
            switch (dragIndicatorPosition) {
                case TableDragIndicatorPosition.Top: return DropTargetType.InsertRowsBefore;
                case TableDragIndicatorPosition.Bottom: return DropTargetType.InsertRowsAfter;
                case TableDragIndicatorPosition.InRow: return DropTargetType.InsertRowsIntoNode;
                default: return DropTargetType.None;
            }
        }

        void PerformDropToViewCore(DragDropManagerBase sourceManager) {
            GridViewHitInfoBase hitInfo = GetHitInfo(HitElement);
            if (BanDrop(hitInfo.RowHandle, hitInfo, sourceManager)) {
                ClearDragInfo(sourceManager);
                return;
            }
            PerformDropToView(sourceManager, hitInfo as CardViewHitInfo, LastPosition, SetReorderDropInfo, x => new MoveRowsDelegate((s, g, h) => MoveSelectedRowsToGroup(s, g, h, x)), SetAddRowsDropInfo);
        }
        void PerformDropToView(DragDropManagerBase sourceManager, CardViewHitInfo hitInfo, Point pt, MoveRowsDelegate reorderDelegate, Func<bool, MoveRowsDelegate> groupDelegateExtractor, MoveRowsDelegate addRowsDelegate) {
            int insertRowHandle = hitInfo.RowHandle;
            if (GridControl.IsGroupRowHandle(insertRowHandle)) {
                groupDelegateExtractor(true)(sourceManager, insertRowHandle, HitElement);
                return;
            }
            if (IsSortedButNotGrouped() || hitInfo.HitTest == CardViewHitTest.DataArea) {
                if (sourceManager.DraggingRows.Count > 0 && GetDataAreaElement(HitElement) != null && !ReferenceEquals(sourceManager, this)) {
                    addRowsDelegate(sourceManager, insertRowHandle, HitElement);
                }
                else {
                    ClearDragInfo(sourceManager);
                }
                return;
            }
            if (insertRowHandle == GridControl.InvalidRowHandle || insertRowHandle == GridControl.AutoFilterRowHandle || insertRowHandle == GridControl.NewItemRowHandle) {
                ClearDragInfo(sourceManager);
                return;
            }
            if (GridControl.GroupCount > 0) {
                int groupRowHandle = GridControl.GetParentRowHandle(insertRowHandle);
                if (ShouldReorderGroup()) {
                    if (!IsSameGroup(sourceManager, GetGroupInfos(groupRowHandle), HitElement))
                        groupDelegateExtractor(false)(sourceManager, groupRowHandle, HitElement);
                    reorderDelegate(sourceManager, insertRowHandle, HitElement);
                }
                else
                    groupDelegateExtractor(true)(sourceManager, groupRowHandle, HitElement);
            }
            else {
                reorderDelegate(sourceManager, insertRowHandle, HitElement);
            }
        }

        List<object> GetChildRows(int groupRowHandle) {
            List<object> list = new List<object>();
            CollectGroupRowChildren(groupRowHandle, list);
            return list;
        }

        void UpdateHoverRowHandle(UIElement source, Point pt) {
            HoverRowHandle = GetOverRowHandle(source, pt);
        }

        int GetOverRowHandle(UIElement source, Point pt) {
            UIElement element = GetVisibleHitTestElement(pt);
            GridViewHitInfoBase hitInfo = GetHitInfo(element);
            return hitInfo.RowHandle;
        }

        bool ShouldReorderGroup() {
            return (GridControl.SortInfo.Count - GridControl.GroupCount) <= 0;
        }

        void AddRow(DragDropManagerBase sourceManager, object row, int insertRowHandle) {
            sourceManager.RemoveObject(row);
            ItemsSource.Add(sourceManager.GetObject(row));
        }
        void SetAddRowsDropInfo(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement) {
            sourceManager.SetDropTargetType(DropTargetType.DataArea);
            sourceManager.ShowDropMarker(GetDataAreaElement(hitElement), TableDragIndicatorPosition.None);
        }
        void AddRows(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement) {
            DataControl.BeginDataUpdate();
            foreach (object row in sourceManager.DraggingRows)
                AddRow(sourceManager, row, insertRowHandle);
            DataControl.EndDataUpdate();
        }

        void SetMoveToGroupRowDropInfo(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement) {
            GroupInfo[] groupInfo = GetGroupInfos(insertRowHandle);
            if (CanMoveSelectedRowsToGroup(sourceManager, groupInfo, hitElement)) {
                sourceManager.SetDropTargetType(DropTargetType.InsertRowsIntoGroup);
                sourceManager.ViewInfo.GroupInfo = groupInfo;
                sourceManager.ShowDropMarker(GetRowElement(hitElement), TableDragIndicatorPosition.None);
            }
            else {
                ClearDragInfo(sourceManager);
            }
        }

        void CollectGroupRowChildren(int groupRowHandle, IList list) {
            int childCount = GridControl.GetChildRowCount(groupRowHandle);
            for (int i = 0; i < childCount; i++) {
                int childRowHandle = GridControl.GetChildRowHandle(groupRowHandle, i);
                if (GridControl.IsGroupRowHandle(childRowHandle))
                    CollectGroupRowChildren(childRowHandle, list);
                else
                    list.Add(GridControl.GetRow(childRowHandle));
            }
        }
        bool IsSortedButNotGrouped() {
            return IsSorted() && !IsGrouped();
        }
        bool IsGrouped() {
            return GridControl.GroupCount > 0;
        }
        bool IsSorted() {
            return GridControl.SortInfo.Count > 0;
        }
        void MoveSelectedRows(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement) {
            object insertObject = GridControl.GetRow(insertRowHandle);
            if (insertObject == null || sourceManager.DraggingRows.Contains(GridControl.GetRow(insertRowHandle)))
                return;
            FrameworkElement row = GetRowElement(hitElement);
            DropTargetType dropTargetType = GetDropTargetType(row);
            if (dropTargetType == DropTargetType.None)
                return;
            int index = ItemsSource.IndexOf(insertObject);
            if (dropTargetType == DropTargetType.InsertRowsAfter)
                index++;

            GridControl.BeginDataUpdate();
            foreach (object obj in sourceManager.DraggingRows) {
                object sourceObject = sourceManager.GetObject(obj);

                if (ReferenceEquals(ItemsSource, sourceManager.GetSource(obj))) {
                    if (index > ItemsSource.IndexOf(sourceObject))
                        index--;
                }
                sourceManager.RemoveObject(obj);
                ItemsSource.Insert(index, sourceObject);
                index++;
            }
            GridControl.EndDataUpdate();

            if (RestoreSelection) {
                int startRowHandle = GridControl.GetRowHandleByListIndex(ItemsSource.IndexOf(sourceManager.DraggingRows[0]));
                int endRowHandle = GridControl.GetRowHandleByListIndex(ItemsSource.IndexOf(sourceManager.DraggingRows[sourceManager.DraggingRows.Count - 1]));
                GridControl.SelectRange(startRowHandle, endRowHandle);
            }
            else
                GridControl.UnselectAll();
            if (sourceManager.DraggingRows.Count > 0)
                GridControl.CurrentItem = sourceManager.DraggingRows[0];
        }
        void MoveSelectedRowsToGroup(DragDropManagerBase sourceManager, int groupRowHandle, DependencyObject hitElement, bool allowChangeSource) {
            GroupInfo[] groupInfo = GetGroupInfos(groupRowHandle);
            if (!CanMoveSelectedRowsToGroup(sourceManager, groupInfo, hitElement))
                return;
            foreach (object obj in sourceManager.DraggingRows) {
                foreach (GroupInfo item in groupInfo)
                    DevExpress.Xpf.Grid.DragDrop.Utils.SetPropertyValue(sourceManager.GetObject(obj), item.FieldName, item.Value);
                if (allowChangeSource && !ItemsSource.Contains(obj)) {
                    sourceManager.RemoveObject(sourceManager.GetObject(obj));
                    ItemsSource.Add(sourceManager.GetObject(obj));
                }
            }
        }
        //void MoveSelectedRowsToGroup(DragDropManagerBase sourceManager, int groupRowHandle, DependencyObject hitElement) {
        //    GroupInfo[] groupInfo = GetGroupInfos(groupRowHandle);
        //    if (!CanMoveSelectedRowsToGroup(sourceManager, groupInfo, hitElement))
        //        return;

        //    GridControl.BeginDataUpdate();
        //    foreach (object obj in sourceManager.DraggingRows) {
        //        foreach (GroupInfo item in groupInfo) {
        //            DevExpress.Xpf.Grid.DragDrop.Utils.SetPropertyValue(sourceManager.GetObject(obj), item.FieldName, item.Value);
        //        }
        //        if (!ItemsSource.Contains(obj)) {
        //            sourceManager.RemoveObject(sourceManager.GetObject(obj));
        //            ItemsSource.Add(sourceManager.GetObject(obj));
        //        }
        //    }
        //    GridControl.EndDataUpdate();
        //}
        bool CanMoveSelectedRowsToGroup(DragDropManagerBase sourceManager, GroupInfo[] groupInfos, DependencyObject hitElement) {
            if (GetRowElement(hitElement) == null)
                return false;
            else return !IsSameGroup(sourceManager, groupInfos, hitElement);
        }

        void SetReorderDropInfo(DragDropManagerBase sourceManager, int insertRowHandle, DependencyObject hitElement) {
            FrameworkElement rowElement = GetRowElement(hitElement);
            TableDragIndicatorPosition dragIndicatorPosition = GetDragIndicatorPositionForRowElement(rowElement);
            if (dragIndicatorPosition != TableDragIndicatorPosition.None) {
                DropTargetType dropTargetType = dragIndicatorPosition == TableDragIndicatorPosition.Bottom ? DropTargetType.InsertRowsAfter : DropTargetType.InsertRowsBefore;
                sourceManager.SetDropTargetType(dropTargetType);
                sourceManager.ViewInfo.DropTargetRow = GridControl.GetRow(insertRowHandle);
                sourceManager.ShowDropMarker(rowElement, dragIndicatorPosition);
            }
            else {
                ClearDragInfo(sourceManager);
            }
        }

        bool IsSameGroup(DragDropManagerBase sourceManager, GroupInfo[] groupInfos, DependencyObject hitElement) {
            //foreach (object obj in sourceManager.DraggingRows) {
            //    if (!ItemsSource.Contains(obj))
            //        return false;
            //    foreach (GroupInfo groupInfo in groupInfos) {
            //        object value;
            //        if (groupInfo.FieldName.Contains(".")) {
            //            ComplexPropertyDescriptor complexDescr = new ComplexPropertyDescriptor(obj, groupInfo.FieldName);
            //            value = complexDescr.GetValue(obj);
            //        }
            //        else
            //            value = TypeDescriptor.GetProperties(obj)[groupInfo.FieldName].GetValue(obj);
            //        if (!object.Equals(value, groupInfo.Value))
            //            return false;
            //    }
            //}
            //return true;
            foreach (object obj in sourceManager.DraggingRows) {
                if (!ItemsSource.Contains(obj))
                    return false;
                foreach (GroupInfo groupInfo in groupInfos) {
                    if (!object.Equals(DevExpress.Xpf.Grid.DragDrop.Utils.GetPropertyValue(obj, groupInfo.FieldName), groupInfo.Value))
                        return false;
                }
            }
            return true;
        }
        GroupInfo[] GetGroupInfos(int rowHandle) {
            int rowLevel = GridControl.GetRowLevelByRowHandle(rowHandle);
            GroupInfo[] groupInfo = new GroupInfo[rowLevel + 1];
            int currentGroupRowHandle = rowHandle;
            for (int i = rowLevel; i >= 0; i--) {
                groupInfo[i] = new GroupInfo() {
                    Value = GridControl.GetGroupRowValue(currentGroupRowHandle),
                    FieldName = GridControl.SortInfo[i].FieldName
                };
                currentGroupRowHandle = GridControl.GetParentRowHandle(currentGroupRowHandle);
            }
            return groupInfo;
        }
        FrameworkElement GetElementAcceptVisitor(DependencyObject hitElement, DataViewHitTestVisitorBase visitor) {
            GetHitInfo(hitElement).Accept(visitor);
            return (visitor as FindCardElementHitTestVisitorBase).StoredHitElement;
        }
        DataViewHitTestVisitorBase CreateFindDataAreaElementHitTestVisitor(DragDropManagerBase dataViewDragDropManager) {
            return new FindCardViewDataAreaElementHitTestVisitor(dataViewDragDropManager);
        }
        DataViewHitTestVisitorBase CreateFindRowElementHitTestVisitor(DragDropManagerBase dataViewDragDropManager) {
            return new FindCardViewRowElementHitTestVisitor(dataViewDragDropManager);
        }

        UIElement GetVisibleHitTestElement(Point pt) {
            DragDropHitTestResult result = new DragDropHitTestResult(this);
            VisualTreeHelper.HitTest(DataControl, null, new HitTestResultCallback(result.CallBack), new PointHitTestParameters(pt));
            return result.Element;
        }
        FrameworkElement GetRowElement(DependencyObject hitElement) {
            DataViewHitTestVisitorBase visitor = CreateFindRowElementHitTestVisitor(this);
            return GetElementAcceptVisitor(hitElement, visitor);
        }
        FrameworkElement GetDataAreaElement(DependencyObject hitElement) {
            DataViewHitTestVisitorBase visitor = CreateFindDataAreaElementHitTestVisitor(this);
            return GetElementAcceptVisitor(hitElement, visitor);
        }
        //void SetPropertyValue(object obj, string propertyName, object value) {
            //if (propertyName.Contains(".")) {
            //    DevExpress.Data.Access.ComplexPropertyDescriptor complexDesc = new DevExpress.Data.Access.ComplexPropertyDescriptor(obj, propertyName);
            //    complexDesc.SetValue(obj, value);
            //}
            //else
            //    TypeDescriptor.GetProperties(obj)[propertyName].SetValue(obj, value);
        //}
        object GetPropertyValue(object obj, string propertyName) {
            return TypeDescriptor.GetProperties(obj)[propertyName].GetValue(obj);
        }
        DropTargetType GetDropTargetType(FrameworkElement row) {
            switch (GetDragIndicatorPositionForRowElement(row)) {
                case TableDragIndicatorPosition.Top:
                    return DropTargetType.InsertRowsBefore;
                case TableDragIndicatorPosition.Bottom:
                    return DropTargetType.InsertRowsAfter;
                case TableDragIndicatorPosition.InRow:
                    return DropTargetType.InsertRowsIntoNode;
                default:
                    return DropTargetType.None;
            }
        }
        //void SetDropMarkerVisibility(bool visible) {
        //    IsDropMarkerVisible = visible;
        //    if (!visible)
        //        HideDropMarker();
        //}
    }
}
