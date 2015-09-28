using System.Collections.Generic;
using DatabaseDeployer.Database.Models;

namespace DatabaseDeployer.Database
{
    public class Series : Watchable
    {
        public List<Season> Seasons { get; set; }
    }
}
