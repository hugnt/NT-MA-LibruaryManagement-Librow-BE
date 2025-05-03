using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Librow.Infrastructure.Repositories.Base;
public interface IRepository<T>
{
    public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                CancellationToken token = default,
                                params Expression<Func<T, object>>[] navigationProperties);

    public Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, bool>>? predicate = null,
                                                            Expression<Func<T, TResult>>? selectQuery = null,
                                                            CancellationToken token = default,
                                                            params Expression<Func<T, object>>[] navigationProperties);

    public Task<(IEnumerable<TResult> Data, int TotalCount)> GetByFilterAsync<TResult>(int? pageSize, int? pageNumber,
                                                                                        Expression<Func<T, TResult>> selectQuery,
                                                                                        Expression<Func<T, bool>>? predicate = null,
                                                                                        List<(Expression<Func<TResult, object>> KeySelector, bool IsAsc)>? orderBy = null,
                                                                                        CancellationToken token = default,
                                                                                        params Expression<Func<T, object>>[] navigationProperties);
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null,
                                        CancellationToken token = default,
                                        params Expression<Func<T, object>>[] navigationProperties);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>>? predicate = null,
                                    Expression<Func<T, TResult>> selectQuery = null,
                                    CancellationToken token = default,
                                    params Expression<Func<T, object>>[] navigationProperties);
    public Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties);
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties);
    public Task AddRangeAsync(List<T> listEntity);
    public void Add(T entity);
    public void Update(T entity);
    public void Delete(T entity);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
    public Task ExecuteDeleteAsync(Expression<Func<T, bool>>? predicate = null);
    public Task ExecuteUpdateAsync(Expression<Func<T, bool>> predicate, 
                                   Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression,
                                   CancellationToken cancellationToken = default);
    public Task BeginTransactionAsync();
    public Task CommitAsync();
    public Task RollbackAsync();
}
