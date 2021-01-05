using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Data.Common;
using System.Linq.Expressions;
using System.Collections.Generic;
using Dapper;

namespace DynamicSearch.Extenstions
{
    using DynamicSearch.Core;
    public static class DapperExstenstions
    {
        public static IEnumerable<TEntity> Search<TEntity>(this IDbConnection connection, SearchParameters searchParameters)
        {
            var builder = new StringBuilder($"SELECT * FROM {typeof(TEntity).Name}");

            //构建Where语句
            var sqlWhere = searchParameters.BuildSqlWhere();
            if (!string.IsNullOrEmpty(sqlWhere.Item1))
                builder.Append(sqlWhere.Item1);

            //构建Order By语句
            var sqlOrderBy = searchParameters.BuildSqlOrderBy();
            if (!string.IsNullOrEmpty(sqlOrderBy))
                builder.Append(sqlOrderBy);

            //构建Limit语句
            var sqlLimit = searchParameters.BuildSqlLimit();
            if (!string.IsNullOrEmpty(sqlLimit))
                builder.Append(sqlLimit);

            //查询
            var sql = builder.ToString();
            if (sqlWhere.Item2 != null && sqlWhere.Item2.Any())
                return connection.Query<TEntity>(sql, sqlWhere.Item2);
            return connection.Query<TEntity>(sql);
        }
        private static (string, Dictionary<string, object>) BuildSqlWhere(this SearchParameters searchParameters)
        {
            var conditions = searchParameters.Query;
            if (conditions == null || !conditions.Any())
                return (string.Empty, null);

            var sqlExps = new List<string>();
            var sqlParam = new Dictionary<string, object>();

            //构建简单条件
            var simpleConditions = conditions.FindAll(x => string.IsNullOrEmpty(x.OrGroup));
            sqlExps.Add(simpleConditions.BuildSqlWhere(ref sqlParam));

            //构建复杂条件
            var complexConditions = conditions.FindAll(x => !string.IsNullOrEmpty(x.OrGroup));
            sqlExps.AddRange(complexConditions.GroupBy(x => x.OrGroup).ToList().Select(x => "( " + x.BuildSqlWhere(ref sqlParam, " OR ") + " )"));

            var sqlWhwere = sqlExps.Count > 1 ? string.Join(" AND ", sqlExps) : sqlExps[0];
            return ($" WHERE {sqlWhwere} ", sqlParam);
        }

        private static string BuildSqlOrderBy(this SearchParameters searchParameters)
        {
            var pageInfo = searchParameters.Page;
            if (pageInfo.SortFields != null && pageInfo.SortFields.Any())
            {
                var orderBy = string.Join(",", pageInfo.SortFields);
                var orderType = pageInfo.SortType == SortType.Asc ? "ASC" : "DESC";
                return $" ORDER BY {orderBy} {orderType}";
            }

            return string.Empty;
        }

        private static string BuildSqlLimit(this SearchParameters searchParameters)
        {
            var pageInfo = searchParameters.Page;
            var skipCount = (pageInfo.CurrentPage - 1) * pageInfo.PageSize;
            var pageSize = pageInfo.PageSize;
            return ($" LIMIT {skipCount}, {pageSize}");
        }

        private static string BuildSqlWhere(this IEnumerable<Condition> conditions, ref Dictionary<string, object> sqlParams, string keywords = " AND ")
        {
            if (conditions == null || !conditions.Any())
                return string.Empty;

            var sqlParamIndex = 1;
            var sqlExps = new List<string>();
            foreach (var condition in conditions)
            {
                var index = sqlParams.Count + sqlParamIndex;
                switch (condition.Op)
                {
                    case Operation.Equals:
                        sqlExps.Add($"{condition.Field} = @Param{index}");
                        sqlParams[$"Param{index}"] = condition.Value;
                        break;
                    case Operation.NotEquals:
                        sqlExps.Add($"{condition.Field} <> @Param{index}");
                        sqlParams[$"Param{index}"] = condition.Value;
                        break;
                    case Operation.Contains:
                        sqlExps.Add($"{condition.Field} LIKE @Param{index}");
                        sqlParams[$"Param{index}"] = $"%{condition.Value}%";
                        break;
                    case Operation.NotContains:
                        sqlExps.Add($"{condition.Field} NOT LIKE @Param{index}");
                        sqlParams[$"Param{index}"] = $"%{condition.Value}%";
                        break;
                    case Operation.StartsWith:
                        sqlExps.Add($"{condition.Field} LIKE @Param{index}");
                        sqlParams[$"Param{index}"] = $"%{condition.Value}";
                        break;
                    case Operation.EndsWith:
                        sqlExps.Add($"{condition.Field} LIKE @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}%";
                        break;
                    case Operation.GreaterThen:
                        sqlExps.Add($"{condition.Field} > @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                    case Operation.GreaterThenOrEquals:
                        sqlExps.Add($"{condition.Field} >= @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                    case Operation.LessThan:
                        sqlExps.Add($"{condition.Field} < @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                    case Operation.LessThanOrEquals:
                        sqlExps.Add($"{condition.Field} <= @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                    case Operation.StdIn:
                        sqlExps.Add($"{condition.Field} IN @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                    case Operation.StdNotIn:
                        sqlExps.Add($"{condition.Field} NOT IN @Param{index}");
                        sqlParams[$"Param{index}"] = $"{condition.Value}";
                        break;
                }

                sqlParamIndex += 1;
            }

            return sqlExps.Count > 1 ? string.Join(keywords, sqlExps) : sqlExps[0];
        }
    }
}