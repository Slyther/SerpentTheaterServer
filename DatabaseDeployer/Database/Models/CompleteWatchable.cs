using System.Collections.Generic;

namespace DatabaseDeployer.Database.Models
{
    public class CompleteWatchable : Watchable
    {
        public List<Subtitles> Subtitles { get; set; }
        public string Path { get; set; }
    }
}
