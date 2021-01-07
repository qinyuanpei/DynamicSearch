using System;
using Xunit;
using System.Linq;
using DynamicSearch.Core;
using System.Data.SQLite;
using Dapper;
using DynamicSearch.Extenstions;
using System.Collections.Generic;

namespace DynamicSearch.Test
{
    using DynamicSearch.Core;
    using DynamicSearch.Extenstions;

    public class DapperTest
    {
        [Fact]
        public void Test_SpecialCase()
        {
            var list = new List<Foo>();
            var query = new QueryModel();
            query.Add(new Condition() { Field = "p2", Op = Operation.Equals, Value = 1 });
            query.Add(new Condition() { Field = "p2", Op = Operation.StdIn, Value = new List<int> { 1, 2, 3 } });
            var lambdaExp = LambdaExpressionBuilder.BuildLambda<Foo>(query);
            var lambda = lambdaExp.Compile();
            list = list.Where(lambda).ToList();
        }

        [Fact]
        public void Test_Query_Equal()
        {
            using (var connection = new SQLiteConnection("Data Source=Chinook.db"))
            {
                var searchParameters = new SearchParameters();
                searchParameters.QueryModel.Add(new Condition() { Field = "AlbumId", Op = Operation.Equals, Value = 1 });
                var list = connection.Search<Album>(searchParameters);
                Assert.True(list.Count() == 1);
            }
        }

        public class Foo
        {
            public List<string> p1 { get; set; }
            public int? p2 { get; set; }
            public string[] p3 { get; set; }
        }
    }
}
