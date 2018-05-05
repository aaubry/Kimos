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

using Kimos.Drivers;
using Kimos.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Kimos.Internal
{
    internal class CommandBuilderSyntax<TEntity> : ICommandBuilderSyntax<TEntity> where TEntity : class
	{
        private readonly IDatabaseDriverRegistry drivers;

        public CommandBuilderSyntax(IDatabaseDriverRegistry drivers)
        {
            this.drivers = drivers;
        }

		public IInsertComandBuilderSyntax<TEntity, TParams> Insert<TParams>(Expression<InsertSpecificationDelegate<TEntity, TParams>> insert)
		{
			return new InsertOrUpdateCommandBuilderSyntax<TEntity, TParams, object>(drivers, insert);
		}

        public IUpdateComandBuilderSyntax<TEntity, TParams> Update<TParams>(Expression<UpdateSpecificationDelegate<TEntity, TParams>> update)
        {
            return new InsertOrUpdateCommandBuilderSyntax<TEntity, TParams, object>(drivers, update);
        }

		public IDeleteComandBuilderSyntax<TEntity, TParams> Delete<TParams>(Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate)
		{
			return new DeleteCommandBuilderSyntax<TEntity, TParams>(drivers, predicate);
		}
    }

	internal class CommandBuilderSyntax<TEntity, TParams>
	{
		protected QueryMetadata GetMetadata(DbContext context)
		{
            var entityType = context.Model.GetEntityTypes(typeof(TEntity)).Single();

            var tableName = GetAnnotation(entityType, "Relational:TableName");

            entityType.GetDeclaredProperties()
                .ToDictionary(p => p.Name, p => GetAnnotation(p, "Relational:ColumnName", p.Name));

            return new QueryMetadata
            {
                TableName = GetAnnotation(entityType, "Relational:TableName"),
                TableSchema = GetAnnotation(entityType, "Relational:Schema", null),
                ColumnMappings = entityType.GetDeclaredProperties()
                    .ToDictionary(p => p.Name, p => GetAnnotation(p, "Relational:ColumnName", p.Name)),
            };
		}

        private string GetAnnotation(IAnnotatable annotatable, string annotationName)
        {
            return annotatable.GetAnnotations()
                .Where(a => a.Name == annotationName)
                .Select(a => (string)a.Value)
                .Single();
        }

        private string GetAnnotation(IAnnotatable annotatable, string annotationName, string defaultValue)
        {
            return annotatable.GetAnnotations()
                .Where(a => a.Name == annotationName)
                .Select(a => (string)a.Value)
                .SingleOrDefault()
                ?? defaultValue;
        }
    }
}