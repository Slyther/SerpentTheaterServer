using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IEpisodesRepository
    {
        Episode Create(Episode episode);
        Episode GetById(long id);
        Episode Update(Episode episode);
        Episode Delete(long id);
        Episode Delete(Episode episode);
        IQueryable<Episode> Query(Expression<Func<Episode, bool>> expression);
    }
}
