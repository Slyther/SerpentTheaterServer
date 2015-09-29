using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class EpisodesRepository : IEpisodesRepository
    {
        private readonly TheaterContext _context;

        public EpisodesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Episode Create(Episode episode)
        {
            var ep = _context.Episodes.Add(episode);
            _context.SaveChanges();
            return ep;
        }

        public Episode GetById(long id)
        {
            return _context.Episodes.FirstOrDefault(x => x.Id == id);
        }

        public Episode Update(Episode episode)
        {
            var ep = _context.Episodes.FirstOrDefault(cp => cp.Id == episode.Id);
            if (ep == null) throw new ArgumentException();
            _context.Entry(episode).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return ep;
        }

        public Episode Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Episode Delete(Episode episode)
        {
            var ep = _context.Episodes.Remove(episode);
            _context.SaveChanges();
            return ep;
        }

        public IQueryable<Episode> Query(Expression<Func<Episode, bool>> expression)
        {
            return _context.Episodes.Where(expression);
        }
    }
}
