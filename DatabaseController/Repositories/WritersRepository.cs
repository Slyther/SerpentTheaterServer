using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class WritersRepository : IWritersRepository
    {
        private readonly TheaterContext _context;

        public WritersRepository(TheaterContext context)
        {
            _context = context;
        }

        public Writer Create(Writer writer)
        {
            var wr = _context.Writers.Add(writer);
            _context.SaveChanges();
            return wr;
        }

        public Writer GetById(long id)
        {
            return _context.Writers.FirstOrDefault(x => x.Id == id);
        }

        public Writer Update(Writer writer)
        {
            var wr = _context.Writers.FirstOrDefault(cp => cp.Id == writer.Id);
            if (wr == null) throw new ArgumentException();
            _context.Entry(writer).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return wr;
        }

        public Writer Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Writer Delete(Writer writer)
        {
            var wr = _context.Writers.Remove(writer);
            _context.SaveChanges();
            return wr;
        }

        public IQueryable<Writer> Query(Expression<Func<Writer, bool>> expression)
        {
            return _context.Writers.Where(expression);
        }
    }
}
