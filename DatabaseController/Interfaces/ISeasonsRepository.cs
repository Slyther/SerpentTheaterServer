using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface ISeasonsRepository
    {
        Season Create(Season season);
        Season GetById(long id);
        Season Update(Season season);
        Season Delete(long id);
        Season Delete(Season season);
        IQueryable<Season> Query(Expression<Func<Season, bool>> expression);
    }
}
