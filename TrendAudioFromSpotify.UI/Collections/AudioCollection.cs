using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.UI.Model;

namespace TrendAudioFromSpotify.UI.Collections
{
    public class AudioCollection : ObservableCollection<Audio>
    {
        public AudioCollection()
        {
            CollectionChanged += AudioCollection_Changed;
        }

        public AudioCollection(IEnumerable<Audio> audios) : base(audios)
        {
            CollectionChanged += AudioCollection_Changed;
            RefreshNo();
        }

        private void AudioCollection_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Remove)
                RefreshNo();
        }

        private void RefreshNo()
        {
            for (int i = 0; i < this.Count(); i++)
                this.ElementAt(i).No = i+1;
        }
    }
}
