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
    public class ContentPathsRepository : IContentPathsRepository
    {
        private TheaterContext _context;

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
            if (contPath != null)
            {
                contPath.Path = contentPath.Path;
                contPath.ContentType = contentPath.ContentType;
                _context.SaveChanges();
                return contPath;
            }
            throw new ArgumentException(); 
        }

        public ContentPath Delete(long id)
        {
            var path = Delete(GetById(id));
            return path;
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
