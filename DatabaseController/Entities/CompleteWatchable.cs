using System.Collections.Generic;

namespace DatabaseController.Entities
{
    public class CompleteWatchable : Watchable
    {
        public List<Subtitles> Subtitles { get; set; }
        public string Path { get; set; }
    }
}
