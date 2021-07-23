using Caliburn.Micro;
using QuantBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Ch03
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class CustomerViewModel : Screen
    {
        private readonly IEventAggregator _events;
        [ImportingConstructor]
        public CustomerViewModel(IEventAggregator events)
        {
            this._events = events;
            DisplayName = "01. Customers";
            MyCustomers = new BindableCollection<Customer>();
        }

        public BindableCollection<Customer> MyCustomers { get; set; }

        public async void GetData()
        {
            using (var db = new NorthwindEntities())
            {
                var query = from d in db.Customers select d;
                MyCustomers.Clear();
                MyCustomers.AddRange(query);
            }
            await _events.PublishOnUIThreadAsync(new ModelEvents(new List<object>(new object[] { "From Customers: Count = " + MyCustomers.Count.ToString() })));
        }
    }
}
