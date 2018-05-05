
# Introduction

Kimos is a library that extends Entity Framework, enabling the use of CRUD operations without having to load the entities first. It currently supports the following operations:

* Insert
* Update
* Upsert (insert or update)
* Delete

# Motivation

Entity Framework is an excelent solution for data access. Most of the time the performance is adequate and it provides a powerful abstraction for the data access layer.

There are cases, however, where using Entity Framework compromises non-functional requirements. For example, if an operation requires to perform an "insert or update", Entity Framework requires you to read the existing record, if it exists, then either update or insert it. This can be problematic when that operation needs to be performed in isolation. In order to defend against two or more instances of the same operation executing concurrently, one needs to use the "serializable" isolation level, which can be a source of contention and / or deadlocks.

A solution for these issues is to use a SQL query directly. By taking advantage of the full SQL language, it is possible to write a single statement that performs an operation that would have required multiple roundtrips and a transaction using Entity Framework.

The usual strategy to achieve this is to embed SQL statements in the parts of the code that cannot use Entity Framework. This works, but at the expense of giving up on the strongly-typed syntax offered by Entity Framework. For example, is we misspell a column name, the compiler won't catch that error, and the code will fail at runtime.

Kimos provides a strongly-typed syntax for a set of commonly used SQL constructs, while abstracting the syntatic differences between different SQL implementations.

# Usage

All operations are executed in two steps. First a command is created and configured using a fluent interface. Then, the command is executed one or many times. Each time a command is executed, it can receive different arguments.

To create a command, start with an instance of `DirectSqlCommandBuilder` and call its `CreateCommand<T>` method, specifiying the type of the entity that this command will refer to. This type should be an entity type from your `DbContext`. Then, use the fluent methods to configure the command, finishing with the `Create` method. The resulting object allows to execute the command and retrieve its output, if applicable.

The following examples illustrate most use cases. The unit tests also contain good usage examples.

## Insert

Inserts a record, optionally returning one or more of the inserted values.

### Insert a record

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Insert<InsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
    .Create(context);

var insertCount = cmd.Execute(context.Database, new InsertParams
{
    Name = "one",
    Version = 1,
});
```

### Insert a record, retrieving the generated id

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Insert<InsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
    .Output(e => new { Id = e.Id })
    .Create(context);

var result = cmd.QueryFirst(context.Database, new InsertParams
{
    Name = "one",
    Version = 1,
});

// Use result.Id
```

## Update

### Update all records matching a criteria and increment the version number

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Update<UpdateParams>((e, p) => new TestEntity
    {
        Amount = p.Amount,
        Version = e.Version + 1
    })
    .Where((e, p) => e.Category == p.Category)
    .Create(context);

var updateCount = cmd.Execute(context.Database, new UpdateParams
{
    Amount = 5.99,
    Category = "Books",
});
```

### Update all records matching a criteria, increment and return the version number

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Update<UpdateParams>((e, p) => new TestEntity
    {
        Amount = p.Amount,
        Version = e.Version + 1
    })
    .Where((e, p) => e.Category == p.Category)
    .Output(e => new { e.Id, e.Version })
    .Create(context);

var updateList = cmd.Query(context.Database, new UpdateParams
{
    Amount = 5.99,
    Category = "Books",
});

foreach (var update in updateList)
{
    // Use update.Id and update.Version
}
```

## Upsert (insert or update)

### Insert a record, or increment its version

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
    .OrUpdate(
        e => new { e.Name },
        (e, p) => new TestEntity
        {
            Version = e.Version + 1,
        }
    )
    .Create(context);

var updateCount = cmd.Execute(context.Database, new UpsertParams
{
    Name = "one",
    Version = 1,
});    
```

## Delete

### Delete by id

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Delete<DeleteParams>((e, p) => e.Id == p.Id)
    .Create(context);

var deleteCount = cmd.Execute(context.Database, new DeleteParams
{
    Id = 123,
});
```

### Delete many and retrieve the deleted data

```c#
DbContext context; // Initialization of the DbContext not shown
var builder = new DirectSqlCommandBuilder();

var cmd = builder
    .CreateCommand<TestEntity>()
    .Delete<DeleteParams>((e, p) => e.Category == p.Category)
    .Output(e => new { e.Id, e.Name })
    .Create(context);

var deleteList = cmd.Execute(context.Database, new DeleteParams
{
    Category = "Books",
});

foreach (var deleted in deleteList)
{
    // Use deleted.Id and deleted.Version
}
```

# Syntax independence

Each SQL implementation has it own specific syntax peculiarities. Kimos abstract these differences by constructing a generic representation of the command, then delegating the SQL code generation to an adapter.

## Supporting other syntaxes

The database adapters are implementations of the `IDatabaseDriver` interface. Kimos contains built-in implementations for the following databases:
- SQL Server
- PostgreSql

Adding support for another syntax is a matter of adding an implementation of that interface, and registering it on `DirectSqlCommandBuilder`:

```c#
var builder = new DirectSqlCommandBuilder(new DatabaseDriverRegistry
{
    { "Pomelo.EntityFrameworkCore.MySql", new MySqlDriver() },
});
```

# Why is it named "Kimos" ?

For no particular reason. This name was chosen from a set of randomly generated names.

# License

Kimos is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Kimos is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

See the [LICENSE](LICENSE) file for a copy of the GNU General Public License.

## Clarification

Refer to [tldrlegal.com](https://tldrlegal.com/license/gnu-general-public-license-v3-(gpl-3)) for a simpler explanation of what you **can**, **cannot** and **must** do if you use this code.

Notice also that most restrictions of the GPL **apply only if you redistribute** your application. From the [GPL FAQ](https://www.gnu.org/licenses/gpl-faq.en.html#GPLRequireSourcePostedPublic):

> The GPL does not require you to release your modified version, or any part of it. You are free to make modifications and use them privately, without ever releasing them. This applies to organizations (including companies), too; an organization can make a modified version and use it internally without ever releasing it outside the organization.
>
> But if you release the modified version to the public in some way, the GPL requires you to make the modified source code available to the program's users, under the GPL.
>
> Thus, the GPL gives permission to release the modified program in certain ways, and not in other ways; but the decision of whether to release it is up to you.
