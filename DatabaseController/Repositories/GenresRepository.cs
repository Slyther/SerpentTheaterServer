using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class GenresRepository : IGenresRepository
    {
        private readonly TheaterContext _context;

        public GenresRepository(TheaterContext context)
        {
            _context = context;
        }

        public Genre Create(Genre genre)
        {
            var gen = _context.Genres.Add(genre);
            _context.SaveChanges();
            return gen;
        }

        public Genre GetById(long id)
        {
            return _context.Genres.FirstOrDefault(x => x.Id == id);
        }

        public Genre Update(Genre genre)
        {
            var gen = _context.Genres.FirstOrDefault(cp => cp.Id == genre.Id);
            if (gen == null) throw new ArgumentException();
            _context.Entry(genre).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return gen;
        }

        public Genre Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Genre Delete(Genre genre)
        {
            var gen = _context.Genres.Remove(genre);
            _context.SaveChanges();
            return gen;
        }

        public IQueryable<Genre> Query(Expression<Func<Genre, bool>> expression)
        {
            return _context.Genres.Where(expression);
        }
    }
}
