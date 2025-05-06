using Librow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using System.Linq.Expressions;

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
                                                                                                CancellationToken token = default,
                                                                                                params Expression<Func<T, object>>[] navigationProperties)
    {
        var query = GetQuery(predicate, navigationProperties);
        var totalCount = await query.CountAsync(token);

        IQueryable<TResult> projectedQuery = query.Select(selectQuery);

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            projectedQuery = projectedQuery.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        //var queryString = projectedQuery.ToQueryString();
        return (await projectedQuery.ToListAsync(token), totalCount);
    }

    public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
                                             CancellationToken token = default,
                                             params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).ToListAsync(token);
    }
    public virtual async Task<IEnumerable<TResult>> GetAllAsync<TResult>(Expression<Func<T, bool>>? predicate = null,
                                                                             Expression<Func<T, TResult>>? selectQuery = null,
                                                                             CancellationToken token = default,
                                                                             params Expression<Func<T, object>>[] navigationProperties)
    {
        if (selectQuery == null) throw new ArgumentNullException(nameof(selectQuery));
        var query = GetQuery(predicate, navigationProperties);
        return await query.Select(selectQuery).ToListAsync(token);
    }


    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).FirstOrDefaultAsync(token);
    }


    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, TResult>> selectQuery = null!, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        if (selectQuery == null) throw new ArgumentNullException(nameof(selectQuery));
        var query = GetQuery(predicate, navigationProperties);
        return await query.Select(selectQuery).FirstOrDefaultAsync(token);
    }


    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).AnyAsync(token);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).CountAsync(token);
    }

    public virtual async Task<int> SumAsync(Expression<Func<T, int>> selector, Expression<Func<T, bool>>? predicate = null, CancellationToken token = default, params Expression<Func<T, object>>[] navigationProperties)
    {
        return await GetQuery(predicate, navigationProperties).SumAsync(selector);
    }

    public Task SaveChangesAsync_OLD(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added ||
                                                            e.State == EntityState.Modified ||
                                                            e.State == EntityState.Deleted).ToList();


        foreach (var entry in entries)
        {
            try
            {
                var entityName = entry.Entity.GetType().Name;
                if (entityName == nameof(RefreshToken)) continue;
                var primaryKey = entry.Properties.First(p => p.Metadata.IsPrimaryKey()).CurrentValue?.ToString();

                var auditLog = new AuditLog
                {
                    Action = entry.State.ToString(),
                    EntityName = entityName,
                    EntityId = primaryKey,
                    CreatedAt = DateTime.Now,
                };

                var updatedByProperty = entry.Entity.GetType().GetProperty("UpdatedBy");
                if (updatedByProperty != null)
                {
                    var updatedByValue = updatedByProperty.GetValue(entry.Entity);
                    if (updatedByValue is Guid guidValue && guidValue != Guid.Empty)
                    {
                        auditLog.UserId = guidValue;
                    }
                }

                if (entry.State == EntityState.Modified)
                {
                    auditLog.OldValues = JsonConvert.SerializeObject(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]));
                    auditLog.NewValues = JsonConvert.SerializeObject(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));
                }
                else if (entry.State == EntityState.Added)
                {
                    auditLog.NewValues = JsonConvert.SerializeObject(entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditLog.OldValues = JsonConvert.SerializeObject(entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]));
                }
                _context.Set<AuditLog>().Add(auditLog);
            }
            catch (Exception) { }
        }
        await _context.SaveChangesAsync(cancellationToken);
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

    public async Task<TResult?> ExecuteRawSqlSingleAsync<TResult>(string sql, params object[] parameters)
    {
        return await _context.Database.SqlQueryRaw<TResult>(sql, parameters).SingleOrDefaultAsync();
    }

    public async Task<List<TResult>> ExecuteRawSqlAsync<TResult>(string sql, params object[] parameters)
    {
        return await _context.Database.SqlQueryRaw<TResult>(sql, parameters).ToListAsync();
    }
}
