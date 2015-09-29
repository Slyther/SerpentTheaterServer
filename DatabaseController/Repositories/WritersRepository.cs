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
    public class WritersRepository : IWritersRepository
    {
        private TheaterContext _context;

        public WritersRepository(TheaterContext context)
        {
            _context = context;
        }

        public Writer Create(Writer writer)
        {
            throw new NotImplementedException();
        }

        public Writer GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Writer Update(Writer writer)
        {
            throw new NotImplementedException();
        }

        public Writer Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Writer Delete(Writer writer)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Writer> Query(Expression<Func<Writer, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
