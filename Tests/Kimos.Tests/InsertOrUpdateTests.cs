// Copyright (C) 2018 Antoine Aubry
// 
// This file is part of Kimos.
// 
// Kimos is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Kimos is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Kimos.  If not, see <http://www.gnu.org/licenses/>.
// 

using AutoFixture;
using Kimos.Tests.Common.DataModel;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Kimos.Tests
{
    public class InsertOrUpdateTests
    {
        private readonly DirectSqlCommandBuilder builder = new DirectSqlCommandBuilder();

        private readonly Fixture fixture;
        private readonly ITestOutputHelper output;

        public InsertOrUpdateTests(ITestOutputHelper output)
        {
            fixture = new Fixture();

            fixture.Customize<TestEntity>(b => b
                .Without(e => e.Id)
            );
            this.output = output;
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void Insert(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();

            // Act
            int insertCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                output.WriteLine($"####### {context.Database.ProviderName}");
                insertCount = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.Equal(1, insertCount);
            using (var context = contextFactory.CreateDbContext(null))
            {
                var exists = context.Entities.Any(e => e.Name == upsertParams.Name);
                Assert.True(exists);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();

            // Act
            InsertResult insertResult;
            using (var context = contextFactory.CreateDbContext(null))
            {
                insertResult = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .Output(e => new InsertResult { Id = e.Id, Version = e.Version })
                    .Log(context, output)
                    .Create(context)
                    .QueryFirst(context.Database, upsertParams);
            }

            // Assert
            Assert.NotNull(insertResult);
            Assert.Equal(upsertParams.Version, insertResult.Version);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Id == insertResult.Id);
                Assert.Equal(upsertParams.Name, inserted.Name);
                Assert.Equal(upsertParams.Version, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertNew(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.Equal(1, rowCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertExisting(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.Equal(1, rowCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertNewOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();

            // Act
            UpsertParams result;
            using (var context = contextFactory.CreateDbContext(null))
            {
                result = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Output(e => new UpsertParams
                    {
                        Id = e.Id,
                        Version = e.Version,
                    })
                    .Log(context, output)
                    .Create(context)
                    .QueryFirstOrDefault(context.Database, upsertParams);
            }

            // Assert
            Assert.NotNull(result);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version, inserted.Version);
                Assert.Equal(inserted.Id, result.Id);
                Assert.Equal(inserted.Version, result.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertExistingOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            int existingId;
            using (var context = contextFactory.CreateDbContext(null))
            {
                var entity = new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                };
                context.Entities.Add(entity);
                context.SaveChanges();

                existingId = entity.Id;
            }

            // Act
            UpsertParams result;
            using (var context = contextFactory.CreateDbContext(null))
            {
                result = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Output(e => new UpsertParams
                    {
                        Id = e.Id,
                        Version = e.Version,
                    })
                    .Log(context, output)
                    .Create(context)
                    .QueryFirstOrDefault(context.Database, upsertParams);
            }

            // Assert
            Assert.NotNull(result);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(existingId, inserted.Id);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);
                Assert.Equal(inserted.Id, result.Id);
                Assert.Equal(inserted.Version, result.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertExistingWhere(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            int existingId;
            using (var context = contextFactory.CreateDbContext(null))
            {
                var entity = new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                };
                context.Entities.Add(entity);
                context.SaveChanges();

                existingId = entity.Id;
            }

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Where((e, p) => e.Version < 0)
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.Equal(0, rowCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var existing = context.Entities.Single(e => e.Id == existingId);
                Assert.Equal(upsertParams.Version, existing.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpsertExistingWhereOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            int existingId;
            using (var context = contextFactory.CreateDbContext(null))
            {
                var entity = new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                };
                context.Entities.Add(entity);
                context.SaveChanges();

                existingId = entity.Id;
            }

            // Act
            UpsertParams result;
            using (var context = contextFactory.CreateDbContext(null))
            {
                result = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .OrUpdate(
                        e => new { e.Name },
                        (e, p) => new TestEntity
                        {
                            Version = e.Version + 1,
                        }
                    )
                    .Where((e, p) => e.Version < 0)
                    .Output(e => new UpsertParams
                    {
                        Id = e.Id,
                        Version = e.Version,
                    })
                    .Log(context, output)
                    .Create(context)
                    .QueryFirstOrDefault(context.Database, upsertParams);
            }

            // Assert
            Assert.Null(result);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var existing = context.Entities.Single(e => e.Id == existingId);
                Assert.Equal(upsertParams.Version, existing.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateExisting(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = e.Version + 1,
                    })
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.NotEqual(0, rowCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateExistingOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            IEnumerable<UpsertParams> results;
            using (var context = contextFactory.CreateDbContext(null))
            {
                results = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = e.Version + 1,
                    })
                    .Output(e => new UpsertParams { Id = e.Id, Version = e.Version })
                    .Log(context, output)
                    .Create(context)
                    .Query(context.Database, upsertParams);
            }

            // Assert
            Assert.NotEmpty(results);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);

                var result = results.Single(r => r.Id == inserted.Id);
                Assert.Equal(upsertParams.Version + 1, result.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateExistingWhereOutput(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            IEnumerable<UpsertParams> results;
            using (var context = contextFactory.CreateDbContext(null))
            {
                results = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = e.Version + 1,
                    })
                    .Where((e, p) => e.Name == p.Name)
                    .Output(e => new UpsertParams { Id = e.Id, Version = e.Version })
                    .Log(context, output)
                    .Create(context)
                    .Query(context.Database, upsertParams);
            }

            // Assert
            Assert.Single(results);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);

                var result = results.Single();
                Assert.Equal(upsertParams.Version + 1, result.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateExistingWhere(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            int updateCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                updateCount = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = e.Version + 1,
                    })
                    .Where((e, p) => e.Name == p.Name)
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.Equal(1, updateCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version + 1, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void OutputAnonymousType(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();

            // Act
            using (var context = contextFactory.CreateDbContext(null))
            {
                var insertResult = builder
                    .CreateCommand<TestEntity>()
                    .Insert<UpsertParams>(p => new TestEntity { Name = p.Name, Version = p.Version })
                    .Output(e => new { e.Id, e.Version })
                    .Log(context, output)
                    .Create(context)
                    .QueryFirst(context.Database, upsertParams);

                // Assert
                Assert.NotNull(insertResult);
                Assert.Equal(upsertParams.Version, insertResult.Version);

                var inserted = context.Entities.Single(e => e.Id == insertResult.Id);
                Assert.Equal(upsertParams.Name, inserted.Name);
                Assert.Equal(upsertParams.Version, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateWithTernary(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = e.Version < 5 ? e.Version + 1 : e.Version,
                    })
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.NotEqual(0, rowCount);

            using (var context = contextFactory.CreateDbContext(null))
            {
                var inserted = context.Entities.Single(e => e.Name == upsertParams.Name);
                Assert.Equal(upsertParams.Version < 5 ? upsertParams.Version + 1 : upsertParams.Version, inserted.Version);
            }
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void InsertOrUpdate_UpdateWithNullable(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var upsertParams = fixture.Create<UpsertParams>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(new TestEntity
                {
                    Name = upsertParams.Name,
                    Version = upsertParams.Version,
                });
                context.SaveChanges();
            }

            upsertParams.NullableVersion = null;

            // Act
            int rowCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                rowCount = builder
                    .CreateCommand<TestEntity>()
                    .Update<UpsertParams>((e, p) => new TestEntity
                    {
                        Version = 1,
                    })
                    .Where((e, p) => e.Version == p.NullableVersion || p.NullableVersion == null)
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, upsertParams);
            }

            // Assert
            Assert.NotEqual(0, rowCount);
        }

        private class UpsertParams
        {
            public int Id { get; set; }
            public int Version { get; set; }
            public int? NullableVersion { get; set; }
            public string Name { get; set; }
        }

        private class InsertResult
        {
            public int Id { get; set; }
            public int Version { get; set; }
        }
    }
}
