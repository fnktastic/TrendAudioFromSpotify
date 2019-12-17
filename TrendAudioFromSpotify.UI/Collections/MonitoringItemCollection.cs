using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Collections
{
    public class MonitoringItemCollection : ObservableCollection<MonitoringItem>
    {
        public MonitoringItemCollection()
        {

        }

        public MonitoringItemCollection(IEnumerable<MonitoringItem> monitoringItems) : base(monitoringItems)
        {

        }
    }
}
