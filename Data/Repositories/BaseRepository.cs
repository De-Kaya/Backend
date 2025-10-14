using AutoMapper;
using Data.Context;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace Data.Repositories;

public abstract class BaseRepository<TEntity, TModel> : IBaseRepository<TEntity, TModel> where TEntity : class where TModel : class
{
    protected readonly DataContext _context;
    protected readonly DbSet<TEntity> _table;
    protected readonly IMapper _mapper;
    private IDbContextTransaction? _transaction;

    protected BaseRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _table = _context.Set<TEntity>();
        _mapper = mapper;
    }

    #region Transaction Methods

    public virtual async Task BeginTransactionAsync()
    {
        if (_transaction != null)
            throw new InvalidOperationException("A transaction is already in progress.");
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public virtual async Task CommitTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");
        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public virtual async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction in progress.");
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    #endregion Transaction Methods

    public virtual async Task<ApiResponse<bool>> AddAsync(TEntity entity)
    {
        try
        {
            if (entity == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "Entity cannot be null", Result = false };

            _table.Add(entity);
            await _context.SaveChangesAsync();
            return new ApiResponse<bool> { Succeeded = true, StatusCode = 201, Message = "Entity added successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    public virtual async Task<ApiResponse<IEnumerable<TModel>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _table.AsNoTracking();

            if (where != null)
                query = query.Where(where);

            if (includes != null && includes.Length > 0)
                foreach (var include in includes)
                    query = query.Include(include);

            if (sortBy != null)
                query = orderByDescending
                    ? query.OrderByDescending(sortBy)
                    : query.OrderBy(sortBy);

            var entities = await query.ToListAsync();
            var result = _mapper.Map<IEnumerable<TModel>>(entities);

            return new ApiResponse<IEnumerable<TModel>> { Succeeded = true, StatusCode = 200, Message = "Entities retrieved successfully", Result = result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<TModel>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public virtual async Task<ApiResponse<IEnumerable<TSelect>>> GetAllAsync<TSelect>(Expression<Func<TEntity, TSelect>> selector, bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            IQueryable<TEntity> query = _table.AsNoTracking();
            if (where != null)
                query = query.Where(where);

            if (includes != null && includes.Length > 0)
                foreach (var include in includes)
                    query = query.Include(include);

            if (sortBy != null)
                query = orderByDescending
                    ? query.OrderByDescending(sortBy)
                    : query.OrderBy(sortBy);

            var entities = await query.Select(selector).ToListAsync();
            return new ApiResponse<IEnumerable<TSelect>> { Succeeded = true, StatusCode = 200, Message = "Entities retrieved successfully", Result = entities };
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<TSelect>> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public virtual async Task<ApiResponse<TModel>> GetAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            if (where == null)
                return new ApiResponse<TModel> { Succeeded = false, StatusCode = 400, Message = "Where clause cannot be null", Result = null };

            IQueryable<TEntity> query = _table.AsNoTracking();
            if (includes != null && includes.Length > 0)
                foreach (var include in includes)
                    query = query.Include(include);

            var entity = await query.FirstOrDefaultAsync(where);
            if (entity == null)
                return new ApiResponse<TModel> { Succeeded = false, StatusCode = 404, Message = "No entities found", Result = null };

            var result = _mapper.Map<TModel>(entity);
            return new ApiResponse<TModel> { Succeeded = true, StatusCode = 200, Message = "Entity retrieved successfully", Result = result };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TModel> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = null };
        }
    }

    public virtual async Task<ApiResponse<bool>> ExistsAsync(Expression<Func<TEntity, bool>> findBy)
    {
        try
        {
            if (findBy == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "FindBy clause cannot be null", Result = false };

            var exists = await _table.AnyAsync(findBy);
            return new ApiResponse<bool> { Succeeded = true, StatusCode = exists ? 200 : 404, Message = exists ? "Entity exists" : "Entity not found", Result = exists };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    public virtual async Task<ApiResponse<bool>> UpdateAsync(TEntity entity)
    {
        try
        {
            if (entity == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "Entity cannot be null", Result = false };

            _table.Update(entity);
            await _context.SaveChangesAsync();
            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Entity updated successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    public virtual async Task<ApiResponse<bool>> DeleteAsync(TEntity entity)
    {
        try
        {
           if (entity == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 400, Message = "Entity cannot be null", Result = false };

            _table.Remove(entity);
            await _context.SaveChangesAsync();
            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Entity deleted successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    public virtual async Task<ApiResponse<bool>> DeleteByIdAsync(string id)
    {
        try
        {
            var entity = await _table.FindAsync(id);
            if (entity == null)
                return new ApiResponse<bool> { Succeeded = false, StatusCode = 404, Message = "Entity not found", Result = false };
            _table.Remove(entity);
            await _context.SaveChangesAsync();
            return new ApiResponse<bool> { Succeeded = true, StatusCode = 200, Message = "Entity deleted successfully", Result = true };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Succeeded = false, StatusCode = 500, Message = ex.Message, Result = false };
        }
    }

    //Generic bir çözüm. Test için ekledim.
    public virtual async Task<TEntity?> FindByIdAsync(string id)
    {
        return await _table.FindAsync(id);
    }
}
