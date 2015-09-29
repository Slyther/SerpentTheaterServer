using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface ISeriesRepository
    {
        Series Create(Series series);
        Series GetById(long id);
        Series Update(Series series);
        Series Delete(long id);
        Series Delete(Series series);
        IQueryable<Series> Query(Expression<Func<Series, bool>> expression);
    }
}
