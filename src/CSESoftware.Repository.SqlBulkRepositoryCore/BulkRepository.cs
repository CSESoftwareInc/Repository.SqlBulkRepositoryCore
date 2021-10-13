using CSESoftware.Core.Entity;
using CSESoftware.Repository.EntityFrameworkCore;
using CSESoftware.Repository.SqlBulkRepositoryCore.RepositoryHelpers;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CSESoftware.Repository.SqlBulkRepositoryCore
{
    public class BulkRepository<TContext> : Repository<TContext>, IRepositoryBulk where TContext : DbContext
    {
        private const int RetryCount = 3;
        private const int DeadlockExceptionNumber = 1205;
        private const string ConstSelectTableName = "#TemporarySelect";
        private const string ConstUpdateTableName = "#TemporaryUpdate";
        private const string ConstDeleteTableName = "#TemporaryDelete";
        private readonly int _batchSize;

        public BulkRepository(TContext context, int batchSize = 50000) : base(context)
        {
            _batchSize = batchSize;
        }

        public async Task<List<TEntity>> BulkSelectAsync<TEntity, TObject>(TEntity entityToSelect,
            IReadOnlyCollection<TObject> selectByValues, IQuery<TEntity> filter = null)
            where TEntity : class, IEntity
        {
            if (!selectByValues.Any()) return new List<TEntity>();

            const OperationType type = OperationType.Select;
            var batchedEntities = RepositoryHelper.SplitLargeVolumesOfEntities(selectByValues.ToList(), _batchSize);
            var conn = (SqlConnection)Context.Database.GetDbConnection();
            var mustOpen = conn.State != ConnectionState.Open;

            await using var command = new SqlCommand("", conn);
            try
            {
                if (mustOpen) conn.Open();
                ExecuteCreateTempTableQuery(command, selectByValues.ElementAt(0), type);

                var entities = new List<TEntity>();
                foreach (var batch in batchedEntities)
                    entities.AddRange(await BulkSelectBaseAsync(entityToSelect, batch, conn, command, filter));
                return entities;
            }
            catch (Exception ex)
            {
                throw new BulkRepositoryException("Bulk Select operation failed due to an exception.", ex);
            }
            finally
            {
                ExecuteDropTempTableQuery(command, type);
                if (mustOpen) conn.Close();
            }
        }

        public async Task BulkCreateAsync<TEntity>(IReadOnlyCollection<TEntity> entities)
            where TEntity : IEntity
        {
            if (!entities.Any()) return;

            var batchedEntities = RepositoryHelper.SplitLargeVolumesOfEntities(entities.ToList(), _batchSize);
            var conn = (SqlConnection)Context.Database.GetDbConnection();

            try
            {
                foreach (var batch in batchedEntities)
                    await BulkWriteBaseAsync(batch, conn);
            }
            catch (Exception ex)
            {
                throw new BulkRepositoryException("Bulk Create operation failed due to an exception.", ex);
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close();
            }
        }

        public async Task<List<TEntity>> BulkCreateAndReturnAsync<TEntity, T>(IReadOnlyCollection<TEntity> entities)
            where TEntity : class, IBaseEntity<T>
        {
            var rawDate = entities.FirstOrDefault()?.CreatedDate ?? DateTime.UtcNow;
            var createdDate = RepositoryHelper.RoundDateTimeToFloor(rawDate);

            foreach (var entity in entities)
                entity.CreatedDate = createdDate;

            await BulkCreateAsync(entities);

            var selectQuery = QueryableManager.GetCreateQuery<TEntity, T>(createdDate);
            return await GetAllAsync(selectQuery);
        }

        public async Task BulkUpdateAsync<TEntity, TObject>(TEntity entityToUpdate,
            IReadOnlyCollection<TObject> updateByValues)
            where TEntity : IEntity
        {
            if (!updateByValues.Any()) return;

            const OperationType type = OperationType.Update;
            var batchedEntities = RepositoryHelper.SplitLargeVolumesOfEntities(updateByValues.ToList(), _batchSize);
            var conn = (SqlConnection)Context.Database.GetDbConnection();
            var mustOpen = conn.State != ConnectionState.Open;

            await using var command = new SqlCommand("", conn);
            try
            {
                if (mustOpen) conn.Open();
                ExecuteCreateTempTableQuery(command, updateByValues.ElementAt(0), type);

                foreach (var batch in batchedEntities)
                    await BulkUpdateBaseAsync(entityToUpdate, batch, conn, command);
            }
            catch (Exception ex)
            {
                throw new BulkRepositoryException("Bulk Update operation failed due to an exception.", ex);
            }
            finally
            {
                ExecuteDropTempTableQuery(command, type);
                if (mustOpen) conn.Close();
            }
        }

        public async Task BulkDeleteAsync<TEntity, TObject>(TEntity entityToDelete,
            IReadOnlyCollection<TObject> deleteByValues)
            where TEntity : IEntity
        {
            if (!deleteByValues.Any()) return;

            const OperationType type = OperationType.Delete;
            var batchedEntities = RepositoryHelper.SplitLargeVolumesOfEntities(deleteByValues.ToList(), _batchSize);
            var conn = (SqlConnection)Context.Database.GetDbConnection();
            var mustOpen = conn.State != ConnectionState.Open;

            await using var command = new SqlCommand("", conn);
            try
            {
                if (mustOpen) conn.Open();
                ExecuteCreateTempTableQuery(command, deleteByValues.ElementAt(0), type);

                foreach (var batch in batchedEntities)
                    await BulkDeleteBaseAsync(entityToDelete, batch, conn, command);
            }
            catch (Exception ex)
            {
                throw new BulkRepositoryException("Bulk Delete operation failed due to an exception.", ex);
            }
            finally
            {
                ExecuteDropTempTableQuery(command, type);
                if (mustOpen) conn.Close();
            }
        }

        public async Task SaveWithoutDetachAsync()
        {
            await Context.SaveChangesAsync();
        }

        private async Task<List<TEntity>> BulkSelectBaseAsync<TEntity, TObject>(TEntity entityToSelect,
            IReadOnlyCollection<TObject> selectByValues, SqlConnection conn, IDbCommand command,
            IQuery<TEntity> filter)
            where TEntity : class, IEntity
        {
            const OperationType type = OperationType.Select;
            var tryCount = RetryCount;
            var success = false;
            var entities = new List<TEntity>();

            while (tryCount > 0 && !success)
            {
                try
                {
                    await BulkWriteToTempTableAsync(selectByValues, conn, type);

                    var parameters =
                        RepositoryHelper.AssembleParameters(Context, entityToSelect, selectByValues.ElementAt(0), type);
                    var query = GetQueryable<TEntity>(parameters);
                    entities = QueryableManager.GetQueryableForSelect(filter, query).ToList();

                    ExecuteDeleteFromTempTableQuery(command, type);
                    success = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number != DeadlockExceptionNumber) throw;
                    ExecuteDeleteFromTempTableQuery(command, type);
                    tryCount--;
                    if (tryCount == 0) throw;
                }
            }

            return entities;
        }

        private async Task BulkWriteBaseAsync<TEntity>(IEnumerable<TEntity> entities, SqlConnection conn)
            where TEntity : IEntity
        {
            var mustOpen = conn.State != ConnectionState.Open;
            var columnNames = Context.RetrieveEntityColumnNames<TEntity>();
            var tableName = Context.GetTableName<TEntity>();

            if (mustOpen) conn.Open();
            await using var reader = ObjectReader.Create(entities, columnNames.Values.ToArray());
            using var bcp = new SqlBulkCopy(conn)
            {
                DestinationTableName = tableName
            };

            foreach (var (key, value) in columnNames)
                bcp.ColumnMappings.Add(value, key);

            await bcp.WriteToServerAsync(reader);
        }

        private async Task BulkUpdateBaseAsync<TEntity, TObject>(TEntity entityToUpdate,
            IReadOnlyCollection<TObject> updateByValues, SqlConnection conn, IDbCommand command)
            where TEntity : IEntity
        {
            const OperationType type = OperationType.Update;
            var tryCount = RetryCount;
            var success = false;

            while (tryCount > 0 && !success)
            {
                try
                {
                    await BulkWriteToTempTableAsync(updateByValues, conn, type);

                    var parameters =
                        RepositoryHelper.AssembleParameters(Context, entityToUpdate, updateByValues.ElementAt(0), type);
                    ExecuteUpdateQuery<TEntity>(command, parameters);
                    success = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number != DeadlockExceptionNumber) throw;
                    ExecuteDeleteFromTempTableQuery(command, type);
                    tryCount--;
                    if (tryCount == 0) throw;
                }
            }
        }

        private async Task BulkDeleteBaseAsync<TEntity, TObject>(TEntity entityToDelete,
            IReadOnlyCollection<TObject> deleteByValues, SqlConnection conn, IDbCommand command)
            where TEntity : IEntity
        {
            const OperationType type = OperationType.Delete;
            var tryCount = RetryCount;
            var success = false;

            while (tryCount > 0 && !success)
            {
                try
                {
                    await BulkWriteToTempTableAsync(deleteByValues, conn, type);

                    var parameters =
                        RepositoryHelper.AssembleParameters(Context, entityToDelete, deleteByValues.ElementAt(0), type);
                    ExecuteDeleteQuery<TEntity>(command, parameters);
                    success = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number != DeadlockExceptionNumber) throw;
                    ExecuteDeleteFromTempTableQuery(command, type);
                    tryCount--;
                    if (tryCount == 0) throw;
                }
            }
        }

        private static void ExecuteCreateTempTableQuery<TObject>(IDbCommand command, TObject operationObject,
            OperationType type)
        {
            var parameters = RepositoryHelper.AssembleTempTableColumns(operationObject);
            var tableName = GetTempTableName(type);

            command.CommandText = $"CREATE TABLE {tableName}({parameters})";
            command.ExecuteNonQuery();
        }

        private static void ExecuteDeleteFromTempTableQuery(IDbCommand command, OperationType type)
        {
            var tableName = GetTempTableName(type);

            command.CommandText = $"DELETE FROM {tableName}";
            command.ExecuteNonQuery();
        }

        private static void ExecuteDropTempTableQuery(IDbCommand command, OperationType type)
        {
            var tableName = GetTempTableName(type);

            command.CommandText = $"DROP TABLE IF EXISTS {tableName}";
            command.ExecuteNonQuery();
        }

        private IQueryable<TEntity> GetQueryable<TEntity>(string parameters)
            where TEntity : class, IEntity
        {
            var destinationTableName = Context.GetTableName<TEntity>();
            var queryString =
                $"SELECT O.* FROM {destinationTableName} O INNER JOIN {ConstSelectTableName} T ON {parameters}";

            return Context.Set<TEntity>().FromSqlRaw(queryString);
        }

        private void ExecuteUpdateQuery<TEntity>(IDbCommand command, string parameters)
            where TEntity : IEntity
        {
            var destinationTableName = Context.GetTableName<TEntity>();

            command.CommandText =
                $"UPDATE O SET {parameters} FROM {destinationTableName} O INNER JOIN {ConstUpdateTableName} T ON O.Id = T.Id";
            command.ExecuteNonQuery();
        }

        private void ExecuteDeleteQuery<TEntity>(IDbCommand command, string parameters)
            where TEntity : IEntity
        {
            var destinationTableName = Context.GetTableName<TEntity>();

            command.CommandText =
                $"DELETE O FROM {destinationTableName} O INNER JOIN {ConstDeleteTableName} T ON {parameters}";
            command.ExecuteNonQuery();
        }

        private static async Task BulkWriteToTempTableAsync<TObject>(IReadOnlyCollection<TObject> operationObjects,
            SqlConnection connection, OperationType type)
        {
            var objectType = operationObjects.ElementAt(0).GetType();
            var propertiesToMap = objectType.GetProperties().Select(x => x.Name).ToArray();
            var tempTableName = GetTempTableName(type);

            using var bcp = new SqlBulkCopy(connection);
            await using var reader = new ObjectReader(objectType, operationObjects, propertiesToMap);
            bcp.DestinationTableName = tempTableName;
            await bcp.WriteToServerAsync(reader);
            bcp.Close();
        }

        private static string GetTempTableName(OperationType type)
        {
            return type switch
            {
                OperationType.Select => ConstSelectTableName,
                OperationType.Update => ConstUpdateTableName,
                OperationType.Delete => ConstDeleteTableName,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}
