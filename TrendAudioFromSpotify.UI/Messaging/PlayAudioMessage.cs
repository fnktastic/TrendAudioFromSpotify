using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Messaging
{
    public class PlayAudioMessage
    {
        public Audio Audio { get; set; }

        public PlayAudioMessage(Audio audio)
        {
            Audio = audio;
        }
    }
}
