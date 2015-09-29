using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IDirectorsRepository
    {
        Director Create(Director director);
        Director GetById(long id);
        Director Update(Director director);
        Director Delete(long id);
        Director Delete(Director director);
        IQueryable<Director> Query(Expression<Func<Director, bool>> expression);
    }
}
