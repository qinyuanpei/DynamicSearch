# DynamicSearch
Expression Tree Based Condition Builder for `Entity Framework` & `Entity Framework Core` & `Dapper`

# Quick Start
* Pull Source Code & Build Project:
```
git clone git@github.com:qinyuanpei/DynamicSearch.git
```
* Add Reference To Your Project:
```CSharp
using DynamicSearch.Core;
using DynamicSearch.Extenstions;
```
* Example of `Entity Framework` & `Entity Framework Core`:
```CSharp
var searchParameters = new SearchParameters();
searchParameters.QueryModel = new QueryModel();
searchParameters.QueryModel.Add(new Condition() { Field = "ArtistId", Op = Operation.Equals, Value = 1 });
searchParameters.QueryModel.Add(new Condition() { Field = "AlbumId", Op = Operation.StdIn, Value = new int[] { 1, 2, 3} });
var results = context.Album.Search(searchParameters);

//等价形式
var results = context.Album.Where(x => x.ArtistId == 1 & new int[] { 1, 2, 3 }.Contains(x.AlbumId))
```
* Example of `Dapper`:
```CSharp
using (var connection = new SQLiteConnection("Data Source=Chinook.db"))
{
    var searchParameters = new SearchParameters();
    searchParameters.QueryModel.Add(new Condition() { Field = "ArtistId", Op = Operation.Equals, Value = 1 });
    searchParameters.QueryModel.Add(new Condition() { Field = "AlbumId", Op = Operation.StdIn, Value = new int[] { 1, 2, 3} });
    var results = connection.Search<Album>(searchParameters);
     
    //等价形式
    var results = connection.Query<Album>("SELECT * FROM Album WHERE ArtistId = @ArtistId AND AlbumId IN @AlbumId", 
        new { ArtistId = 1, AlbumId = new int[] { 1, 2, 3 } });
}
```
* More Features:

Equals, NotEquals, LessThan, LessThanOrEquals, GreaterThan, GreaterThanOrEquals, StdIn, StdNotIn, Contains, NotContains, StartsWith, EndsWith, Pagination, Sort


# Related Links：
* [https://blog.yuanpei.me/posts/118272597/](https://blog.yuanpei.me/posts/118272597/)

