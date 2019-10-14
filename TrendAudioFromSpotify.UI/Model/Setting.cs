using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAudioFromSpotify.UI.Model
{
    public class Setting
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public Setting() { }

        public Setting(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
