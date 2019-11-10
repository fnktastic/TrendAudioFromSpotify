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

        public DbSet<Audio> Audios { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Audio>().HasKey(a => a.Id);

            modelBuilder.Entity<Playlist>().HasKey(p => p.Id);

            modelBuilder.Entity<PlaylistAudio>()
                .HasKey(pa => new { pa.PlaylistId, pa.AudioId });

            modelBuilder.Entity<PlaylistAudio>()
                .HasRequired(pa => pa.Audio)
                .WithMany(p => p.PlaylistAudios)
                .HasForeignKey(pc => pc.AudioId);

            modelBuilder.Entity<PlaylistAudio>()
                .HasRequired(pc => pc.Playlist)
                .WithMany(c => c.PlaylistAudios)
                .HasForeignKey(pc => pc.PlaylistId);

            base.OnModelCreating(modelBuilder);
        }
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
