using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.UI.Utility
{
    public static class UrlValidator
    {
        public static Uri Validate(string s)
        {
            var result = Uri.TryCreate(s, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
                return uriResult;

            return null;
        }
    }
}
