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
    public class SeriesRepository : ISeriesRepository
    {
        private TheaterContext _context;

        public SeriesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Series Create(Series series)
        {
            throw new NotImplementedException();
        }

        public Series GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Series Update(Series series)
        {
            throw new NotImplementedException();
        }

        public Series Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Series Delete(Series series)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Series> Query(Expression<Func<Series, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
