using System.Collections.Generic;
using System.Collections.ObjectModel;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Collections
{
    public class GroupCollection : ObservableCollection<Group>
    {
        public GroupCollection()
        {
        }

        public GroupCollection(IEnumerable<Group> groups) : base(groups)
        {

        }
    }
}
