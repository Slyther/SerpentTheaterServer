using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IContentPathsRepository
    {
        ContentPath Create(ContentPath contentPath);
        ContentPath GetById(long id);
        ContentPath Update(ContentPath contentPath);
        ContentPath Delete(long id);
        ContentPath Delete(ContentPath contentPath);
        IQueryable<ContentPath> Query(Expression<Func<ContentPath, bool>> expression);
    }
}
