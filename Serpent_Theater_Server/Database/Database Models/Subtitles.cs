﻿using Serpent_Theater_Server.Database.Models;

namespace Serpent_Theater_Server.Database
{
    public enum SubtitleLanguage
    {
        English,
        Spanish
    }
    public class Subtitles
    {
        public int Id { get; set; }
        public SubtitleLanguage Language { get; set; }
        public string Path { get; set; }
        public int WatchableId { get; set; }
        public CompleteWatchable Watchable { get; set; }
    }
}