using Microsoft.IdentityModel.Tokens;
using SupportMe.DTOs;
using SupportMe.Models.Enums;
using System.Linq.Expressions;
using System.Reflection;

namespace SupportMe.Helpers
{
    public static class SortingHelper
    {
        public static IQueryable<T> ApplyMultipleSortingAndPagination<T>(IQueryable<T> query, BaseFilter filter, bool applyPagination = false)
        {
            IOrderedQueryable<T> orderedQuery = null;

            if (!filter.Sorting.IsNullOrEmpty())
            {
                foreach (var sort in filter.Sorting)
                {
                    string methodName;

                    if (orderedQuery == null)
                    {
                        methodName = sort.SortBy == SORTBY.DESC ? "OrderByDescending" : "OrderBy";
                    }
                    else
                    {
                        methodName = sort.SortBy == SORTBY.DESC ? "ThenByDescending" : "ThenBy";
                    }

                    var type = typeof(T);
                    var property = FindPropertyV2(type, sort.Field, out List<PropertyInfo> parentProperty);

                    if (property == null)
                    {
                        throw new InvalidOperationException($"Property '{sort.Field}' not found in type '{type.Name}'.");
                    }

                    var parameter = Expression.Parameter(type, "p");
                    var propertyAccess = parentProperty == null
                        ? Expression.MakeMemberAccess(parameter, property)
                        : CreatePropertyAccess(parameter, parentProperty, property);

                    var orderByExpression = Expression.Lambda(propertyAccess, parameter);

                    var resultExpression = Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new Type[] { type, property.PropertyType },
                        (orderedQuery == null ? query.Expression : orderedQuery.Expression),
                        Expression.Quote(orderByExpression)
                    );

                    orderedQuery = query.Provider.CreateQuery<T>(resultExpression) as IOrderedQueryable<T>;
                }
            }

            var response = orderedQuery ?? query;

            if (!applyPagination)
            {
                return response;
            }

            if (filter.Skip.HasValue)
            {
                response = response.Skip(filter.Skip.Value);
            }

            if (filter.Limit.HasValue)
            {
                response = response.Take(filter.Limit.Value);
            }

            return response;
        }

        private static Expression CreatePropertyAccess(Expression parameter, List<PropertyInfo> parentProperty, PropertyInfo property)
        {
            Expression parentPropertyAccess = parameter;
            parentProperty.Reverse();
            foreach (var item in parentProperty)
            {

                parentPropertyAccess = Expression.MakeMemberAccess(parentPropertyAccess, item);

            }
            //var parentPropertyAccess = Expression.MakeMemberAccess(parameter, parentProperty);
            var propertyAccess = Expression.MakeMemberAccess(parentPropertyAccess, property);

            return propertyAccess;
        }

        private static PropertyInfo FindPropertyV2(Type type, string propertyName, out List<PropertyInfo> parentProperty)
        {
            var properties = type.GetProperties();

            if (properties.IsNullOrEmpty())
            {
                parentProperty = null;
                return null;
            }

            if (propertyName.IsNullOrEmpty())
            {
                parentProperty = null;
                return null;
            }
            var propertiesPath = propertyName.SplitAtFirstDot();

            var prop = properties.Where(x => x.Name.Equals(propertiesPath[0], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (prop is null)
            {
                parentProperty = null;
                return null;
            }

            if (propertiesPath.Count() > 1)
            {

                var result = FindPropertyV2(prop.PropertyType, propertiesPath[1], out parentProperty);
                if (parentProperty is null)
                {
                    parentProperty = new List<PropertyInfo>();
                }
                parentProperty.Add(prop);

                return result;
            }
            else
            {
                parentProperty = new List<PropertyInfo>();
                return prop;
            }

            parentProperty = null;
            return null;
        }
        static string[] SplitAtFirstDot(this string input)
        {
            int dotIndex = input.IndexOf('.');
            if (dotIndex == -1)
            {
                return new[] { input };
            }
            return new[]
            {
            input.Substring(0, dotIndex),
            input.Substring(dotIndex + 1)
            };
        }
    }
}
