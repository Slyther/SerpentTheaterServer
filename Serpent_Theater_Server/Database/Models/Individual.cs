using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serpent_Theater_Server.Database.Models
{
    public class Individual
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Portrait { get; set; }
        public List<Watchable> Watchables { get; set; }
    }
}
