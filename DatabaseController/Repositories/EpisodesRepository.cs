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
    public class EpisodesRepository : IEpisodesRepository
    {
        private TheaterContext _context;

        public EpisodesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Episode Create(Episode episode)
        {
            throw new NotImplementedException();
        }

        public Episode GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Episode Update(Episode episode)
        {
            throw new NotImplementedException();
        }

        public Episode Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Episode Delete(Episode episode)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Episode> Query(Expression<Func<Episode, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
