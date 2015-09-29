using System.Collections.Generic;

namespace DatabaseController.Entities
{
    public class Genre
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
