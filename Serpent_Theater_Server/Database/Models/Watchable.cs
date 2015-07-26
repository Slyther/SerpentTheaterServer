using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serpent_Theater_Server.Database.Models
{
    public class Watchable
    {
        public int Id { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Rating { get; set; }
        public short RunTime { get; set; }
        public DateTime ReleaseDate { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Director> Directors { get; set; }
        public List<Writer> Writers { get; set; }
        public List<Actor> Actors { get; set; }
        public string ShortPlot { get; set; }
        public string LongPlot { get; set; }
        public string Language { get; set; }
        public byte[] Poster { get; set; }
        public decimal ImdbRating { get; set; }
    }
}
