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
    public class SeasonsRepository : ISeasonsRepository
    {
        private TheaterContext _context;

        public SeasonsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Season Create(Season season)
        {
            throw new NotImplementedException();
        }

        public Season GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Season Update(Season season)
        {
            throw new NotImplementedException();
        }

        public Season Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Season Delete(Season season)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Season> Query(Expression<Func<Season, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
