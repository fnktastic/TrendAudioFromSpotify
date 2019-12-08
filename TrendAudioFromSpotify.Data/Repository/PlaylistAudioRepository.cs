using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IPlaylistAudioRepository
    {

    }

    public class PlaylistAudioRepository : IPlaylistAudioRepository
    {
        private readonly Context _context;

        public PlaylistAudioRepository(Context context)
        {
            _context = context;
        }
    }
}
