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

using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Kimos.Syntax
{
    public interface IInsertComandBuilderSyntax<TEntity, TParams> : ICommandTextBuilderSyntax
	{
        /// <summary>
        /// Builds a SQL "insert or update" command.
        /// </summary>
        /// <param name="conflictColumns">An expression that selects the list of columns that will be used as identity for the operation.</param>
        /// <param name="update">An expression that specifies how to update the record, if it exists.</param>
        IUpdateComandBuilderSyntax<TEntity, TParams> OrUpdate(Expression<ColumnSpecificationDelegate<TEntity>> conflictColumns, Expression<UpdateSpecificationDelegate<TEntity, TParams>> update);

        /// <summary>
        /// Specifies which columns are to be returned after the insertion.
        /// </summary>
        /// <typeparam name="TResult">The type of the object that will hold the values.</typeparam>
        /// <param name="output">An expression that specifies which values should be returned by the command.</param>
        IOutputComandBuilderSyntax<TEntity, TParams, TResult> Output<TResult>(Expression<OutputSpecificationDelegate<TEntity, TResult>> output);

        /// <summary>
        /// Creates a command executor for the current command.
        /// </summary>
        /// <param name="context">The <see cref="DbContext" /> for which the code is to be generated.</param>
        IUnpreparedComandBuilderSyntax<TEntity, TParams> Create(DbContext context);
    }
}