namespace TrendAudioFromSpotify.UI.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddedIsPublicpropertytoPlaylistDto : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Playlist", "IsPublic", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Playlist", "IsPublic");
        }
    }
}
