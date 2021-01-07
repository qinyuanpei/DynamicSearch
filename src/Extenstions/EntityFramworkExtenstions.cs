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
            if (searchParameters.QueryModel == null)
                return source;

            //Where
            if (searchParameters.PageModel == null)
                return source.Where(searchParameters.QueryModel);

            //Pagination
            var results = source
                .Where(searchParameters.QueryModel)
                .Skip((searchParameters.PageModel.CurrentPage - 1) * searchParameters.PageModel.PageSize)
                .Take(searchParameters.PageModel.PageSize);

            //Sort
            searchParameters.PageModel.SortFields = searchParameters.PageModel.SortFields.FindAll(x => !string.IsNullOrEmpty(x.Field));
            if (searchParameters.PageModel.SortFields == null || !searchParameters.PageModel.SortFields.Any())
                return results;

            return results.OrderBy(searchParameters.PageModel).ToList();
        }
        private static IQueryable<T> Where<T>(this IQueryable<T> source, QueryModel queryModel)
        {
            var lambdaExp = LambdaExpressionBuilder.BuildLambda<T>(queryModel);
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

        private static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, PageModel pageModel)
        {
            IOrderedQueryable<T> sorted = null;
            for (var index = 0; index < pageModel.SortFields.Count; index++)
            {
                sorted = source.OrderByOrThenBy<T>(pageModel.SortFields[index], index);
                source = sorted.AsQueryable();
            }

            return sorted;
        }

        private static IOrderedQueryable<T> OrderByOrThenBy<T>(this IQueryable<T> source, SortField sortField, int sortOrder)
        {
            var fieldName = sortField.Field;
            var sortMethod = sortField.SortType == SortType.Desc ? "OrderByDescending" : "OrderBy";
            if (sortOrder != 0)
                sortMethod = sortField.SortType == SortType.Desc ? "ThenByDescending" : "ThenBy";
            return source.ApplyOrder(fieldName, sortMethod);
        }
    }
}