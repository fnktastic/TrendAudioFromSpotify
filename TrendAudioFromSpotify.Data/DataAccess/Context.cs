using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.DataAccess
{
    public class Context : DbContext
    {
        public Context() : base("TrendifyDb")
        {
                
        }

        public DbSet<Playlist> Playlists { get; set; }
    }

    public class DataInitializer : DropCreateDatabaseIfModelChanges<Context>
    {
        public DataInitializer()
        {
            using (var context = new Context())
            {
                InitializeDatabase(context);
            }
        }
    }
}
