using DatabaseDeployer.Database.Models;

namespace DatabaseDeployer.Database
{
    public class Episode : CompleteWatchable
    {
        public int SeasonId { get; set; }
        public Season Season { get; set; }
    }
}