using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Entities;

namespace DatabaseController.Interfaces
{
    public interface IActorsRepository
    {
        Actor Create(Actor actor);
        Actor GetById(long id);
        Actor Update(Actor actor);
        Actor Delete(long id);
        Actor Delete(Actor actor);
        IQueryable<Actor> Query(Expression<Func<Actor, bool>> expression);
    }
}
