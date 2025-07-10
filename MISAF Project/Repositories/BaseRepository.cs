using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MISAF_Project.Repositories
{
    // BaseRepository is a generic repository that provides shared CRUD operations for any entity class.
    // It implements the IBaseRepository<T> interface for contract enforcement.
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        // Protected fields accessible by derived repository classes
        protected readonly DbContext _context;     // The injected EF DbContext
        protected readonly DbSet<T> _dbSet;        // The DbSet for the given entity type T

        // Constructor accepts a DbContext instance (injected by DI container)
        public BaseRepository(DbContext context)
        {
            _context = context;                // Store the context for use in methods
            _dbSet = context.Set<T>();         // Initialize DbSet for entity type T
        }

        // ==================
        // Shared CRUD Methods
        // ==================

        // Add a new entity to the context (to be saved later)
        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        // Find and remove an entity by its primary key (if it exists)
        public void Delete(object id)
        {
            var entity = _dbSet.Find(id);     // Find the entity by ID
            if (entity != null)
                _dbSet.Remove(entity);        // Remove it from the DbSet
        }

        // Return all records as an IEnumerable<T>
        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();           // Executes the query and materializes the result
        }

        // Get a single entity by its primary key
        public T GetById(object id)
        {
            return _dbSet.Find(id);           // Returns null if not found
        }

        // Save all changes made in the context to the database
        public void SaveChanges()
        {
            _context.SaveChanges();           // Commits all pending changes
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // Update an existing entity by setting its state to Modified
        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified; // Marks the entity as modified for EF to update
        }
    }

}