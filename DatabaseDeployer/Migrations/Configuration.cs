using System.Collections.Generic;
using DatabaseDeployer.Database;
using DatabaseDeployer.Database.Models;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace DatabaseDeployer.Migrations
{
    public sealed class Configuration : DbMigrationsConfiguration<TheaterContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(TheaterContext context)
        {
            if (context.Movies.Any())
                return;
            var episode = new Episode
            {
                Title = "Test Title",
                ReleaseDate = DateTime.UtcNow
            };

            var season = new Season
            {
                Name = "Season 1",
            };
            var series = new Series
            {
                Title = "Some Series",
                ReleaseDate = DateTime.UtcNow
            };
            var actor = new Actor
            {
                Name = "Michael",
                Watchables = new List<Watchable>()
            };
            season.Series = series;
            episode.Season = season;
            actor.Watchables.Add(series);
            actor.Watchables.Add(episode);
            context.Series.Add(series);
            context.Seasons.Add(season);
            context.Episodes.Add(episode);
            context.Actors.Add(actor);
            context.SaveChanges();
            series.ShortPlot = "A very interesting plot!";
            context.SaveChanges();
            context.Actors.Remove(context.Actors.FirstOrDefault(x => x.Name == "Michael"));
            context.Episodes.Remove(context.Episodes.FirstOrDefault(x => x.Title == "Test Title"));
            context.Seasons.Remove(context.Seasons.FirstOrDefault(x => x.Name == "Season 1"));
            context.Series.Remove(context.Series.FirstOrDefault(x => x.Title == "Some Series"));
            context.SaveChanges();
        }
    }
}
