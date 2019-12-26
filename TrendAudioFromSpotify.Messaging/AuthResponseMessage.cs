using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.Messaging
{
    public class AuthResponseMessage
    {
        public string AccessToken { get; set; }

        public AuthResponseMessage(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}
