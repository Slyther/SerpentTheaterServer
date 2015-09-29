using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class SubtitlesRepository : ISubtitlesRepository
    {
        private readonly TheaterContext _context;

        public SubtitlesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Subtitles Create(Subtitles subtitles)
        {
            var subs = _context.Subtitles.Add(subtitles);
            _context.SaveChanges();
            return subs;
        }

        public Subtitles GetById(long id)
        {
            return _context.Subtitles.FirstOrDefault(x => x.Id == id);
        }

        public Subtitles Update(Subtitles subtitles)
        {
            var subs = _context.Subtitles.FirstOrDefault(cp => cp.Id == subtitles.Id);
            if (subs == null) throw new ArgumentException();
            _context.Entry(subtitles).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return subs;
        }

        public Subtitles Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Subtitles Delete(Subtitles subtitles)
        {
            var subs = _context.Subtitles.Remove(subtitles);
            _context.SaveChanges();
            return subs;
        }

        public IQueryable<Subtitles> Query(Expression<Func<Subtitles, bool>> expression)
        {
            return _context.Subtitles.Where(expression);
        }
    }
}
