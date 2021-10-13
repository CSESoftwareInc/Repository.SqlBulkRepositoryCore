using CSESoftware.Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.RepositoryHelpers
{
    internal static class RepositoryHelper
    {
        /// <summary>
        /// This function assembles the column matching strings for select, update, and delete to join temp tables to 
        /// operational data tables in the database.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <typeparam name="TObject">An anonymous object with properties that match column names for the database object</typeparam>
        /// <param name="context">Database context necessary for column mapping</param>
        /// <param name="entity">A sample of the entity to be used for processing</param>
        /// <param name="operationObject">A sample of the anonymous object to be matched to the entity</param>
        /// <param name="type">The type of operation to be performed</param>
        /// <returns>A string for use in parameterized queries</returns>
        internal static string AssembleParameters<TEntity, TObject>(DbContext context, TEntity entity,
            TObject operationObject, OperationType type)
            where TEntity : IEntity
        {
            var entityProperties = GetPropertyNames(entity);
            var operationEntityProperties = GetPropertyNames(operationObject);

            return type == OperationType.Update
                ? AssembleUpdateColumnToColumnMatch<TEntity>(context, entityProperties, operationEntityProperties)
                : AssembleSelectColumnToColumnMatch(entityProperties, operationEntityProperties);
        }

        /// <summary>
        /// This function splits lists of entities that are greater than the limiter into batches equal in size to the limiter.
        /// Performance begins to degrade on batch create functions where the quantity of entities to create is greater than 50k.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <param name="entities">Entities to be split into batches</param>
        /// <param name="batchSize">The size of each batch</param>
        /// <returns>Batched entities to be processed in a loop</returns>
        internal static List<List<TEntity>> SplitLargeVolumesOfEntities<TEntity>(List<TEntity> entities, int batchSize)
        {
            var batches = new List<List<TEntity>>();

            if (entities.Count < batchSize)
                batches.Add(entities);
            else
            {
                while (entities.Any())
                {
                    batches.Add(entities.Take(batchSize).ToList());
                    entities = entities.Skip(batchSize).ToList();
                }
            }

            return batches;
        }

        /// <summary>
        /// This function assembles the column string of properties for creating the temp table.
        /// </summary>
        /// <typeparam name="TObject">An anonymous object with properties that match column names for a database object</typeparam>
        /// <param name="typeToEvaluate">A sample of the anonymous object to be be inserted into the temp table</param>
        /// <returns>A string for use in parameterized queries</returns>
        internal static string AssembleTempTableColumns<TObject>(TObject typeToEvaluate)
        {
            var type = typeToEvaluate.GetType();
            var properties = type.GetProperties().ToList();
            var propertyList = properties.Select(AddPropertiesToTableColumns).ToList();

            return string.Join(",", propertyList);
        }

        /// <summary>
        /// This function provides the table name for an entity in the database even if that name differs from the object name.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <param name="context">The database context to query</param>
        /// <returns>A string for the table name to be used in parameterized queries</returns>
        internal static string GetTableName<TEntity>(this DbContext context)
            where TEntity : IEntity
        {
            var entityType = context.Model.FindEntityType(typeof(TEntity));
            return entityType.GetTableName();
        }

        /// <summary>
        /// This function provides a dictionary of the column names for an entity in the database where the Key is the database
        /// column name and the value is the property name for the entity object in code.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <param name="context">The database context to query</param>
        /// <returns>A dictionary with Key representing database column name and Value representing property name</returns>
        internal static Dictionary<string, string> RetrieveEntityColumnNames<TEntity>(this DbContext context)
            where TEntity : IEntity
        {
            var tableName = GetTableName<TEntity>(context);
            var columns = context.Model.GetEntityTypes(typeof(TEntity))
                .SelectMany(t => t.GetProperties())
                .ToDictionary(x => x.GetColumnName(StoreObjectIdentifier.Table(tableName, null)), x => x.Name);

            return columns;
        }

        /// <summary>
        /// This function takes in a date and rounds it down to the preceding second. This is to counter DateTime comparison difficulties
        /// between CLR DateTime objects and SQL DateTime column data.
        /// </summary>
        /// <param name="date">Date to parse</param>
        /// <returns>Date passed in rounded down to the preceding second</returns>
        internal static DateTime RoundDateTimeToFloor(DateTime date)
        {
            var span = new TimeSpan(0, 0, 1);
            var ticks = date.Ticks / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }

        private static List<string> GetPropertyNames<T>(T sample)
        {
            return sample.GetType().GetProperties().Select(x => x.Name).ToList();
        }

        private static string AssembleUpdateColumnToColumnMatch<TEntity>(DbContext context,
            ICollection<string> entityProperties, IEnumerable<string> updateProperties)
            where TEntity : IEntity
        {
            var primaryKeys = context.Model.GetEntityTypes(typeof(TEntity))
                .Select(x => x.FindPrimaryKey());
            var primaryKeyColumns = primaryKeys.SelectMany(x => x.Properties)
                .Select(x => x.Name).ToList();

            var returnList = updateProperties.Where(x =>
                    entityProperties.Contains(x) && !primaryKeyColumns.Contains(x))
                .Select(prop => ConstructMatchString(prop, entityProperties)).ToList();

            return string.Join(",", returnList);
        }

        private static string AssembleSelectColumnToColumnMatch(ICollection<string> entityProperties,
            IEnumerable<string> selectProperties)
        {
            var returnList = selectProperties.Where(entityProperties.Contains)
                .Select(prop => ConstructMatchString(prop, entityProperties)).ToList();

            return returnList.Count > 1 ? string.Join(" AND ", returnList) : returnList[0];
        }

        private static string ConstructMatchString(string property, IEnumerable<string> possibleMatches)
        {
            var match = possibleMatches.First(x => x == property);
            return $"O.{match} = T.{property}";
        }

        private static string AddPropertiesToTableColumns(PropertyInfo property)
        {
            var finalPropertyType = PropertyIsNullable(property)
                ? ParseSqlType(property.PropertyType.GetGenericArguments()[0].Name, true)
                : ParseSqlType(property.PropertyType.Name);
            var propertyName = property.Name;

            return $"{propertyName} {finalPropertyType}";
        }

        private static bool PropertyIsNullable(PropertyInfo property)
        {
            return property.PropertyType.IsGenericType &&
                   property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static string ParseSqlType(string clrType, bool nullable = false)
        {
            var checkType = clrType.Trim().ToUpper();
            return checkType switch
            {
                "INT32" => "int " + (nullable ? "NULL" : ""),
                "BYTE" => "tinyint " + (nullable ? "NULL" : ""),
                "INT16" => "smallint " + (nullable ? "NULL" : ""),
                "INT64" => "bigint " + (nullable ? "NULL" : ""),
                "SINGLE" => "float " + (nullable ? "NULL" : ""),
                "DOUBLE" => "float " + (nullable ? "NULL" : ""),
                "STRING" => "nvarchar(max) NULL",
                "CHAR" => "nchar" + (nullable ? "NULL" : ""),
                "BOOLEAN" => "bit " + (nullable ? "NULL" : ""),
                "GUID" => "uniqueidentifier " + (nullable ? "NULL" : ""),
                "DECIMAL" => "decimal(20,10) " + (nullable ? "NULL" : ""),
                "DATETIME" => "datetime2 " + (nullable ? "NULL" : ""),
                "DATETIMEOFFSET" => "datetimeoffset " + (nullable ? "NULL" : ""),
                "ENUM" => "int",
                _ => clrType,
            };
        }
    }
}
