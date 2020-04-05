using SpotifyAPI.Web.Models;

namespace TrendAudioFromSpotify.UI.Extensions
{
    public static class PlaylistExtensions
    {
        public static SimplePlaylist ToSimple(this FullPlaylist fullPlaylist)
        {
            return new SimplePlaylist()
            {
                Collaborative = fullPlaylist.Collaborative,
                Href = fullPlaylist.Href,
                Error = fullPlaylist.Error,
                ExternalUrls = fullPlaylist.ExternalUrls,
                Id = fullPlaylist.Id,
                Images = fullPlaylist.Images,
                Name = fullPlaylist.Name,
                Owner = fullPlaylist.Owner,
                Public = fullPlaylist.Public,
                SnapshotId = fullPlaylist.SnapshotId,
                Type = fullPlaylist.Type,
                Uri = fullPlaylist.Uri,
                Tracks = fullPlaylist.Tracks.ToPlaylistTrackCollection()
            };
        }

        public static PlaylistTrackCollection ToPlaylistTrackCollection(this Paging<PlaylistTrack> paging)
        {
            return new PlaylistTrackCollection()
            {
                Total = paging.Total,
                Href = paging.Href
            };
        }
    }
}
