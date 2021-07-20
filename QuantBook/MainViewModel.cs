using Caliburn.Micro;
using QuantBook.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuantBook
{
    [Export(typeof(MainViewModel))]
    public class MainViewModel : Conductor<IScreen>.Collection.OneActive, IConductor, IHandle<ModelEvents>
    {
        #region bindable properties
        private string chapter;
        public string Chapter
        {
            get { return chapter; }
            set
            {
                chapter = value;
                NotifyOfPropertyChange(() => Chapter);
            }
        }

        private int progressMin = 0;
        public int ProgressMin
        {
            get { return progressMin; }
            set
            {
                progressMin = value;
                NotifyOfPropertyChange(() => ProgressMin);
            }
        }

        private int progressMax = 1;
        public int ProgressMax
        {
            get { return progressMax; }
            set
            {
                progressMax = value;
                NotifyOfPropertyChange(() => ProgressMax);
            }
        }

        private int progressValue = 0;
        public int ProgressValue
        {
            get { return progressValue; }
            set
            {
                progressValue = value;
                NotifyOfPropertyChange(() => ProgressValue);
            }
        }
        private string statusText;    

        public string StatusText
        {
            get { return statusText; }
            set
            {
                statusText = value;
                NotifyOfPropertyChange(() => StatusText);
            }
        }

        #endregion

        private readonly IEnumerable<IScreen> screens;
        private readonly IEventAggregator events;

        [ImportingConstructor]
        public MainViewModel([ImportMany]IEnumerable<IScreen> screens, IEventAggregator events)
        {
            this.screens = screens;
            this.events = events;
            Items.Clear();
            this.events.SubscribeOnPublishedThread(this);
            DisplayName = "Quant Book Project";
        }

        public void OnClick(object sender)
        {
            Button btn = sender as Button;
            int num = Convert.ToInt16(btn.Content.ToString().Split(' ')[1]);
            string ch = "Ch";
            if (num < 10)
                ch += "0" + num.ToString();
            else
                ch += num.ToString();

            Items.Clear();
            var sc = from q in screens where q.ToString().Contains(ch) orderby q.DisplayName select q;
            Items.AddRange(sc);

            var view = this.GetView() as MainView;
            foreach (Button b in view.buttonPanel.Children)
            {
                b.Background = Brushes.Transparent;
                b.Foreground = Brushes.Black;
            }

            btn.Background = Brushes.Black;
            btn.Foreground = Brushes.White;
        }

        public Task HandleAsync(ModelEvents message, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var events = message.EventList;
                StatusText = events.First().ToString();
                if (events.Count > 1)
                {
                    ProgressMin = Convert.ToInt32(events[1]);
                    ProgressMax = Convert.ToInt32(events[2]);
                    ProgressValue = Convert.ToInt32(events[3]);
                }
            }, cancellationToken);
        }
    }
}