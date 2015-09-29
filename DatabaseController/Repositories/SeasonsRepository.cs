using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class SeasonsRepository : ISeasonsRepository
    {
        private readonly TheaterContext _context;

        public SeasonsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Season Create(Season season)
        {
            var seas = _context.Seasons.Add(season);
            _context.SaveChanges();
            return seas;
        }

        public Season GetById(long id)
        {
            return _context.Seasons.FirstOrDefault(x => x.Id == id);
        }

        public Season Update(Season season)
        {
            var seas = _context.Seasons.FirstOrDefault(cp => cp.Id == season.Id);
            if (seas == null) throw new ArgumentException();
            _context.Entry(season).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return seas;
        }

        public Season Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Season Delete(Season season)
        {
            var seas = _context.Seasons.Remove(season);
            _context.SaveChanges();
            return seas;
        }

        public IQueryable<Season> Query(Expression<Func<Season, bool>> expression)
        {
            return _context.Seasons.Where(expression);
        }
    }
}
