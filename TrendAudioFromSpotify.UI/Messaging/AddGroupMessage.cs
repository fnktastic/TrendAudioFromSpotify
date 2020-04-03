using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class AddGroupMessage
    {
        public Group Group { get; set; }

        public AddGroupMessage(Group group)
        {
            Group = group;
        }
    }
}
