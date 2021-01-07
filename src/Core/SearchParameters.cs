using System;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace DynamicSearch.Core
{
    [Serializable]
    public class SearchParameters
    {
        public PageModel PageModel { get; set; } = PageModel.Default;
        public QueryModel QueryModel { get; set; } = new QueryModel();
    }

    [Serializable]
    public class QueryModel : List<Condition>
    {
        public void Add<T>(Condition<T> condition) where T : class
        {
            var filedName = string.Empty;
            var memberExp = condition.Field.Body as MemberExpression;
            if (memberExp == null)
            {
                var ubody = (UnaryExpression)condition.Field.Body;
                memberExp = ubody.Operand as MemberExpression;
            }
            filedName = memberExp.Member.Name;
            Add(new Condition() { Field = filedName, Op = condition.Op, Value = condition.Value, OrGroup = condition.OrGroup });
        }
    }

    [Serializable]
    public class PageModel
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public List<SortField> SortFields { get; set; }
        public static PageModel Default => new PageModel() { PageSize = 500, CurrentPage = 1 };
    }

    public enum SortType
    {
        Asc = 10,
        Desc = 20
    }

    [Serializable]
    public class SortField
    {
        public SortType SortType { get; set; }
        public string Field { get; set; }
    }

    public enum Operation
    {
        Equals = 10,
        NotEquals = 20,
        LessThan = 30,
        LessThanOrEquals = 40,
        GreaterThen = 50,
        GreaterThenOrEquals = 60,
        StdIn = 70,
        StdNotIn = 80,
        Contains = 90,
        NotContains = 100,
        StartsWith = 110,
        EndsWith = 120
    }

    [Serializable]
    public class Condition
    {
        public string Field { get; set; }
        public Operation Op { get; set; }
        public object Value { get; set; }
        public string OrGroup { get; set; }
    }

    public class Condition<T> : Condition
    {
        public new Expression<Func<T, dynamic>> Field { get; set; }
    }
}