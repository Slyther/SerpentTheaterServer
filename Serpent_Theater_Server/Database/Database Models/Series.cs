using System.Collections.Generic;
using Serpent_Theater_Server.Database.Models;

namespace Serpent_Theater_Server.Database
{
    public class Series : Watchable
    {
        public List<Season> Seasons { get; set; }
    }
}
