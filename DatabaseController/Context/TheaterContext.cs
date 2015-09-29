using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DatabaseController.Entities;

namespace DatabaseController.Context
{
    public class TheaterContext : DbContext
    {
        public TheaterContext() : base("TheaterContext") {}
        public DbSet<Actor> Actors { get; set; }
        public DbSet<Director> Directors { get; set; }
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Season> Seasons { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Subtitles> Subtitles { get; set; }
        public DbSet<Writer> Writers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        } 
    }
}
