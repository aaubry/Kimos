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

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kimos.Syntax
{
    public interface IUnpreparedComandBuilderSyntax<TEntity, TParams>
	{
        /// <summary>
        /// Executes the command synchronously.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the number of rows that were affected by the command.</returns>
		int Execute(DatabaseFacade database, TParams parameters);

        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <param name="database">The database where the command is to be executed.</param>
        /// <param name="parameters">The command parameters.</param>
        /// <returns>Returns the number of rows that were affected by the command.</returns>
        Task<int> ExecuteAsync(DatabaseFacade database, TParams parameters);
	}
}