using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class ChangeSomgPositionMessage : ChangeItemPosition
    {
        public Audio Audio { get; set; }

        public ChangeSomgPositionMessage(Audio audio, int oldPosition, int newPosition)
        {
            Audio = audio;
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }

        public ChangeSomgPositionMessage()
        {

        }
    }
}
