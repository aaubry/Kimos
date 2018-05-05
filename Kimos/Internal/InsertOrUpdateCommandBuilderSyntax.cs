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
using System.Linq.Expressions;

namespace Kimos.Internal
{
    internal class InsertOrUpdateCommandBuilderSyntax<TEntity, TParams, TResult>
		: CommandBuilderSyntax<TEntity, TParams>
        , IInsertOrUpdateCommand<TEntity, TParams, TResult>
        , IInsertComandBuilderSyntax<TEntity, TParams>
		, IOutputComandBuilderSyntax<TEntity, TParams, TResult>
		, IUpdateComandBuilderSyntax<TEntity, TParams>
		where TEntity : class
	{
        private readonly IDatabaseDriverRegistry drivers;

        private Expression<InsertSpecificationDelegate<TEntity, TParams>> insert;
		private Expression<OutputSpecificationDelegate<TEntity, TResult>> output;
		private Expression<ColumnSpecificationDelegate<TEntity>> conflictColumns;
		private Expression<UpdateSpecificationDelegate<TEntity, TParams>> update;
		private Expression<PredicateSpecificationDelegate<TEntity, TParams>> updatePredicate;

        Expression<InsertSpecificationDelegate<TEntity, TParams>> IInsertOrUpdateCommand<TEntity, TParams, TResult>.Insert => insert;
        Expression<OutputSpecificationDelegate<TEntity, TResult>> IInsertOrUpdateCommand<TEntity, TParams, TResult>.Output => output;
        Expression<ColumnSpecificationDelegate<TEntity>> IInsertOrUpdateCommand<TEntity, TParams, TResult>.ConflictColumns => conflictColumns;
        Expression<UpdateSpecificationDelegate<TEntity, TParams>> IInsertOrUpdateCommand<TEntity, TParams, TResult>.Update => update;
        Expression<PredicateSpecificationDelegate<TEntity, TParams>> IInsertOrUpdateCommand<TEntity, TParams, TResult>.UpdatePredicate => updatePredicate;

        public InsertOrUpdateCommandBuilderSyntax(IDatabaseDriverRegistry drivers, Expression<InsertSpecificationDelegate<TEntity, TParams>> insert)
		{
            this.drivers = drivers;
			this.insert = insert;
		}

        public InsertOrUpdateCommandBuilderSyntax(IDatabaseDriverRegistry drivers, Expression<UpdateSpecificationDelegate<TEntity, TParams>> update)
        {
            this.drivers = drivers;
            this.update = update;
        }

        private InsertOrUpdateCommandBuilderSyntax(
            IDatabaseDriverRegistry drivers,
            Expression<InsertSpecificationDelegate<TEntity, TParams>> insert,
            Expression<OutputSpecificationDelegate<TEntity, TResult>> output,
            Expression<ColumnSpecificationDelegate<TEntity>> conflictColumns,
            Expression<UpdateSpecificationDelegate<TEntity, TParams>> update,
            Expression<PredicateSpecificationDelegate<TEntity, TParams>> updatePredicate
        )
        {
            this.drivers = drivers;
            this.insert = insert;
            this.output = output;
            this.conflictColumns = conflictColumns;
            this.update = update;
            this.updatePredicate = updatePredicate;
        }

		public IUpdateComandBuilderSyntax<TEntity, TParams> OrUpdate(Expression<ColumnSpecificationDelegate<TEntity>> conflictColumns, Expression<UpdateSpecificationDelegate<TEntity, TParams>> update)
		{
			this.conflictColumns = conflictColumns;
			this.update = update;
			return this;
        }

        public IUpdateWhereComandBuilderSyntax<TEntity, TParams> Where(Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate)
        {
            this.updatePredicate = predicate;
            return this;
        }

        public IOutputComandBuilderSyntax<TEntity, TParams, TNewResult> Output<TNewResult>(Expression<OutputSpecificationDelegate<TEntity, TNewResult>> output)
		{
			return new InsertOrUpdateCommandBuilderSyntax<TEntity, TParams, TNewResult>(drivers, insert, output, conflictColumns, update, updatePredicate);
		}

		public IUnpreparedComandBuilderSyntax<TEntity, TParams> Create(DbContext context)
		{
			return CreateInternal(context);
		}

		IUnpreparedOutputComandBuilderSyntax<TEntity, TParams, TResult> IOutputComandBuilderSyntax<TEntity, TParams, TResult>.Create(DbContext context)
		{
			return CreateInternal(context);
		}

        private UnpreparedCommandExecutor<TEntity, TParams, TResult> CreateInternal(DbContext context)
        {
            var commandText = BuildCommandText(context);
            return new UnpreparedCommandExecutor<TEntity, TParams, TResult>(commandText);
        }

        public string BuildCommandText(DbContext context)
        {
            var metadata = GetMetadata(context);
            return drivers
                .SelectDriver(context.Database.ProviderName)
                .CreateInsertOrUpdateCommandGenerator()
                .GenerateSqlCommand(this, metadata);
        }
    }
}