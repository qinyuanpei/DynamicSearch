using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DynamicSearch.Core
{
    public static class LambdaExpressionBuilder
    {
        private static Expression GetExpression(ParameterExpression parameter, Condition condition)
        {
            var propertyParam = Expression.Property(parameter, condition.Field);

            var propertyInfo = propertyParam.Member as PropertyInfo;
            if (propertyInfo == null) 
                throw new MissingMemberException(nameof(Condition), condition.Field);

            //Support Nullable<>
            var realPropertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                propertyParam = Expression.Property(propertyParam, "Value");

            //Support IEnumerable && IEnumerable<T>
            if (condition.Op != Operation.StdIn && condition.Op != Operation.StdNotIn) {
                condition.Value = Convert.ChangeType(condition.Value, realPropertyType);
            } else {
                var typeOfValue = condition.Value.GetType();
                var typeOfList = typeof(List<>).MakeGenericType(realPropertyType);
                if (typeOfValue.IsGenericType && typeOfList.IsAssignableFrom(typeOfValue))
                {
                    condition.Value = typeOfList.GetMethod("ToArray").Invoke(condition.Value, null);
                }
             
            }
                
            var constantParam = Expression.Constant(condition.Value);
            switch (condition.Op)
            {
                case Operation.Equals:
                    return Expression.Equal(propertyParam, constantParam);
                case Operation.NotEquals:
                    return Expression.NotEqual(propertyParam, constantParam);
                case Operation.Contains:
                    return Expression.Call(propertyParam, "Contains", null, constantParam); ;
                case Operation.NotContains:
                    return Expression.Not(Expression.Call(propertyParam, "Contains", null, constantParam));
                case Operation.StartsWith:
                    return Expression.Call(propertyParam, "StartsWith", null, constantParam);
                case Operation.EndsWith:
                    return Expression.Call(propertyParam, "EndsWith", null, constantParam);
                case Operation.GreaterThen:
                    return Expression.GreaterThan(propertyParam, constantParam);
                case Operation.GreaterThenOrEquals:
                    return Expression.GreaterThanOrEqual(propertyParam, constantParam);
                case Operation.LessThan:
                    return Expression.LessThan(propertyParam, constantParam);
                case Operation.LessThanOrEquals:
                    return Expression.LessThanOrEqual(propertyParam, constantParam);
                case Operation.StdIn:
                    return Expression.Call(typeof(Enumerable), "Contains",new Type[] { realPropertyType }, new Expression[] { constantParam, propertyParam });
                case Operation.StdNotIn:
                    return Expression.Not(Expression.Call(typeof(Enumerable), "Contains", new Type[] { realPropertyType }, new Expression[] { constantParam, propertyParam }));
            }

            return null;
        }

        private static Expression GetGroupExpression(ParameterExpression parameter, List<Condition> orConditions)
        {
            if (orConditions.Count == 0)
                return null;

            var exps = orConditions.Select(c => GetExpression(parameter, c)).ToList();
            return exps.Aggregate<Expression, Expression>(null, (left, right) =>
            {
                if (left == null)
                    return right;
                return Expression.OrElse(left, right);
            });
        }

        public static Expression<Func<T, bool>> BuildLambda<T>(IEnumerable<Condition> conditions)
        {
            if (conditions == null || !conditions.Any()) return x => true;
            var parameter = Expression.Parameter(typeof(T), "x");

            //简单条件
            var simpleExps = conditions
                .ToList()
                .FindAll(c => string.IsNullOrEmpty(c.OrGroup))
                .Select(c => GetExpression(parameter, c))
                .ToList();

            //复杂条件
            var complexExps = conditions
                .ToList()
                .FindAll(c => !string.IsNullOrEmpty(c.OrGroup))
                .GroupBy(x => x.OrGroup)
                .Select(g => GetGroupExpression(parameter, g.ToList()))
                .ToList();

            var exp = simpleExps.Concat(complexExps).Aggregate<Expression, Expression>(null, (left, right) =>
            {
                if (left == null)
                    return right;
                return Expression.AndAlso(left, right);
            }); ;
            return Expression.Lambda<Func<T, bool>>(exp, parameter);
        }

        public static Expression<Func<T, bool>> BuildAndAlsoLambda<T>(IEnumerable<Condition> conditions)
        {
            if (conditions == null || !conditions.Any()) return x => true;
            var parameter = Expression.Parameter(typeof(T), "x");
            var simpleExps = conditions
                .ToList()
                .Select(c=>GetExpression(parameter, c))
                .ToList();

            var exp = simpleExps.Aggregate<Expression, Expression>(null, (left, right) => {
                if (left == null)
                    return right;
                return Expression.AndAlso(left, right);
            });
            return Expression.Lambda<Func<T, bool>>(exp, parameter);

        }

        public static Expression<Func<T,bool>> BuildOrElseLambda<T>(IEnumerable<Condition> conditions)
        {
            if (conditions == null || !conditions.Any()) return x => true;
            var parameter = Expression.Parameter(typeof(T), "x");
            var simpleExps = conditions
                .ToList()
                .Select(c=>GetExpression(parameter, c))
                .ToList();

            var exp = simpleExps.Aggregate<Expression, Expression>(null, (left, right) => {
                if (left == null)
                    return right;
                return Expression.OrElse(left, right);
            });
            return Expression.Lambda<Func<T, bool>>(exp, parameter);
        }
    }

}
