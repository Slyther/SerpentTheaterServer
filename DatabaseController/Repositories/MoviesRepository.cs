using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class MoviesRepository : IMoviesRepository
    {
        private TheaterContext _context;

        public MoviesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Movie Create(Movie movie)
        {
            throw new NotImplementedException();
        }

        public Movie GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Movie Update(Movie movie)
        {
            throw new NotImplementedException();
        }

        public Movie Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Movie Delete(Movie movie)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Movie> Query(Expression<Func<Movie, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
