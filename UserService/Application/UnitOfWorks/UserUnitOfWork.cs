﻿using UserService.Domain.Entities;
using UserService.Domain.Interface.IRepositories;
using UserService.Domain.Interface.UnitOfWork;
using UserService.Infrastructure.Data.DBContext;
using UserService.Infrastructure.Data.Repositories;


namespace UserService.Application.UnitOfWorks
{
    public class UserUnitOfWork : IUnitOfWork
    {
        private readonly UserContext _Context;
        private readonly IRepository<Account> _AccountRepository;
        private readonly IRepository<Customer> _CustomerRepository;
        private readonly IRepository<RefreshToken> _RefreshTokenRepository;
        public UserUnitOfWork(UserContext context)
        {
            _AccountRepository = new Repository<Account>(context);
            _CustomerRepository = new Repository<Customer>(context);
            _RefreshTokenRepository = new Repository<RefreshToken>(context);
            _Context = context;
        }

        public async Task Commit()
        {
            await _Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void Rollback()
        {
            if (_Context.Database.CurrentTransaction != null)
            {
                _Context.Database.CurrentTransaction.Rollback();
            }
        }

        public IRepository<Customer> CustomerRepository()
        {
            return this._CustomerRepository;
        }

        public IRepository<Account> AccountRepository()
        {
            return this._AccountRepository;
        }

        public IRepository<RefreshToken> RefreshTokenRepository()
        {
            return this._RefreshTokenRepository;
        }
    }
}
