using CSESoftware.Core.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CSESoftware.Repository.SqlBulkRepositoryCore
{
    public interface IRepositoryBulk : IRepository
    {
        /// <summary>
        /// Solution to: SQL Query Processor throws a processing complexity error if contains is used in a query where the
        /// number of values in the list is greater than 3k-5k elements.
        /// 
        /// This function retrieves an unlimited number of values from the database using a list of properties to match to values in the database.
        /// Matching on multiple parameters is possible by adding correct property names to the anonymous objects passed in as selectByValues.
        /// Additional filtering can be performed against the selected objects by overloading with an additional IQuery for the entity to be retrieved.
        /// </summary>
        /// <typeparam name="TEntity">An entity of type BaseEntity to be retrieved from the database</typeparam>
        /// <typeparam name="TObject">An anonymous entity containing properties to match on</typeparam>
        /// <param name="entityToSelect">An instantiated version of the entity to select for type-specific processing</param>
        /// <param name="selectByValues">A list of anonymous objects containing the properties to match on and their values</param>
        /// <param name="filter">An optional IQuery for the entity to be retrieved to further filter or expand results</param>
        /// <returns>A list of the entities being selected</returns>
        Task<List<TEntity>> BulkSelectAsync<TEntity, TObject>(TEntity entityToSelect, IReadOnlyCollection<TObject> selectByValues,
            IQuery<TEntity> filter = null) where TEntity : class, IEntity;

        /// <summary>
        /// Solution to: Entity Framework executes "batch" creation processes as N+1 queries that are not efficient.
        /// 
        /// This function creates an unlimited number of values in the database as single queries of 50,000 objects.
        /// </summary>
        /// <typeparam name="TEntity">An entity of type Entity to be created in the database</typeparam>
        /// <param name="entities">List of entities to create</param>
        /// <returns></returns>
        Task BulkCreateAsync<TEntity>(IReadOnlyCollection<TEntity> entities)
            where TEntity : IEntity;

        /// <summary>
        /// Solution to: Entity Framework executes "batch" creation processes as N+1 queries that are not efficient.
        /// 
        /// This function creates an unlimited number of values in the database as single queries of 50,000 objects and returns
        /// all of those objects after creation has been performed with Id values populated.
        /// 
        /// Note that created values in this collection should all share the same "CreatedDate" that has been converted to the
        /// most recent second in DateTime to comply with SQL database query accuracy.
        /// </summary>
        /// <typeparam name="TEntity">An entity of type BaseEntity to be created and then retrieved from the database</typeparam>
        /// <typeparam name="T">Primary key type for the entity</typeparam>
        /// <param name="entities">List of entities to create</param>
        /// <returns>The list of created entities immediately retrieved from the database</returns>
        Task<List<TEntity>> BulkCreateAndReturnAsync<TEntity, T>(IReadOnlyCollection<TEntity> entities)
            where TEntity : class, IBaseEntity<T>;

        /// <summary>
        /// Solution to: Entity Framework executes "batch" update processes as N+1 queries that are not efficient.
        /// 
        /// This function updates an unlimited number of values in the database as a single query.
        /// </summary>
        /// <typeparam name="TEntity">An entity of type BaseEntity to be updated in the database</typeparam>
        /// <typeparam name="TObject">An anonymous entity containing properties to match on</typeparam>
        /// <param name="entityToUpdate">An instantiated version of the entity to update for type-specific processing</param>
        /// <param name="updateByValues">A list of anonymous objects containing the properties to update and their values</param>
        /// <returns></returns>
        Task BulkUpdateAsync<TEntity, TObject>(TEntity entityToUpdate, IReadOnlyCollection<TObject> updateByValues)
            where TEntity : IEntity;

        /// <summary>
        /// Solution to: Entity Framework executes "batch" delete processes as N+1 queries that are not efficient.
        /// 
        /// This function deletes an unlimited number of values in the database based on provided matching criteria as a single query.
        /// </summary>
        /// <typeparam name="TEntity">An entity of type Entity to be deleted from the database</typeparam>
        /// <typeparam name="TObject">An anonymous entity containing properties to match on</typeparam>
        /// <param name="entityToDelete">An instantiated version of the entity to delete for type-specific processing</param>
        /// <param name="deleteByValues">A list of anonymous objects containing the properties to match on and their values</param>
        /// <returns></returns>
        Task BulkDeleteAsync<TEntity, TObject>(TEntity entityToDelete, IReadOnlyCollection<TObject> deleteByValues)
            where TEntity : IEntity;

        /// <summary>
        /// Solution to: The CSE Software Repository detaches entities during the save async operation to support Entity Framework.
        ///
        /// Since all of these operations avoid Entity Framework in favor of a different process, detaching the entities bloats
        /// the operation time rather than providing any benefit. This function performs the save without the detach.
        /// </summary>
        /// <returns></returns>
        Task SaveWithoutDetachAsync();
    }
}
