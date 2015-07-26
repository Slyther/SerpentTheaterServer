﻿using System.Collections.Generic;

namespace Serpent_Theater_Server.Database
{
    public class Season
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Plot { get; set; }
        public byte[] Poster { get; set; }
        public int SeriesId { get; set; }
        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }
    }
}
