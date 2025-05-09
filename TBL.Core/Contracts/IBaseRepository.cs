﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TBL.Core.Contracts
{
    public interface IBaseRepository<T>where T:class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(bool track);
        Task<T?> GetOneAsync<KEY>(KEY identifier);
        Task<T?> GetOneAsync<KEY>(KEY identifier,bool track);
        Task<T?> GetSpecific(Expression<Func<T,bool>>filter);
        Task<T?> GetSpecific(Expression<Func<T,bool>>filter,bool track);
        Task AddAsync(T item);
        //Task UpdateAsync(T item);
        void Remove(T item);
        void RemoveRange(IEnumerable<T> item);
    }
}
