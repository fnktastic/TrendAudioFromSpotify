using System.Collections.Generic;
using System.Collections.ObjectModel;
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
