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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Xpf.Grid;

namespace DXSample.DragDropExtension {
    abstract class FindCardElementHitTestVisitorBase : DevExpress.Xpf.Grid.CardViewHitTestVisitorBase {
        protected readonly DragDropManagerBase dragDropManager;
        public FrameworkElement StoredHitElement { get; protected set; }
        protected FindCardElementHitTestVisitorBase(DragDropManagerBase dragDropManager) {
            this.dragDropManager = dragDropManager;
        }
        protected void StoreHitElement() {
            StoredHitElement = HitElement as FrameworkElement;
        }
    }
    internal class FindCardViewRowElementHitTestVisitor : FindCardElementHitTestVisitorBase {
        public FindCardViewRowElementHitTestVisitor(DragDropManagerBase dragDropManager)
            : base(dragDropManager) {
        }
        public override void VisitCard(int rowHandle) {
            StoreHitElement();
        }
        public override void VisitCardHeader(int rowHandle) {
            StoreHitElement();
        }
        public override void VisitCardHeaderButton(int rowHandle) {
            StoreHitElement();
        }
        public override void VisitGroupRow(int rowHandle) {
            StoreHitElement();
            StopHitTesting();
        }
    }
    internal class FindCardViewDataAreaElementHitTestVisitor : FindCardElementHitTestVisitorBase {
        public FindCardViewDataAreaElementHitTestVisitor(DragDropManagerBase dragDropManager)
            : base(dragDropManager) {
        }
        public override void VisitDataArea() {
            StoreHitElement();
        }
    }
}
