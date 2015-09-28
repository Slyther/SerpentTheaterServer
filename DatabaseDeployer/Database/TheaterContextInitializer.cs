using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseDeployer.Database.Models;

namespace DatabaseDeployer.Database
{
    class TheaterContextInitializer
    {
        public TheaterContext Context;

        public TheaterContextInitializer() 
        {
            Context = new TheaterContext();
        }
        public void Initialize()
        {
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
            Context.Series.Add(series);
            Context.Seasons.Add(season);
            Context.Episodes.Add(episode);
            Context.Actors.Add(actor);
            Context.SaveChanges();
            series.ShortPlot = "A very interesting plot!";
            Context.SaveChanges();
            Context.Actors.Remove(Queryable.FirstOrDefault<Actor>(Context.Actors, x => x.Name == "Michael"));
            Context.Episodes.Remove(Queryable.FirstOrDefault<Episode>(Context.Episodes, x => x.Title == "Test Title"));
            Context.Seasons.Remove(Queryable.FirstOrDefault<Season>(Context.Seasons, x => x.Name == "Season 1"));
            Context.Series.Remove(Queryable.FirstOrDefault<Series>(Context.Series, x => x.Title == "Some Series"));
            Context.SaveChanges();
        }
    }
}
