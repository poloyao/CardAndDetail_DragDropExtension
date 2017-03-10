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

using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DXSample {
    public class OrderHelper {
        public ObservableCollection<Order> Orders { get; private set; }
		public ObservableCollection<OrderParent> Orders2 { get; private set; } = new ObservableCollection<OrderParent>();
		public ObservableCollection<OrderParent> Orders3 { get; private set; } = new ObservableCollection<OrderParent>();

		public OrderHelper() {
            Random rnd = new Random();

            Orders = new ObservableCollection<Order>();
            for (int i = 0; i < 10; i++)
                Orders.Add(new Order());
			Orders2 = new ObservableCollection<DXSample.OrderParent>();
			Orders2.Add(new OrderParent()
			{
				Name = "aaaa",
				items = new ObservableCollection<Order>() {
					new Order(),
					new Order()
				}
			});
			Orders2.Add(new OrderParent()
			{
				Name = "bbbbb",
				items = new ObservableCollection<Order>() {
					new Order(),
					new Order()
				}
			});
			Orders3.Add(new OrderParent()
			{
				Name = "cccc",
				items = new ObservableCollection<Order>() {
					new Order(),
					new Order()
				}
			});
		}
    }

	public class OrderParent : ViewModelBase
	{
		public string Name { get; set; }

		public ObservableCollection<Order> items { get; set; } = new ObservableCollection<Order>();
	}

    public class Order : ViewModelBase {
        static Random rnd = new Random();

        string _Name;
        DateTime _OrderDate;
        int _Amount;
        int _Price;
        bool _IsProcessed;

        #region Properties

        public string Name {
            get { return _Name; }
            set { SetProperty(ref _Name, value, () => Name); }
        }
        public DateTime OrderDate {
            get { return _OrderDate; }
            set { SetProperty(ref _OrderDate, value, () => OrderDate); }
        }
        public int Amount {
            get { return _Amount; }
            set { SetProperty(ref _Amount, value, () => Amount); }
        }
        public int Price {
            get { return _Price; }
            set { SetProperty(ref _Price, value, () => Price); }
        }
        public bool IsProcessed {
            get { return _IsProcessed; }
            set { SetProperty(ref _IsProcessed, value, () => IsProcessed); }
        }

        #endregion

        public Order() {
            Name = RandomStringHelper.GetRandomString();
            OrderDate = new DateTime(rnd.Next(1998, 2012), rnd.Next(1, 12), rnd.Next(1, 28));
            Amount = rnd.Next(-1000, 1000);
            Price = rnd.Next(0, 10000);
            IsProcessed = rnd.NextDouble() > 0.5;
        }

    }

    public class RandomStringHelper {
        static Random rnd = new Random();
        static string letters = "abcdefghijklmnopqrstuvwxyz";

        public static string GetRandomString() {
            int length = rnd.Next(6, 20);
            string retVal = ("" + letters[rnd.Next(25)]).ToUpper();

            for (int i = 0; i < length - 1; i++)
                retVal += letters[rnd.Next(25)];

            return retVal;
        }
    }
}
