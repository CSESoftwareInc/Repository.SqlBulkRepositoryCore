using CSESoftware.Core.Entity;
using CSESoftware.Repository.Builder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace CSESoftware.Repository.SqlBulkRepositoryCore.RepositoryHelpers
{
    internal static class QueryableManager
    {
        /// <summary>
        /// This function returns a combined queryable to assist with Select processing by further filtering results.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <param name="filter">Secondary query to filter against</param>
        /// <param name="query">Original queryable constructed by the select process</param>
        /// <returns>A combined queryable to execute against the database</returns>
        internal static IQueryable<TEntity> GetQueryableForSelect<TEntity>(IQuery<TEntity> filter, IQueryable<TEntity> query)
            where TEntity : class, IEntity
        {
            if (filter == null) return query;

            filter.Include ??= new List<Expression<Func<TEntity, object>>>();

            if (filter.Predicate != null)
                query = query.Where(filter.Predicate);

            query = filter.Include.Aggregate(query, (current, property) => current.Include(ToPropertyString(property)));

            if (filter.OrderBy != null)
                query = filter.OrderBy(query);

            if (filter.Skip.HasValue)
                query = query.Skip(filter.Skip.Value);

            if (filter.Take.HasValue)
                query = query.Take(filter.Take.Value);

            return query;
        }

        /// <summary>
        /// This function returns the select query for bulk create.
        /// </summary>
        /// <typeparam name="TEntity">An entity that exists in the database</typeparam>
        /// <typeparam name="T">Primary key type for the entity</typeparam>
        /// <param name="createdDate">A datetime object matching the created date for the entities created. Note this DateTime must not be more precise than to the second or the SQL query generated will not return the objects.</param>
        /// <returns>An IQuery to use against the repository and return the created objects</returns>
        internal static IQuery<TEntity> GetCreateQuery<TEntity, T>(DateTime createdDate)
            where TEntity : class, IBaseEntity<T>
        {
            return new QueryBuilder<TEntity>()
                .Where(x => x.CreatedDate == createdDate)
                .Select(x => x.Id)
                .Build();
        }

        private static string ToPropertyString<TEntity>(Expression<Func<TEntity, object>> property)
        {
            var modifiedProperty = property.ToString();

            modifiedProperty = Regex.Replace(modifiedProperty, @"^.*?\.", "");
            modifiedProperty = Regex.Replace(modifiedProperty, @"Select\(.*?\.", "");
            modifiedProperty = modifiedProperty.Replace(")", "");

            return modifiedProperty;
        }
    }
}
