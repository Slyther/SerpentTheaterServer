using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class ActorsRepository : IActorsRepository
    {
        private readonly TheaterContext _context;

        public ActorsRepository(TheaterContext context)
        {
            _context = context;
        }

        public Actor Create(Actor actor)
        {
            var ac = _context.Actors.Add(actor);
            _context.SaveChanges();
            return ac;
        }

        public Actor GetById(long id)
        {
            return _context.Actors.FirstOrDefault(x => x.Id == id);
        }

        public Actor Update(Actor actor)
        {
            var ac = _context.Actors.FirstOrDefault(cp => cp.Id == actor.Id);
            if (ac == null) throw new ArgumentException();
            _context.Entry(actor).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return ac;
        }

        public Actor Delete(long id)
        {
            return Delete(GetById(id));
        }

        public Actor Delete(Actor actor)
        {
            var ac = _context.Actors.Remove(actor);
            _context.SaveChanges();
            return ac;
        }

        public IQueryable<Actor> Query(Expression<Func<Actor, bool>> expression)
        {
            return _context.Actors.Where(expression);
        }
    }
}
