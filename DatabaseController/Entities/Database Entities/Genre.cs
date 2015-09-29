using System.Collections.Generic;

namespace DatabaseController.Entities
{
    public class Genre
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
