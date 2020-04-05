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
