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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kimos.Syntax
{
    public interface IUnpreparedOutputComandBuilderSyntax<TEntity, TParams, TResult>
	{
        /// <summary>
        /// Executes the command synchronously and returns all results.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the output rows returned by the command.</returns>
		IEnumerable<TResult> Query(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command asynchronously and returns all results.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the output rows returned by the command.</returns>
        Task<IEnumerable<TResult>> QueryAsync(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command synchronously and returns the first result.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the first output row returned by the command.</returns>
		TResult QueryFirst(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command asynchronously and returns the first result.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the first output row returned by the command.</returns>
        Task<TResult> QueryFirstAsync(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command synchronously and returns the first result.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the first output row returned by the command, or default(TResult) if no rows are returned.</returns>
		TResult QueryFirstOrDefault(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command asynchronously and returns the first result.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the first output row returned by the command, or default(TResult) if no rows are returned.</returns>
        Task<TResult> QueryFirstOrDefaultAsync(DatabaseFacade database, TParams parameters);
	}
}