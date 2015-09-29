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
    public class ActorsRepository : IActorsRepository
    {
        private TheaterContext _context;

        public ActorsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Actor Create(Actor actor)
        {
            throw new NotImplementedException();
        }

        public Actor GetById(long id)
        {
            throw new NotImplementedException();
        }

        public Actor Update(Actor actor)
        {
            throw new NotImplementedException();
        }

        public Actor Delete(long id)
        {
            throw new NotImplementedException();
        }

        public Actor Delete(Actor actor)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Actor> Query(Expression<Func<Actor, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
