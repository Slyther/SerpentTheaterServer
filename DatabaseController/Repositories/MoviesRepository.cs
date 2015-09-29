using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class MoviesRepository : IMoviesRepository
    {
        private readonly TheaterContext _context;

        public MoviesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Movie Create(Movie movie)
        {
            var mov = _context.Movies.Add(movie);
            _context.SaveChanges();
            return mov;
        }

        public Movie GetById(long id)
        {
            return _context.Movies.FirstOrDefault(x => x.Id == id);
        }

        public Movie Update(Movie movie)
        {
            var mov = _context.Movies.FirstOrDefault(cp => cp.Id == movie.Id);
            if (mov == null) throw new ArgumentException();
            _context.Entry(movie).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return mov;
        }

        public Movie Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Movie Delete(Movie movie)
        {
            var mov = _context.Movies.Remove(movie);
            _context.SaveChanges();
            return mov;
        }

        public IQueryable<Movie> Query(Expression<Func<Movie, bool>> expression)
        {
            return _context.Movies.Where(expression);
        }
    }
}
