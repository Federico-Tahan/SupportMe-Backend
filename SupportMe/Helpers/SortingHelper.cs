using SupportMe.DTOs;
using SupportMe.Models.Enums;

namespace SupportMe.Helpers
{
    public static class SortingHelper
    {

        /// <summary>
        /// Applies sorting to an IQueryable based on entity properties and filter parameters. Additionally, supports pagination using Skip and Limit.
        /// </summary>
        /// <typeparam name="T">Type of entity in the IQueryable.</typeparam>
        /// <param name="query">IQueryable to which sorting will be applied.</param>
        /// <param name="filter">Sorting filter containing the column and order, and optional pagination parameters.</param>
        /// <returns>IQueryable with applied sorting and pagination.</returns>
        public static IQueryable<T> ApplySortingAndPagination<T>(IQueryable<T> query, BaseFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.ColumnFilter))
            {
                string methodName = filter.SortBy == SORTBY.DESC ? "OrderByDescending" : "OrderBy";
                var type = typeof(T);
                var property = type.GetProperties().FirstOrDefault(p => p.Name.Equals(filter.ColumnFilter, StringComparison.OrdinalIgnoreCase));

                if (property != null)
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(type, "p");
                    var propertyAccess = System.Linq.Expressions.Expression.MakeMemberAccess(parameter, property);
                    var orderByExpression = System.Linq.Expressions.Expression.Lambda(propertyAccess, parameter);

                    var resultExpression = System.Linq.Expressions.Expression.Call(typeof(Queryable), methodName,
                        new Type[] { type, property.PropertyType },
                        query.Expression, System.Linq.Expressions.Expression.Quote(orderByExpression));

                    query = query.Provider.CreateQuery<T>(resultExpression);
                }
            }

            if (filter.Skip.HasValue)
            {
                query = query.Skip(filter.Skip.Value);
            }

            if (filter.Limit.HasValue)
            {
                query = query.Take(filter.Limit.Value);
            }

            return query;
        }
    }
}
