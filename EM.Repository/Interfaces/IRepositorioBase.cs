using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using EM.Domain.Interface;

namespace EM.Repository.Interfaces
{
    public interface IRepositorioBase<T> where T : IEntidade
    {
        void Add(T entidade);
        void Remove(T entidade);
        void Update(T entidade);
        IEnumerable<T> GetAll();
        IEnumerable<T> Get(Expression<Func<T, bool>> predicate);
        T? GetById(int id);
        bool Exists(int id);
        int Count();
    }
}

