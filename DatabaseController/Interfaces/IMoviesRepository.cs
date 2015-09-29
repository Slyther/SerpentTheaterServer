using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IMoviesRepository
    {
        Movie Create(Movie movie);
        Movie GetById(long id);
        Movie Update(Movie movie);
        Movie Delete(long id);
        Movie Delete(Movie movie);
        IQueryable<Movie> Query(Expression<Func<Movie, bool>> expression);
    }
}
