using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface ISubtitlesRepository
    {
        Subtitles Create(Subtitles subtitles);
        Subtitles GetById(long id);
        Subtitles Update(Subtitles subtitles);
        Subtitles Delete(long id);
        Subtitles Delete(Subtitles subtitles);
        IQueryable<Subtitles> Query(Expression<Func<Subtitles, bool>> expression);
    }
}
