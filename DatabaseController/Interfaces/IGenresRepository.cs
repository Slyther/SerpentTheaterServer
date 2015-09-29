using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IGenresRepository
    {
        Genre Create(Genre genre);
        Genre GetById(long id);
        Genre Update(Genre genre);
        Genre Delete(long id);
        Genre Delete(Genre genre);
        IQueryable<Genre> Query(Expression<Func<Genre, bool>> expression);
    }
}
