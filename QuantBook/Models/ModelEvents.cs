using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantBook.Models
{
    public class ModelEvents
    {
        public List<object> EventList { get; }

        public ModelEvents(List<object> eventList)
        {
            this.EventList = eventList;
        }
    }
}
