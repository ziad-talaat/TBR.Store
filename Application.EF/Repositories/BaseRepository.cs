using Application.EF.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;

namespace TBL.EF.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        public BaseRepository(AppDbContext context)
        {
            _context = context; 
        }
        public async Task AddAsync(T item)
        {
            await  _context.AddAsync(item);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var items=await _context.Set<T>().ToListAsync();
            return items;
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool track)
        {
            IQueryable<T> query = GetQuery();
            if (track == false)
            {
                query=query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter, bool track)
        {
            IQueryable<T> query = GetQuery();
            if (track == false)
            {
                query = query.AsNoTracking();
            }
            return await query.Where(filter).ToListAsync();
        }

        public async Task<T?> GetOneAsync<KEY>(KEY identifier)
        {
           T? item=await  _context.Set<T>().FindAsync(identifier);
            return item;
        }

        public async Task<T?> GetOneAsync<KEY>(KEY identifier, bool track)
        {
            IQueryable<T> query = GetQuery();
            if (track == false)
            {
                query = query.AsNoTracking();
            }

            var pk = _context.Model.FindEntityType(typeof(T)).FindPrimaryKey();

            if(pk==null)
            {
                throw new InvalidOperationException("No primary key defined for entity.");
            }

            string? identifierName= pk.Properties.FirstOrDefault()?.Name;

            var parameter = Expression.Parameter(typeof(T), "x");
            var property=Expression.Property(parameter, identifierName);

            var propertyType = property.Type;
            var changedType=Convert.ChangeType(identifier, propertyType);

            var constant=Expression.Constant(changedType,propertyType);

            var equal = Expression.Equal(property, constant);
            var lambda=Expression.Lambda<Func<T,bool>>(equal, parameter);
            return await query.FirstOrDefaultAsync(lambda);

        }

        public async Task<T?> GetSpecific(Expression<Func<T, bool>> filter)
        {
            T? item = await _context.Set<T>().FirstOrDefaultAsync(filter);
            return item;
        }

        public async Task<T?> GetSpecific(Expression<Func<T, bool>> filter, bool track)
        {
            IQueryable<T> query = GetQuery();
            if (track == false)
            {
                query = query.AsNoTracking();
            }
            return  await query.FirstOrDefaultAsync(filter);
        }

        public async Task<T?> GetSpecific(Expression<Func<T, bool>> filter, bool track, string[] includes)
        {
            IQueryable<T> query = GetQuery();
            if (track == false)
            {
                query = query.AsNoTracking();
            }
            if (includes != null)
            {
                foreach(var include in includes)
                {
                    query=query.Include(include);
                }
            }
            return await query.FirstOrDefaultAsync(filter);
        }

        public void Remove(T item)
        {
           _context.Set<T>().Remove(item);   
            
        }

        public void RemoveRange(IEnumerable<T> item)
        {
            _context.Set<T>().RemoveRange(item);
        }

        public void Update(T item)
        {
            _context.Set<T>().Update(item);   
        }

        private IQueryable<T>GetQuery()
        {
            IQueryable<T> query=_context.Set<T>().AsQueryable();
            return query;   
        }
    }
}
