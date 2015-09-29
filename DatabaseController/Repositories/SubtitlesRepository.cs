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
    public class SubtitlesRepository : ISubtitlesRepository
    {
        private TheaterContext _context;

        public SubtitlesRepository(TheaterContext context)
        {
            _context = context;
        }

        public Subtitles Create(Subtitles subtitles)
        {
            throw new NotImplementedException();
        }

        public Subtitles GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Subtitles Update(Subtitles subtitles)
        {
            throw new NotImplementedException();
        }

        public Subtitles Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Subtitles Delete(Subtitles subtitles)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Subtitles> Query(Expression<Func<Subtitles, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
