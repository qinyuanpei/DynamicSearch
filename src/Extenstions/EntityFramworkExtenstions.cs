using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicSearch.Extenstions
{
    using DynamicSearch.Core;
    public static class EntityFrameworkExstenstions
    {
        public static IEnumerable<T> Search<T>(this IQueryable<T> source, SearchParameters searchParameters)
        {
            if (searchParameters.Query == null) 
                return source;

            if (searchParameters.Page == null) 
                return source.Where(searchParameters.Query);

            var results = source 
                .Where(searchParameters.Query)
                .Skip((searchParameters.Page.CurrentPage - 1) * searchParameters.Page.PageSize)
                .Take(searchParameters.Page.PageSize);

            if (searchParameters.Page.SortFields == null || !searchParameters.Page.SortFields.Any())
                return results;
                
            return results.OrderBy(searchParameters.Page).ToList();
        }
        private static IQueryable<T> Where<T>(this IQueryable<T> source, QueryModel query)
        {
            var lambdaExp = LambdaExpressionBuilder.BuildLambda<T>(query);
            var lambda = lambdaExp.Compile();
            return source.Where(lambda).AsQueryable();
        }

        private static IOrderedQueryable<T> ApplyOrder<T>(this IQueryable<T> source, string property, string sortMethod)
        {
            var type = typeof(T);
            var parameterExp = Expression.Parameter(type, "x");
            var propertyInfo = type.GetProperty(property);
            var propertyExp = Expression.Property(parameterExp, propertyInfo);
            var delegateType = typeof(Func<,>).MakeGenericType(new Type[] { typeof(T), propertyInfo.PropertyType });
            var lambdaExp = Expression.Lambda(delegateType, propertyExp, parameterExp);
            var methodCall = typeof(Queryable)
                .GetMethods()
                .Single(x => x.Name == sortMethod && x.IsGenericMethodDefinition && x.GetGenericArguments().Length == 2 && x.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyInfo.PropertyType);
            return (IOrderedQueryable<T>)methodCall.Invoke(null, new object[] { source, lambdaExp });
        }

        private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, PageModel page)
        {
            var sortMethod = page.SortType == SortType.Desc ? "OrderByDescending" : "OrderBy";
            return source.ApplyOrder(page.SortFields[0], sortMethod);
        }

        private static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> source, PageModel page)
        {
                        var sortMethod = page.SortType == SortType.Desc ? "ThenByDescending" : "ThenBy";
            return source.ApplyOrder(page.SortFields[0], sortMethod);
        }
    }
}