using Xunit;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicSearch.Test
{
    using DynamicSearch.Core;
    using DynamicSearch.Extenstions;

    public class EntityFrameworkTest
    {
        private IServiceProvider _serviceProvider;
        public EntityFrameworkTest()
        {
            var serviceCollection = new ServiceCollection();
        }

        [Fact]
        public void Test_Query_StdIn()
        {
            using (var context = new ChinookContext())
            {
                var searchParameters = new SearchParameters();
                searchParameters.Query = new QueryModel();
                searchParameters.Query.Add(new Condition() { Field = "AlbumId", Op = Operation.StdIn, Value = new int[] { 1, 2, 3} });
                var results = context.Album.Search(searchParameters);
                Assert.True(results.Count() == 3);
            }
        }
    }
}
