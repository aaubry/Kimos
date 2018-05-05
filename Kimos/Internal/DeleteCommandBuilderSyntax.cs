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
    internal class DeleteCommandBuilderSyntax<TEntity, TParams>
        : CommandBuilderSyntax<TEntity, TParams>
        , IDeleteCommand<TEntity, TParams>
        , IDeleteComandBuilderSyntax<TEntity, TParams>
        where TEntity : class
    {
        private readonly IDatabaseDriverRegistry drivers;
        private readonly Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate;

        Expression<PredicateSpecificationDelegate<TEntity, TParams>> IDeleteCommand<TEntity, TParams>.Predicate => predicate;

        public DeleteCommandBuilderSyntax(IDatabaseDriverRegistry drivers, Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate)
        {
            this.drivers = drivers;
            this.predicate = predicate;
        }

        public IUnpreparedComandBuilderSyntax<TEntity, TParams> Create(DbContext context)
        {
            var commandText = BuildCommandText(context);
            return new UnpreparedCommandExecutor<TEntity, TParams, object>(commandText);
        }

        public string BuildCommandText(DbContext context)
        {
            var metadata = GetMetadata(context);
            return drivers
                .SelectDriver(context.Database.ProviderName)
                .CreateDeleteCommandGenerator()
                .GenerateSqlCommand(this, metadata);
        }
    }
}