using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Librow.Infrastructure.Extensions;

namespace Librow.Infrastructure.Repositories.Base;
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected IDbContextTransaction? _transaction;
    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }
    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public async Task AddRangeAsync(List<T> listEntity)
    {
        await _context.Set<T>().AddRangeAsync(listEntity);
    }


    public virtual async Task<(IEnumerable<TResult> Data, int TotalCount)> GetByFilterAsync<TResult>(int? pageSize, int? pageNumber,
                                                                                                Expression<Func<T, TResult>> selectQuery,
                                                                                                Expression<Func<T, bool>>? predicate = null,
                                                                                                Expression<Func<TResult, bool>>? predicateSearch = null,
                                                                                                List<(Expression<Func<TResult, object>> KeySelector, bool IsAsc)>? orderBy = null,
                                                                                                CancellationToken token = default,
                                                                                                params Expression<Func<T, object>>[] navigationProperties)
    {
        var query = GetQuery(predicate, navigationProperties);
        var totalCount = await query.CountAsync(token);

        IQueryable<TResult> projectedQuery = query.Select(selectQuery);
        if (predicateSearch != null)
        {
            projectedQuery = projectedQuery.Where(predicateSearch);
        }

        if (orderBy != null && orderBy.Any())
        {
            IOrderedQueryable<TResult> orderedQuery = null!;
            bool isFirst = true;

            foreach (var (keySelector, isAsc) in orderBy)
            {
                if (isFirst)
                {
                    orderedQuery = isAsc ? projectedQuery.OrderBy(keySelector) : projectedQuery.OrderByDescending(keySelector);
                    isFirst = false;
                }
                else
                {
                    orderedQuery = isAsc? orderedQuery.ThenBy(keySelector): orderedQuery.ThenByDescending(keySelector);
                }
            }
            projectedQuery = orderedQuery;
        }


        if (pageSize.HasValue && pageNumber.HasValue)
        {
            projectedQuery = projectedQuery.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
        } 

        return (await projectedQuery.ToListAsync(token), totalCount);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                             CancellationToken token = default,
                                             params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetAllAsync<T>(predicate, null, token, navigationProperties);
    }
    public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>( Expression<Func<T, bool>>? predicate = null,
                                                                             Expression<Func<T, TResult>>? selectQuery = null,
                                                                             CancellationToken token = default,
                                                                             params Expression<Func<T, object>>[] navigationProperties)
    {
        var query = GetQuery(predicate, navigationProperties);

        if (selectQuery != null)
        {
            return await query.Select(selectQuery).ToListAsync(token);
        }

        return await query.Cast<TResult>().ToListAsync(token);
    }


    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).FirstOrDefaultAsync(token);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).AnyAsync(token);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).CountAsync(token);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    protected IQueryable<T> GetQuery(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[] navigationProperties)
    {
        IQueryable<T> dbQuery = _context.Set<T>().AsNoTracking();
        if (predicate != null)
        {
            dbQuery = dbQuery.Where(predicate);
        }
        return navigationProperties.Aggregate(dbQuery, (current, navigationProperty) => current.Include(navigationProperty));
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

    }

    public async Task ExecuteDeleteAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate != null)
        {
            await _context.Set<T>().Where(predicate).ExecuteDeleteAsync();
        }
    }

    public async Task ExecuteUpdateAsync(Expression<Func<T, bool>> predicate,
                                        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression,
                                        CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().Where(predicate).ExecuteUpdateAsync(updateExpression, cancellationToken);
    }

}
