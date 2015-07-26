using System.Collections.Generic;
using Serpent_Theater_Server.Database.Models;

namespace Serpent_Theater_Server.Database
{
    public class Episode : CompleteWatchable
    {
        public int SeasonId { get; set; }
        public Season Season { get; set; }
    }
}