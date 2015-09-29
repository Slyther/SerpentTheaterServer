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
    public class DirectorsRepository : IDirectorsRepository
    {
        private TheaterContext _context;

        public DirectorsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Director Create(Director director)
        {
            throw new NotImplementedException();
        }

        public Director GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Director Update(Director director)
        {
            throw new NotImplementedException();
        }

        public Director Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Director Delete(Director director)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Director> Query(Expression<Func<Director, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
