using System.Collections.Generic;
using DatabaseDeployer.Database.Models;

namespace DatabaseDeployer.Database
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
