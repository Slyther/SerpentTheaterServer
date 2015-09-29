using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IWritersRepository
    {
        Writer Create(Writer writer);
        Writer GetById(long id);
        Writer Update(Writer writer);
        Writer Delete(long id);
        Writer Delete(Writer writer);
        IQueryable<Writer> Query(Expression<Func<Writer, bool>> expression);
    }
}
