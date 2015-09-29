namespace DatabaseController.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Portrait = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Watchables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ImdbId = c.String(),
                        Title = c.String(),
                        Year = c.String(),
                        Rating = c.String(),
                        RunTime = c.Short(nullable: false),
                        ReleaseDate = c.DateTime(nullable: false),
                        ShortPlot = c.String(),
                        LongPlot = c.String(),
                        Language = c.String(),
                        Poster = c.Binary(),
                        ImdbRating = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Path = c.String(),
                        SeasonId = c.Int(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Seasons", t => t.SeasonId)
                .Index(t => t.SeasonId);
            
            CreateTable(
                "dbo.Directors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Portrait = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Genres",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Writers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Portrait = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Subtitles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Language = c.Int(nullable: false),
                        Path = c.String(),
                        WatchableId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Watchables", t => t.WatchableId)
                .Index(t => t.WatchableId);
            
            CreateTable(
                "dbo.Seasons",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Plot = c.String(),
                        Poster = c.Binary(),
                        SeriesId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Watchables", t => t.SeriesId)
                .Index(t => t.SeriesId);
            
            CreateTable(
                "dbo.ContentPaths",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Path = c.String(),
                        ContentType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.WatchableActors",
                c => new
                    {
                        Watchable_Id = c.Int(nullable: false),
                        Actor_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Watchable_Id, t.Actor_Id })
                .ForeignKey("dbo.Watchables", t => t.Watchable_Id, cascadeDelete: true)
                .ForeignKey("dbo.Actors", t => t.Actor_Id, cascadeDelete: true)
                .Index(t => t.Watchable_Id)
                .Index(t => t.Actor_Id);
            
            CreateTable(
                "dbo.DirectorWatchables",
                c => new
                    {
                        Director_Id = c.Int(nullable: false),
                        Watchable_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Director_Id, t.Watchable_Id })
                .ForeignKey("dbo.Directors", t => t.Director_Id, cascadeDelete: true)
                .ForeignKey("dbo.Watchables", t => t.Watchable_Id, cascadeDelete: true)
                .Index(t => t.Director_Id)
                .Index(t => t.Watchable_Id);
            
            CreateTable(
                "dbo.GenreWatchables",
                c => new
                    {
                        Genre_Id = c.Int(nullable: false),
                        Watchable_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Genre_Id, t.Watchable_Id })
                .ForeignKey("dbo.Genres", t => t.Genre_Id, cascadeDelete: true)
                .ForeignKey("dbo.Watchables", t => t.Watchable_Id, cascadeDelete: true)
                .Index(t => t.Genre_Id)
                .Index(t => t.Watchable_Id);
            
            CreateTable(
                "dbo.WriterWatchables",
                c => new
                    {
                        Writer_Id = c.Int(nullable: false),
                        Watchable_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Writer_Id, t.Watchable_Id })
                .ForeignKey("dbo.Writers", t => t.Writer_Id, cascadeDelete: true)
                .ForeignKey("dbo.Watchables", t => t.Watchable_Id, cascadeDelete: true)
                .Index(t => t.Writer_Id)
                .Index(t => t.Watchable_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Seasons", "SeriesId", "dbo.Watchables");
            DropForeignKey("dbo.Watchables", "SeasonId", "dbo.Seasons");
            DropForeignKey("dbo.Subtitles", "WatchableId", "dbo.Watchables");
            DropForeignKey("dbo.WriterWatchables", "Watchable_Id", "dbo.Watchables");
            DropForeignKey("dbo.WriterWatchables", "Writer_Id", "dbo.Writers");
            DropForeignKey("dbo.GenreWatchables", "Watchable_Id", "dbo.Watchables");
            DropForeignKey("dbo.GenreWatchables", "Genre_Id", "dbo.Genres");
            DropForeignKey("dbo.DirectorWatchables", "Watchable_Id", "dbo.Watchables");
            DropForeignKey("dbo.DirectorWatchables", "Director_Id", "dbo.Directors");
            DropForeignKey("dbo.WatchableActors", "Actor_Id", "dbo.Actors");
            DropForeignKey("dbo.WatchableActors", "Watchable_Id", "dbo.Watchables");
            DropIndex("dbo.WriterWatchables", new[] { "Watchable_Id" });
            DropIndex("dbo.WriterWatchables", new[] { "Writer_Id" });
            DropIndex("dbo.GenreWatchables", new[] { "Watchable_Id" });
            DropIndex("dbo.GenreWatchables", new[] { "Genre_Id" });
            DropIndex("dbo.DirectorWatchables", new[] { "Watchable_Id" });
            DropIndex("dbo.DirectorWatchables", new[] { "Director_Id" });
            DropIndex("dbo.WatchableActors", new[] { "Actor_Id" });
            DropIndex("dbo.WatchableActors", new[] { "Watchable_Id" });
            DropIndex("dbo.Seasons", new[] { "SeriesId" });
            DropIndex("dbo.Subtitles", new[] { "WatchableId" });
            DropIndex("dbo.Watchables", new[] { "SeasonId" });
            DropTable("dbo.WriterWatchables");
            DropTable("dbo.GenreWatchables");
            DropTable("dbo.DirectorWatchables");
            DropTable("dbo.WatchableActors");
            DropTable("dbo.ContentPaths");
            DropTable("dbo.Seasons");
            DropTable("dbo.Subtitles");
            DropTable("dbo.Writers");
            DropTable("dbo.Genres");
            DropTable("dbo.Directors");
            DropTable("dbo.Watchables");
            DropTable("dbo.Actors");
        }
    }
}
