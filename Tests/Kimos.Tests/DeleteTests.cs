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
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Kimos.Tests
{
    public class DeleteTests
    {
        private readonly DirectSqlCommandBuilder builder = new DirectSqlCommandBuilder();

        private readonly Fixture fixture;
        private readonly ITestOutputHelper output;

        public DeleteTests(ITestOutputHelper output)
        {
            fixture = new Fixture();

            fixture.Customize<TestEntity>(b => b
                .Without(e => e.Id)
            );
            this.output = output;
        }

        [Theory]
        [ClassData(typeof(DbContextFactories))]
        public void DeleteById(IDesignTimeDbContextFactory<TestDbContext> contextFactory)
        {
            // Arrange
            var entity = fixture.Create<TestEntity>();
            using (var context = contextFactory.CreateDbContext(null))
            {
                context.Entities.Add(entity);
                context.SaveChanges();
            }

            // Act
            int deleteCount;
            using (var context = contextFactory.CreateDbContext(null))
            {
                deleteCount = builder
                    .CreateCommand<TestEntity>()
                    .Delete<DeleteParams>((e, p) => e.Id == p.Id)
                    .Log(context, output)
                    .Create(context)
                    .Execute(context.Database, new DeleteParams { Id = entity.Id });
            }

            // Assert
            Assert.Equal(1, deleteCount);
            using (var context = contextFactory.CreateDbContext(null))
            {
                var exists = context.Entities.Any(e => e.Name == entity.Name);
                Assert.False(exists);
            }
        }

        private class DeleteParams
        {
            public int Id { get; set; }
        }
    }
}
