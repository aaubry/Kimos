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

using Dapper;
using Kimos.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Kimos.Internal
{
    internal class UnpreparedCommandExecutor<TEntity, TParams, TResult>
        : IUnpreparedComandBuilderSyntax<TEntity, TParams>
        , IUnpreparedOutputComandBuilderSyntax<TEntity, TParams, TResult>
    {
        private readonly string commandText;

        public UnpreparedCommandExecutor(string commandText)
        {
            this.commandText = commandText;
        }
        
        private static DbConnection GetConnectionAndTransaction(DatabaseFacade database, out DbTransaction transaction)
        {
            var connection = database.GetDbConnection();
            transaction = database.CurrentTransaction != null
                ? ((IInfrastructure<DbTransaction>)database.CurrentTransaction).Instance
                : null;
            return connection;
        }

        private T ExecuteInternal<T>(DatabaseFacade database, Func<DbConnection, DbTransaction, string, T> executor)
        {
            var connection = GetConnectionAndTransaction(database, out var transaction);
            using (connection.OpenAndTrack())
            {
                return executor(connection, transaction, commandText);
            }
        }

        private async Task<T> ExecuteInternalAsync<T>(DatabaseFacade database, Func<DbConnection, DbTransaction, string, Task<T>> executor)
        {
            var connection = GetConnectionAndTransaction(database, out var transaction);
            using (connection.OpenAndTrack())
            {
                return await executor(connection, transaction, commandText);
            }
        }

        public int Execute(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternal(database, (connection, transaction, commandText) => connection.Execute(commandText, parameters, transaction));
        }

        public Task<int> ExecuteAsync(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternalAsync(database, (connection, transaction, commandText) => connection.ExecuteAsync(commandText, parameters, transaction));
        }

        public TResult QueryFirst(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternal(database, (connection, transaction, commandText) => connection.QueryFirst<TResult>(commandText, parameters, transaction));
        }

        public Task<TResult> QueryFirstAsync(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternalAsync(database, (connection, transaction, commandText) => connection.QueryFirstAsync<TResult>(commandText, parameters, transaction));
        }

        public TResult QueryFirstOrDefault(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternal(database, (connection, transaction, commandText) => connection.QueryFirstOrDefault<TResult>(commandText, parameters, transaction));
        }

        public Task<TResult> QueryFirstOrDefaultAsync(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternalAsync(database, (connection, transaction, commandText) => connection.QueryFirstOrDefaultAsync<TResult>(commandText, parameters, transaction));
        }

        public IEnumerable<TResult> Query(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternal(database, (connection, transaction, commandText) => connection.Query<TResult>(commandText, parameters, transaction));
        }

        public Task<IEnumerable<TResult>> QueryAsync(DatabaseFacade database, TParams parameters)
        {
            return ExecuteInternalAsync(database, (connection, transaction, commandText) => connection.QueryAsync<TResult>(commandText, parameters, transaction));
        }

        public override string ToString()
        {
            return commandText;
        }
    }
}