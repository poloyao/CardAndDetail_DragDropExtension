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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CardViewDragDrop {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

		private void CustomGridDragAndDrop_Dropped(object sender, DevExpress.Xpf.Grid.DragDrop.GridDroppedEventArgs e)
		{

		}

		private void CardDragDropManager_Changed(object sender, EventArgs e)
		{

		}

		private void CardDragDropManager_DragLeave(object sender, DevExpress.Xpf.Grid.DragDrop.DragLeaveEventArgs e)
		{

		}
	}
}
