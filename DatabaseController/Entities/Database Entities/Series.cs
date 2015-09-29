using System.Collections.Generic;

namespace DatabaseController.Entities
{
    public class Series : Watchable
    {
        public List<Season> Seasons { get; set; }
    }
}
