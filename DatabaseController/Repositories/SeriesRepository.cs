using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class SeriesRepository : ISeriesRepository
    {
        private readonly TheaterContext _context;

        public SeriesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Series Create(Series series)
        {
            var sers = _context.Series.Add(series);
            _context.SaveChanges();
            return sers;
        }

        public Series GetById(long id)
        {
            return _context.Series.FirstOrDefault(x => x.Id == id);
        }

        public Series Update(Series series)
        {
            var sers = _context.Series.FirstOrDefault(cp => cp.Id == series.Id);
            if (sers == null) throw new ArgumentException();
            _context.Entry(series).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return sers;
        }

        public Series Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Series Delete(Series series)
        {
            var sers = _context.Series.Remove(series);
            _context.SaveChanges();
            return sers;
        }

        public IQueryable<Series> Query(Expression<Func<Series, bool>> expression)
        {
            return _context.Series.Where(expression);
        }
    }
}
