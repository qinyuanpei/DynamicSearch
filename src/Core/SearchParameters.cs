using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Reflection.Emit;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace DynamicSearch.Core
{

    public class SearchParameters
    {
        public PageModel Page { get; set; }
        public QueryModel Query { get; set; }
    }

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

    public class PageModel
    {
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 500;
        public SortType SortType { get; set; }
        public List<string> SortFields { get; set; }
    }

    public enum SortType
    {
        Asc = 10,
        Desc = 20
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