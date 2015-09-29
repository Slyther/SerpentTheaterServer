using System.Collections.Generic;

namespace DatabaseController.Entities
{
    public class Individual
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Portrait { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
