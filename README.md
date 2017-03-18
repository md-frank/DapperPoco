# DapperPoco

基于[Dapper](https://github.com/StackExchange/Dapper)的、轻量级的、高性能的、简单的、灵活的ORM框架
1. 高性能（与Dapper一致），以热启动后计算（第一次启动有缓存过程）
2. 像EF一样使用简单，也可像Dapper一样灵活使用原生SQL
3. 支持使用Fluent API定义实体映射

## 准备工作

### 首先定义一个Poco类

```csharp
//表示文章表里的一条记录
public class Article
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
}
```

### 创建DbContext

```csharp
class MasterDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseConnectionString("连接字符串");
		//使用SQL Server数据库
        optionsBuilder.UseSqlAdapter(new SqlServerAdapter(SqlClientFactory.Instance));
    }

	//如果不使用Poco可以不重写此方法
    protected override void OnEntitiesBuilding(EntitiesBuilder entityBuilder)
    {
		//属性名与表列名（列名）不一样，可在此映射别名
        entityBuilder.Entity<Article>()
            .TableName("T_Article")
            .ColumnName(p => p.Id, "article_id");
    }
}
```

## 使用示例

### 插入数据

```csharp
var masterDb = new MasterDbContext();

//插入一个Poco对象
var a = new Article 
{
	Title = "hello",
	Content = "hello word"
};
masterDb.Insert(a);

//插入了2条记录
masterDb.Insert(new Article[] { a, a });

//也可以显式指定表名
masterDb.Insert(a, "T_Article");

//原生SQL插入
this.Execute("insert T_Article(Title, Content) values (@Title, @Content)", a);

//插入了2条记录
this.Execute("insert T_Article(Title, Content) values (@Title, @Content)", a, a);

//插入了2条记录
this.Execute("insert T_Article(Title, Content) values (@Title, @Content)", new Article[] { a, a });

//也可以直接写参数值
this.Execute("insert T_Article(Title, Content) values (@p0, @p1)", "hello", "hello word");
```

### 更新数据

```csharp
var masterDb = new MasterDbContext();

//先查出来准备更新
var article = masterDb.FirstOrDefault<Article>("select * from T_Article where article_id = @p0", 1);

//更新除主键外的所有列
article.Title = "hello 2";
article.Content = "content 1";
masterDb.Update(article);

//仅更新指定列，指定表列名
article.Title = "hello 2";
masterDb.Update(article, new [] { "Title" });

//仅更新指定列，指定实体属性名
article.Title = "hello 3";
article.Content = "content 1";
masterDb.Update(article, null, null, p=> p.Title, p=> p.Content);
```

### 保存数据

```csharp
var masterDb = new MasterDbContext();

var article = new Article 
{
	Id = 1,
	Title = "hello",
	Content = "hello word"
};

//如果记录存在则更新，不存在则插入
masterDb.Save(article);

//保存并指定列名
masterDb.Save(article, new [] { "Title" });
```

### 删除数据

```csharp
var masterDb = new MasterDbContext();

var article = masterDb.FirstOrDefault<Article>("select * from T_Article where article_id = @p0", 1);

//删除实体记录
masterDb.Delete(article);

//删除实体记录，显式指定主键名
masterDb.Delete(article, "article_id");
```

### 查询数据（立即执行）

```csharp
var masterDb = new MasterDbContext();

//查询T_Article表所有记录
var articles = masterDb.FetchAll<Article>();

//指定条件查询，直接写参数值
var articles = masterDb.Fetch<Article>("select * from T_Article where Title=@p0 and Content=@p1", "hello", "hello word");

//指定条件查询，支持列表（实现了IEnumerable接口的）
var articles = masterDb.Fetch<Article>("select * from T_Article where article_id in @p0", new [] { 1, 2, 3 });

//查询单条记录
masterDb.FirstOrDefault<Article>("select * from T_Article where article_id = @p0", 1);

//查询单列
var count = masterDb.ExecuteScalar<long>("select count(*) from T_Article");

//查询分页的结果（第1页，每页20条）
Paged<Article> paged = masterDb.Paged<Article>(1, 20, "select * from T_Article where Title=@p0", "hello");

//Paged的定义如下
public class Paged<T> where T : new()
{
    //当前页码
    public int CurrentPage { get; set; }

    //总页数
    public int TotalPages { get; set; }

    ///总记录数
    public long TotalItems { get; set; }

    //每页记录数
    public int ItemsPerPage { get; set; }

    //当前页记录列表
    public List<T> Items { get; set; }
}
```

### 查询数据（延迟执行）

延迟查询使用Query，与Fetch不同的是Query返回的结果只有在使用时才会真正查询数据库
```csharp
var masterDb = new MasterDbContext();

//延迟查询
var articles = masterDb.Query<Article>("select * from T_Article where Title=@p0", "hello");
```

### 动态查询条件

```csharp
var title = "此变量来自用户输入";

var sb = new SqlBuilder();
sb.Append("select * from T_Article");
if(!string.IsNullOrEmpty(title))
	sb.Append("where Title=@p0", title);

var sql = sb.Build();
var articles = masterDb.Fetch<Article>(sql.Statement, sql.Parameters);
```

### 事务支持

```csharp
using (var trans = this.GetTransaction())
{
    //这里修改数据库

    //提交事务
    trans.Complete();
}
```
