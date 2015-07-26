using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serpent_Theater_Server.Database.Models;

namespace Serpent_Theater_Server.Database
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
            Context.Actors.Remove(Context.Actors.FirstOrDefault(x => x.Name == "Michael"));
            Context.Episodes.Remove(Context.Episodes.FirstOrDefault(x => x.Title == "Test Title"));
            Context.Seasons.Remove(Context.Seasons.FirstOrDefault(x => x.Name == "Season 1"));
            Context.Series.Remove(Context.Series.FirstOrDefault(x => x.Title == "Some Series"));
            Context.SaveChanges();
        }
    }
}
