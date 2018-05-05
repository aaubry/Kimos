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

using System.Linq.Expressions;

namespace Kimos.Syntax
{
    public interface ICommandBuilderSyntax<TEntity>
	{
        /// <summary>
        /// Builds a SQL INSERT / upsert command.
        /// </summary>
        /// <typeparam name="TParams">The type of the command parameters.</typeparam>
        /// <param name="insert">An expression that specifies the value of the record to be inserted.</param>
        IInsertComandBuilderSyntax<TEntity, TParams> Insert<TParams>(Expression<InsertSpecificationDelegate<TEntity, TParams>> insert);

        /// <summary>
        /// Builds a SQL DELETE command.
        /// </summary>
        /// <typeparam name="TParams">The type of the command parameters.</typeparam>
        /// <param name="predicate">An expression that specifies which rows are to be deleted.</param>
        IDeleteComandBuilderSyntax<TEntity, TParams> Delete<TParams>(Expression<PredicateSpecificationDelegate<TEntity, TParams>> predicate);

        /// <summary>
        /// Builds a SQL UPDATE command.
        /// </summary>
        /// <typeparam name="TParams">The type of the command parameters.</typeparam>
        /// <param name="update">An expression that specifies how to update the record(s).</param>
        IUpdateComandBuilderSyntax<TEntity, TParams> Update<TParams>(Expression<UpdateSpecificationDelegate<TEntity, TParams>> update);
    }
}