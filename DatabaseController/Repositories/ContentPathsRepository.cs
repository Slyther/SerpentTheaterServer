using System;
using System.Linq;
using System.Linq.Expressions;
using DatabaseController.Context;
using DatabaseController.Entities;
using DatabaseController.Interfaces;

namespace DatabaseController.Repositories
{
    public class ContentPathsRepository : IContentPathsRepository
    {
        private readonly TheaterContext _context;

        public ContentPathsRepository(TheaterContext context)
        {
            _context = context;
        }

        public ContentPath Create(ContentPath contentPath)
        {
            var path = _context.ContentPaths.Add(contentPath);
            _context.SaveChanges();
            return path;
        }

        public ContentPath GetById(long id)
        {
            return _context.ContentPaths.FirstOrDefault(x => x.Id == id);
        }

        public ContentPath Update(ContentPath contentPath)
        {
            var contPath = _context.ContentPaths.FirstOrDefault(cp => cp.Id == contentPath.Id);
            if (contPath == null) throw new ArgumentException();
            _context.Entry(contentPath).State = System.Data.Entity.EntityState.Modified;
            _context.SaveChanges();
            return contPath;
        }

        public ContentPath Delete(long id)
        {
            return Delete(GetById(id));
        }

        public ContentPath Delete(ContentPath contentPath)
        {
            var path = _context.ContentPaths.Remove(contentPath);
            _context.SaveChanges();
            return path;
        }

        public IQueryable<ContentPath> Query(Expression<Func<ContentPath, bool>> expression)
        {
            return _context.ContentPaths.Where(expression);
        }
    }
}
