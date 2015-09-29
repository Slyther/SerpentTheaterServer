using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class GenresRepository : IGenresRepository
    {
        private TheaterContext _context;

        public GenresRepository(TheaterContext context)
        {
            _context = context;
        }

        public Genre Create(Genre genre)
        {
            throw new NotImplementedException();
        }

        public Genre GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Genre Update(Genre genre)
        {
            throw new NotImplementedException();
        }

        public Genre Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Genre Delete(Genre genre)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Genre> Query(Expression<Func<Genre, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
