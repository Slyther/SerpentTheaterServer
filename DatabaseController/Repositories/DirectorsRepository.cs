using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class DirectorsRepository : IDirectorsRepository
    {
        private readonly TheaterContext _context;

        public DirectorsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Director Create(Director director)
        {
            var dir = _context.Directors.Add(director);
            _context.SaveChanges();
            return dir;
        }

        public Director GetById(long id)
        {
            return _context.Directors.FirstOrDefault(x => x.Id == id);
        }

        public Director Update(Director director)
        {
            var dir = _context.Directors.FirstOrDefault(cp => cp.Id == director.Id);
            if (dir == null) throw new ArgumentException();
            _context.Entry(director).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return dir;
        }

        public Director Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Director Delete(Director director)
        {
            var dir = _context.Directors.Remove(director);
            _context.SaveChanges();
            return dir;
        }

        public IQueryable<Director> Query(Expression<Func<Director, bool>> expression)
        {
            return _context.Directors.Where(expression);
        }
    }
}
