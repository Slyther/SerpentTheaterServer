using System.Collections.Generic;
using Serpent_Theater_Server.Database.Models;

namespace Serpent_Theater_Server.Database
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
