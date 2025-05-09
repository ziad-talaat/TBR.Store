﻿using Application.EF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBL.Core.Contracts;

namespace TBL.EF.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ICategory Category { get; private set; }

        public IProductRepository Products { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Category=new CategoryRepository(_context) ;
            Products = new ProductRepoistory(_context);
        }
       

        public async Task CompleteAsync()
        {
           await  _context.SaveChangesAsync();
        }

       

        void IDisposable.Dispose()
        {
            _context.Dispose();
        }
    }
}
