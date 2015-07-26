using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serpent_Theater_Server.Database.Models
{
    public class CompleteWatchable : Watchable
    {
        public List<Subtitles> Subtitles { get; set; }
        public string Path { get; set; }
    }
}
